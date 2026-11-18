using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class OrderMaterialSetup
    {
        private Dictionary<MaterialType, List<MaterialScriptableObject>> _materialTypeToMaterials;
        private Dictionary<MaterialType, float> _materialTypeToBeverageQuantity;
        private Dictionary<MaterialType, string> _materialTypeToBeverageUnit;

        public void Configure(BOMScriptableObject bom)
        {
            _materialTypeToMaterials = new();
            _materialTypeToBeverageQuantity = new();
            _materialTypeToBeverageUnit = new();
            foreach (var bomComponent in bom.Components)
            {
                MaterialType materialType = bomComponent.Material.MaterialType;
                if (!_materialTypeToMaterials.ContainsKey(materialType))
                {
                    _materialTypeToMaterials[materialType] = new();
                }
                _materialTypeToMaterials[materialType].Add(bomComponent.Material);
                _materialTypeToBeverageQuantity[materialType] = bomComponent.Quantity;
                _materialTypeToBeverageUnit[materialType] = bomComponent.Unit;
            }
        }

        public bool TryGetMaterials(MaterialType materialType, out IReadOnlyList<MaterialScriptableObject> materials)
        {
            bool hasMaterials = _materialTypeToMaterials.ContainsKey(materialType);
            materials = (hasMaterials ? _materialTypeToMaterials[materialType] : null);
            return hasMaterials;
        }

        public float GetBeverageQuantity(MaterialType materialType)
        {
            float quantity = _materialTypeToBeverageQuantity[materialType];
            string beverageUnit = _materialTypeToBeverageUnit[materialType];

            if (beverageUnit == "L")
                return quantity * 1000f;

            return quantity;
        }
    }


    public class LineController : MonoBehaviour
    {
        [SerializeField] private List<ProcessingUnit> _units = new();
        [SerializeField] private ProductionPath _normalProductionPath;
        [SerializeField] private ProductionPath _figureProductionPath;

        private Dictionary<UnitType, ProcessingUnitData> _unitTypeToProcessingUnitData = new();
        private class ProcessingUnitData
        {
            public ProcessingUnit Unit;
            public ProductionOrderData ProcessingOrder;
            public PackMLState State => Unit.State;
        }

        private List<ProductionOrderData> _orderSequence = new();
        private Dictionary<string, ProductionOrderData> _orderIdToProductionOrderData = new();
        public class ProductionOrderData
        {
            public string OrderId;
            public MESOrderData SourceOrder;
            private HashSet<UnitType> _usedUnits = new()
            {   // common route for Figure Beverage Factory
                UnitType.BlowModular, UnitType.Filler, UnitType.Capper, UnitType.Inspector_FillingCapping,
                UnitType.Labeler, UnitType.Coder, UnitType.Inspector_LabelingCoding,
                UnitType.Packager, UnitType.Palletizer, UnitType.Stretcher,
            };

            public bool UsesUnit(UnitType unitType)
                => _usedUnits.Contains(unitType);
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
            _unitTypeToProcessingUnitData = new();
            foreach (ProcessingUnit processingUnit in _units)
            {
                processingUnit.SetChangeOverCompleteCallback(OnEndChangeOver);
                processingUnit.SetLineController(this);

                _unitTypeToProcessingUnitData[processingUnit.UnitType] = new()
                {
                    Unit = processingUnit,
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
            foreach (ProcessingUnitData processingUnitData in _unitTypeToProcessingUnitData.Values)
            {
                if (CanChangeover(processingUnitData.Unit))
                    ChangeOverToNextOrder(processingUnitData.Unit.UnitType);
            }
        }

        public void ConfigureOrders(IReadOnlyList<MESOrderData> mesOrderDataList)
        {
            if (mesOrderDataList.Count == 0)
            {
                Debug.Log("[LineController] orders from MES is empty".Color(Color.red));
                return;
            }
            Debug.Log("[LineController] Start Parsing Orders from MES...".Color(Color.blue));
            AssignProductionSequence(mesOrderDataList);

            // MES에서 내려온 order데이터를 미리 parsing (unit order 상태를 바로 전환할 수 있게)
            _orderSequence = new();
            foreach (MESOrderData orderData in mesOrderDataList)
            {
                SetupOrder(orderData);
                _orderSequence.Add(_orderIdToProductionOrderData[orderData.OrderId]);
                Debug.Log($"[LineController] Parsed : Order #{orderData.OrderId} - '{orderData.BOM.SkuName}' / {orderData.TargetQuantity} EA");
            }

            // TODO: v4 - 시작 로직 구체화 (order 간 gap 유동적으로 등)
            // 1. Order Production 시작
            // 1-a. first order에 사용되는 unit들에 대하여, material setup(changeOver)
            Debug.Log($"[LineController] Changing-Over all Units' setting...".Color(Color.blue));
            ProductionOrderData firstProductionOrderData = _orderSequence[0];
            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
            {
                if (unitType is UnitType.ConveyerBelt ||
                    !firstProductionOrderData.UsesUnit(unitType))
                    continue;

                ProcessingUnitData processingUnitData = _unitTypeToProcessingUnitData[unitType];
                processingUnitData.ProcessingOrder = firstProductionOrderData;
                processingUnitData.Unit.SetMode(PackMLMode.Production);
                processingUnitData.Unit.SetupMaterial(firstProductionOrderData.MaterialSetup);
            }

            // 1-b. 첫 Order가 사용하는 ProductionPath만 작동
            Debug.Log($"[LineController] Starting first Order ProductionPath...".Color(Color.blue));
            StartConveyersForOrder(firstProductionOrderData);
        }

        private void SetupOrder(MESOrderData orderData)
        {
            ProductionOrderData productionOrderData = new ProductionOrderData
            {
                OrderId = orderData.OrderId,
                SourceOrder = orderData
            };
            productionOrderData.Configure(orderData.BOM);
            productionOrderData.ProductionPath = productionOrderData.MaterialSetup.TryGetMaterials(MaterialType.Figure, out _)
                ? _figureProductionPath
                : _normalProductionPath;
            _orderIdToProductionOrderData[orderData.OrderId] = productionOrderData;
        }

        public ProductionPath GetProductionPath(ProductionOrderData productionOrderData)
        {
            return productionOrderData?.ProductionPath;
        }

        public bool TryGetPrevUnit(ProcessingUnit processingUnit, ProductionOrderData productionOrderData, out ProcessingUnit previousUnit)
        {
            return TryGetConnection(processingUnit, productionOrderData, ConnectionType.Production, true, out previousUnit);
        }

        public bool TryGetNextUnit(ProcessingUnit processingUnit, ProductionOrderData productionOrderData, out ProcessingUnit nextUnit)
        {
            return TryGetConnection(processingUnit, productionOrderData, ConnectionType.Production, false, out nextUnit);
        }

        public bool TryInsertMaterial(ProcessingUnit processingUnit, FactoryMaterial material, ConnectionType connectionType)
        {
            LineConnection connection = FindOutgoingConnection(processingUnit, connectionType);

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

        public IReadOnlyList<LineConnection> GetIncomingConnections(ProcessingUnit processingUnit, ProductionOrderData productionOrderData = null)
        {
            return GetConnections(processingUnit, true, productionOrderData);
        }

        public IReadOnlyList<LineConnection> GetOutgoingConnections(ProcessingUnit processingUnit, ProductionOrderData productionOrderData = null)
        {
            return GetConnections(processingUnit, false, productionOrderData);
        }

        private bool TryGetConnection(ProcessingUnit processingUnit, ProductionOrderData productionOrderData, ConnectionType connectionType, bool isIncoming, out ProcessingUnit connectedUnit)
        {
            ProductionPath productionPath = GetProductionPath(productionOrderData);
            LineConnection lineConnection = productionPath?.Connections.Find(connection =>
                connection.Type == connectionType && (isIncoming ? connection.NextUnit == processingUnit : connection.PrevUnit == processingUnit));

            connectedUnit = lineConnection == null
                ? null
                : isIncoming ? lineConnection.PrevUnit : lineConnection.NextUnit;
            return connectedUnit != null;
        }

        private LineConnection FindOutgoingConnection(ProcessingUnit processingUnit, ConnectionType connectionType)
        {
            if (!_unitTypeToProcessingUnitData.TryGetValue(processingUnit.UnitType, out ProcessingUnitData processingUnitData))
                return null;

            ProductionPath productionPath = GetProductionPath(processingUnitData.ProcessingOrder);
            return productionPath?.Connections.Find(connection =>
                connection.Type == connectionType && connection.PrevUnit == processingUnit);
        }

        private IReadOnlyList<LineConnection> GetConnections(ProcessingUnit processingUnit, bool isIncoming, ProductionOrderData productionOrderData)
        {
            ProductionPath productionPath = productionOrderData == null ? null : GetProductionPath(productionOrderData);
            List<LineConnection> connections = productionPath == null
                ? new List<LineConnection>()
                : productionPath.Connections.FindAll(connection => isIncoming ? connection.NextUnit == processingUnit : connection.PrevUnit == processingUnit);
            return connections;
        }


        public void OnProductProduced(ProductionOrderData productionOrderData)
        {
            MES.Instance.OnProduced(productionOrderData.OrderId);
        }

        public void OnProductReject(ProductionOrderData productionOrderData)
        {
            MES.Instance.OnRejected(productionOrderData.OrderId);
        }


        public void ChangeOverToNextOrder(UnitType targetUnitType)
        {
            if (!_unitTypeToProcessingUnitData.TryGetValue(targetUnitType, out ProcessingUnitData processingUnitData) ||
                processingUnitData.ProcessingOrder == null)
                return;

            int currentOrderIndex = _orderSequence.IndexOf(processingUnitData.ProcessingOrder);
            int nextOrderIndex = currentOrderIndex + 1;

            if (nextOrderIndex >= _orderSequence.Count)
                return;

            ProductionOrderData nextProductionOrder = _orderSequence[nextOrderIndex];
            ProcessingUnit processingUnit = processingUnitData.Unit;

            ApplyConveyersForChangeOver(processingUnit, processingUnitData.ProcessingOrder, nextProductionOrder);

            if (!nextProductionOrder.UsesUnit(targetUnitType))
            {
                processingUnit.Stop_PackML();
                processingUnitData.ProcessingOrder = nextProductionOrder;
                return;
            }

            processingUnitData.ProcessingOrder = nextProductionOrder;

            processingUnit.Stop_PackML();
            processingUnit.SetupMaterial(nextProductionOrder.MaterialSetup);
        }

        public ProductionOrderData GetCurrentOrder(ProcessingUnit processingUnit)
        {
            return _unitTypeToProcessingUnitData.TryGetValue(processingUnit.UnitType, out ProcessingUnitData processingUnitData)
                ? processingUnitData.ProcessingOrder
                : null;
        }

        public bool IsStarved(ProcessingUnit processingUnit)
        {
            ProductionOrderData currentProductionOrder = GetCurrentOrder(processingUnit);

            if (currentProductionOrder == null)
                return true;

            foreach (LineConnection connection in GetIncomingConnections(processingUnit, currentProductionOrder))
            {
                if (connection.Conveyers.Exists(conveyer =>
                    conveyer.GetCapacityStatus() != CapacityState.Empty))
                    return false;
            }

            return true;
        }

        public bool IsPreviousOrderFlowCompleted(ProcessingUnit processingUnit)
        {
            ProductionOrderData currentProductionOrder = GetCurrentOrder(processingUnit);

            if (currentProductionOrder == null ||
                !TryGetPrevUnit(processingUnit, currentProductionOrder, out ProcessingUnit previousUnit))
                return true;

            return GetCurrentOrder(previousUnit) != currentProductionOrder;
        }

        public bool CanChangeover(ProcessingUnit processingUnit)
        {
            if (!_unitTypeToProcessingUnitData.TryGetValue(processingUnit.UnitType, out ProcessingUnitData processingUnitData) ||
                processingUnitData.ProcessingOrder == null ||
                processingUnit.State != PackMLState.Execute ||
                processingUnit.IsProcessing ||
                !IsStarved(processingUnit))
                return false;

            if (!IsPreviousOrderFlowCompleted(processingUnit))
                return false;

            int currentOrderIndex = _orderSequence.IndexOf(processingUnitData.ProcessingOrder);
            return currentOrderIndex >= 0 && currentOrderIndex + 1 < _orderSequence.Count;
        }

        public void OnEndChangeOver(ProcessingUnit processingUnit)
        {
            Debug.Log($"[LineController] {processingUnit.name} ended changeOver for Order ID : {_unitTypeToProcessingUnitData[processingUnit.UnitType].ProcessingOrder.OrderId}");
            processingUnit.Reset_PackML();
            processingUnit.Start_PackML();
        }


        private bool TryGetUnit(UnitType unitType, out ProcessingUnit processingUnit)
        {
            bool hasUnit = _unitTypeToProcessingUnitData.TryGetValue(unitType, out ProcessingUnitData processingUnitData);
            processingUnit = hasUnit ? processingUnitData.Unit : null;
            return hasUnit;
        }

        private bool TryGetUnit<T>(UnitType unitType, out T processingUnit) where T : ProcessingUnit
        {
            bool hasUnit = _unitTypeToProcessingUnitData.TryGetValue(unitType, out ProcessingUnitData processingUnitData);
            processingUnit = hasUnit ? processingUnitData.Unit as T : null;
            return hasUnit;
        }

        private void StartConveyersForOrder(ProductionOrderData productionOrderData)
        {
            ProductionPath productionPath = GetProductionPath(productionOrderData);

            if (productionPath == null)
                return;

            HashSet<Unit_ConveyerBelt> startedConveyers = new();

            foreach (LineConnection connection in productionPath.Connections)
            {
                foreach (Unit_ConveyerBelt conveyer in connection.Conveyers)
                {
                    if (!startedConveyers.Add(conveyer))
                        continue;

                    conveyer.Reset_PackML();
                    conveyer.Start_PackML();
                }

                foreach (ConverterDirectionSetting converterDirectionSetting in connection.ConverterDirections)
                {
                    converterDirectionSetting.Converter.SetMoveDirection(converterDirectionSetting.LocalDirection);
                }
            }
        }

        protected virtual void AssignProductionSequence(IReadOnlyList<MESOrderData> orderDataList)
        {
            for (int sequence = 0; sequence < orderDataList.Count; sequence++)
                orderDataList[sequence].ProductionSequence = sequence;
        }

        private void ApplyConveyersForChangeOver(
            ProcessingUnit processingUnit,
            ProductionOrderData currentProductionOrder,
            ProductionOrderData nextProductionOrder)
        {
            StopOutgoingConveyersForOrder(processingUnit, currentProductionOrder);
            StartOutgoingConveyersForOrder(processingUnit, nextProductionOrder);
        }

        private void StartOutgoingConveyersForOrder(
            ProcessingUnit upstreamProcessingUnit,
            ProductionOrderData productionOrderData)
        {
            ProductionPath productionPath = GetProductionPath(productionOrderData);

            if (productionPath == null)
                return;

            foreach (LineConnection connection in productionPath.Connections)
            {
                if (connection.Type != ConnectionType.Production ||
                    connection.PrevUnit != upstreamProcessingUnit)
                    continue;

                StartConveyersInConnection(connection);
            }
        }

        private void StopOutgoingConveyersForOrder(
            ProcessingUnit upstreamProcessingUnit,
            ProductionOrderData productionOrderData)
        {
            ProductionPath productionPath = GetProductionPath(productionOrderData);

            if (productionPath == null)
                return;

            HashSet<Unit_ConveyerBelt> stoppedConveyers = new();

            foreach (LineConnection connection in productionPath.Connections)
            {
                if (connection.PrevUnit != upstreamProcessingUnit)
                    continue;

                foreach (Unit_ConveyerBelt conveyer in connection.Conveyers)
                {
                    if (!stoppedConveyers.Add(conveyer))
                        continue;

                    conveyer.Stop_PackML();
                }
            }
        }

        private void StartConveyersInConnection(LineConnection connection)
        {
            foreach (Unit_ConveyerBelt conveyer in connection.Conveyers)
            {
                conveyer.Reset_PackML();
                conveyer.Start_PackML();
            }

            foreach (ConverterDirectionSetting converterDirectionSetting in connection.ConverterDirections)
            {
                converterDirectionSetting.Converter.SetMoveDirection(converterDirectionSetting.LocalDirection);
            }
        }


        #region Utility
            public void CollectAllUnits()
            {
                _units.Clear();

                foreach (ProcessingUnit processingUnit in FindObjectsByType<ProcessingUnit>(FindObjectsInactive.Include))
                {
                    _units.Add(processingUnit);
                }
            }

        #endregion

    }
}
