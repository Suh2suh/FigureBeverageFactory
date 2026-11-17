using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class OrderMaterialSetup
    {
        private Dictionary<MaterialType, List<MaterialScriptableObject>> _materialSetup;
        private Dictionary<MaterialType, float> _beverageQuantities;
        private Dictionary<MaterialType, string> _beverageUnits;

        public void Configure(BOMScriptableObject bom)
        {
            _materialSetup = new();
            _beverageQuantities = new();
            _beverageUnits = new();
            foreach (var component in bom.Components)
            {
                var materialType = component.Material.MaterialType;
                if (!_materialSetup.ContainsKey(materialType))
                {
                    _materialSetup[materialType] = new();
                }
                _materialSetup[materialType].Add(component.Material);
                _beverageQuantities[materialType] = component.Quantity;
                _beverageUnits[materialType] = component.Unit;
            }
        }

        public bool TryGetMaterials(MaterialType type, out IReadOnlyList<MaterialScriptableObject> materials)
        {
            bool hasMaterials = _materialSetup.ContainsKey(type);
            materials = (hasMaterials ? _materialSetup[type] : null);
            return hasMaterials;
        }

        public float GetBeverageQuantity(MaterialType type)
        {
            float quantity = _beverageQuantities[type];
            string unit = _beverageUnits[type];

            if (unit == "L")
                return quantity * 1000f;

            return quantity;
        }
    }


    public class LineController : MonoBehaviour
    {
        [SerializeField] private List<ProcessingUnit> _units = new();
        [SerializeField] private ProductionPath _normalProductionPath;
        [SerializeField] private ProductionPath _figureProductionPath;

        private Dictionary<UnitType, UnitData> _typeToUnitData = new();
        private class UnitData
        {
            public ProcessingUnit Unit;
            public OrderData ProcessingOrder;
            public PackMLState State => Unit.State;
        }

        private List<OrderData> _orderSequence = new();
        private Dictionary<OrderData, ParsedOrderData> _orders = new();
        private class ParsedOrderData
        {
            public HashSet<UnitType> _usedUnits = new()
            {   // common route for Figure Beverage Factory
                UnitType.BlowModular, UnitType.Filler, UnitType.Capper, UnitType.Inspector_FillingCapping,
                UnitType.Labeler, UnitType.Coder, UnitType.Inspector_LabelingCoding,
                UnitType.Packager, UnitType.Palletizer, UnitType.Stretcher,
            };
            public OrderMaterialSetup MaterialSetup { get; private set; }
            public ProductionPath ProductionPath { get; set; }

            public void Configure(BOMScriptableObject bom)
            {
                MaterialSetup = new();
                MaterialSetup.Configure(bom);
                bool hasFigure = MaterialSetup.TryGetMaterials(MaterialType.Figure, out _);
                if (hasFigure)
                {
                    _usedUnits.Add(UnitType.FigureInserter);
                    _usedUnits.Add(UnitType.Inspector_Figure);
                }
            }
        }


        private void Awake()
        {
            // CollectAllUnits();
            InitAllUnits();
        }

        private void OnValidate()
        {
        }

        private void InitAllUnits()
        {
            _typeToUnitData = new();
            foreach (var unit in _units)
            {
                unit.SetChangeOverCompleteCallback(OnEndChangeOver);
                unit.SetLineController(this);

                _typeToUnitData[unit.UnitType] = new()
                {
                    Unit = unit,
                    ProcessingOrder = null
                };
            }
        }


        private void Update()
        {
            MonitorUnits();
        }

        private void MonitorUnits()
        {
            foreach (UnitData unitData in _typeToUnitData.Values)
            {
                if (CanChangeover(unitData.Unit))
                    ChangeOverToNextOrder(unitData.Unit.UnitType);
            }
        }

        public void ConfigureOrders(IReadOnlyList<OrderData> orders)
        {
            if (orders.Count == 0)
            {
                Debug.Log("[LineController] orders from MES is empty".Color(Color.red));
                return;
            }
            Debug.Log("[LineController] Start Parsing Orders from MES...".Color(Color.blue));

            // MES에서 내려온 order데이터를 미리 parsing (unit order 상태를 바로 전환할 수 있게)
            _orderSequence = new();
            foreach (OrderData order in orders)
            {
                SetupOrder(order);
                _orderSequence.Add(order);
                Debug.Log($"[LineController] Parsed : Order #{order.ID} - '{order.BOM.SkuName}' / {order.TargetQuantity} EA");
            }

            // TODO: v4 - 시작 로직 구체화 (order 간 gap 유동적으로 등)
            // 1. Order Production 시작
            // 1-a. first order에 사용되는 unit들에 대하여, material setup(changeOver)
            Debug.Log($"[LineController] Changing-Over all Units' setting...".Color(Color.blue));
            var firstOrderIndex = _orderSequence[0];
            var firstOrderData = _orders[firstOrderIndex];
            foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
            {
                if (type is UnitType.ConveyerBelt ||
                    !firstOrderData._usedUnits.Contains(type))
                    continue;

                var unitData = _typeToUnitData[type];
                unitData.ProcessingOrder = firstOrderIndex;
                unitData.Unit.SetMode(PackMLMode.Production);
                unitData.Unit.SetupMaterial(firstOrderData.MaterialSetup);
            }
            // 1-b. ConveyerBelt 작동
            Debug.Log($"[LineController] Starting all ConveyerBelts...".Color(Color.blue));
            StartConveyers(_normalProductionPath);
            StartConveyers(_figureProductionPath);
        }

        private void SetupOrder(OrderData order)
        {
            ParsedOrderData parsedOrderData = new ParsedOrderData();
            parsedOrderData.Configure(order.BOM);
            parsedOrderData.ProductionPath = parsedOrderData.MaterialSetup.TryGetMaterials(MaterialType.Figure, out _)
                ? _figureProductionPath
                : _normalProductionPath;
            _orders[order] = parsedOrderData;
        }

        public ProductionPath GetProductionPath(OrderData order)
        {
            return _orders.TryGetValue(order, out ParsedOrderData parsedOrderData)
                ? parsedOrderData.ProductionPath
                : null;
        }

        public bool TryGetPrevUnit(ProcessingUnit unit, OrderData order, out ProcessingUnit prevUnit)
        {
            return TryGetConnection(unit, order, ConnectionType.Production, true, out prevUnit);
        }

        public bool TryGetNextUnit(ProcessingUnit unit, OrderData order, out ProcessingUnit nextUnit)
        {
            return TryGetConnection(unit, order, ConnectionType.Production, false, out nextUnit);
        }

        public bool TryInsertMaterial(ProcessingUnit unit, FactoryMaterial material, ConnectionType type)
        {
            LineConnection connection = FindOutgoingConnection(unit, type);

            if (connection == null)
                return false;

            foreach (Unit_ConveyerBelt conveyer in connection.Conveyers)
            {
                if (conveyer.GetCapacityStatus() != CapacityState.Full &&
                    conveyer.InsertMaterial(material))
                    return true;
            }

            return false;
        }

        public IReadOnlyList<LineConnection> GetIncomingConnections(ProcessingUnit unit, OrderData order = null)
        {
            return GetConnections(unit, true, order);
        }

        public IReadOnlyList<LineConnection> GetOutgoingConnections(ProcessingUnit unit, OrderData order = null)
        {
            return GetConnections(unit, false, order);
        }

        private bool TryGetConnection(ProcessingUnit unit, OrderData order, ConnectionType type, bool incoming, out ProcessingUnit connectedUnit)
        {
            ProductionPath path = GetProductionPath(order);
            LineConnection connection = path?.Connections.Find(item =>
                item.Type == type && (incoming ? item.NextUnit == unit : item.PrevUnit == unit));

            connectedUnit = connection == null
                ? null
                : incoming ? connection.PrevUnit : connection.NextUnit;
            return connectedUnit != null;
        }

        private LineConnection FindOutgoingConnection(ProcessingUnit unit, ConnectionType type)
        {
            if (!_typeToUnitData.TryGetValue(unit.UnitType, out UnitData unitData))
                return null;

            ProductionPath path = GetProductionPath(unitData.ProcessingOrder);
            return path?.Connections.Find(item =>
                item.Type == type && item.PrevUnit == unit);
        }

        private IReadOnlyList<LineConnection> GetConnections(ProcessingUnit unit, bool incoming, OrderData order)
        {
            ProductionPath path = order == null ? null : GetProductionPath(order);
            List<LineConnection> connections = path == null
                ? new List<LineConnection>()
                : path.Connections.FindAll(item => incoming ? item.NextUnit == unit : item.PrevUnit == unit);
            return connections;
        }


        public void OnProductProduced(OrderData order)
        {
            MES.Instance.OnProduced(order.ID);
        }

        public void OnProductReject(OrderData order)
        {
            MES.Instance.OnRejected(order.ID);
        }


        public void ChangeOverToNextOrder(UnitType targetUnit)
        {
            if (!_typeToUnitData.TryGetValue(targetUnit, out UnitData unitData) ||
                unitData.ProcessingOrder == null)
                return;

            int currentOrderIndex = _orderSequence.IndexOf(unitData.ProcessingOrder);
            int nextOrderIndex = currentOrderIndex + 1;

            if (nextOrderIndex >= _orderSequence.Count)
                return;

            OrderData nextOrder = _orderSequence[nextOrderIndex];
            unitData.ProcessingOrder = nextOrder;

            unitData.Unit.Stop_PackML();
            unitData.Unit.SetupMaterial(_orders[nextOrder].MaterialSetup);
        }

        public OrderData GetCurrentOrder(ProcessingUnit unit)
        {
            return _typeToUnitData.TryGetValue(unit.UnitType, out UnitData unitData)
                ? unitData.ProcessingOrder
                : null;
        }

        public bool IsStarved(ProcessingUnit unit)
        {
            OrderData currentOrder = GetCurrentOrder(unit);

            if (currentOrder == null)
                return true;

            foreach (LineConnection connection in GetIncomingConnections(unit, currentOrder))
            {
                if (connection.Conveyers.Exists(conveyer =>
                    conveyer.GetCapacityStatus() != CapacityState.Empty))
                    return false;
            }

            return true;
        }

        public bool IsPreviousOrderFlowCompleted(ProcessingUnit unit)
        {
            OrderData currentOrder = GetCurrentOrder(unit);

            if (currentOrder == null ||
                !TryGetPrevUnit(unit, currentOrder, out ProcessingUnit prevUnit))
                return true;

            return GetCurrentOrder(prevUnit) != currentOrder;
        }

        public bool CanChangeover(ProcessingUnit unit)
        {
            if (!_typeToUnitData.TryGetValue(unit.UnitType, out UnitData unitData) ||
                unitData.ProcessingOrder == null ||
                unit.State != PackMLState.Execute ||
                unit.IsProcessing ||
                !IsStarved(unit))
                return false;

            if (!IsPreviousOrderFlowCompleted(unit))
                return false;

            int currentOrderIndex = _orderSequence.IndexOf(unitData.ProcessingOrder);
            return currentOrderIndex >= 0 && currentOrderIndex + 1 < _orderSequence.Count;
        }

        public void OnEndChangeOver(ProcessingUnit unit)
        {
            Debug.Log($"[LineController] {unit.name} ended changeOver for Order ID : {_typeToUnitData[unit.UnitType].ProcessingOrder.ID}");
            unit.Reset_PackML();
            unit.Start_PackML();
        }


        private bool TryGetUnit(UnitType unitType, out ProcessingUnit unit)
        {
            bool hasUnit = _typeToUnitData.TryGetValue(unitType, out var unitData);
            unit = hasUnit ? unitData.Unit : null;
            return hasUnit;
        }

        private bool TryGetUnit<T>(UnitType unitType, out T unit) where T : ProcessingUnit
        {
            bool hasUnit = _typeToUnitData.TryGetValue(unitType, out var unitData);
            unit = hasUnit ? unitData.Unit as T : null;
            return hasUnit;
        }

        private void StartConveyers(ProductionPath path)
        {
            if (path == null)
                return;

            HashSet<Unit_ConveyerBelt> startedConveyers = new();

            foreach (LineConnection connection in path.Connections)
            {
                foreach (Unit_ConveyerBelt conveyer in connection.Conveyers)
                {
                    if (!startedConveyers.Add(conveyer))
                        continue;

                    conveyer.Reset_PackML();
                    conveyer.Start_PackML();
                }

                foreach (ConverterDirectionSetting setting in connection.ConverterDirections)
                {
                    setting.Converter.SetMoveDirection(setting.LocalDirection);
                }
            }
        }


        #region Utility
            public void CollectAllUnits()
            {
                _units.Clear();

                foreach (ProcessingUnit unit in FindObjectsOfType<ProcessingUnit>(true))
                {
                    _units.Add(unit);
                }
            }

        #endregion

    }
}
