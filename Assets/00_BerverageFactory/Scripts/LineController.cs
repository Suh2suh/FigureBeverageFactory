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
        [SerializeField] private List<UnitBase> _units = new();
        [SerializeField] private List<GroupConveyerBelt> _conveyerGroup = new();

        /// <summary>
        /// conveyer belt is not included (∵ get conveyerbelt through unit, linked upstream/downstream belt)
        /// </summary>
        private Dictionary<UnitType, UnitData> _typeToUnitData = new();
        private class UnitData
        {
            public UnitBase Unit;
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
            for (int i = _units.Count - 1; i >= 0; i--)
            {
                if (_units[i] is Unit_ConveyerBelt)
                    _units.RemoveAt(i);
            }
        }

        private void InitAllUnits()
        {
            _typeToUnitData = new();
            foreach (var unit in _units)
            {
                if (unit is Unit_ConveyerBelt)
                    continue;

                unit.SetChangeOverCompleteCallback(OnEndChangeOver);

                _typeToUnitData[unit.UnitType] = new()
                {
                    Unit = unit,
                    ProcessingOrder = null
                };
            }
        }


        public void InitOrders(IReadOnlyList<OrderData> orders)
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
                unitData.Unit.SetupMaterial(firstOrderData.MaterialSetup);
            }
            // 1-b. ConveyerBelt 작동
            Debug.Log($"[LineController] Starting all ConveyerBelts...".Color(Color.blue));
            foreach (var conveyerGroup in _conveyerGroup)
            {
                conveyerGroup.Start_PackML();
            }
        }

        private void SetupOrder(OrderData order)
        {
            ParsedOrderData parsedOrderData = new ParsedOrderData();
            parsedOrderData.Configure(order.BOM);
            _orders[order] = parsedOrderData;
        }


        public void OnProductProduced(OrderData order)
        {
            MES.Instance.OnProduced(order.ID);
        }

        public void OnProductReject(OrderData order)
        {
            MES.Instance.OnRejected(order.ID);
        }


        public void OnLastBottleProcessed(Beverage beverage, UnitType unitType)
        {
            if (!_typeToUnitData.TryGetValue(unitType, out UnitData unitData) ||
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

        public void OnEndChangeOver(UnitBase unit)
        {
            Debug.Log($"[LineController] {unit.name} ended changeOver for Order ID : {_typeToUnitData[unit.UnitType].ProcessingOrder.ID}");
            unit.Reset_PackML();
            unit.Start_PackML();
        }


        private bool TryGetUnit(UnitType unitType, out UnitBase unit)
        {
            bool hasUnit = _typeToUnitData.TryGetValue(unitType, out var unitData);
            unit = hasUnit ? unitData.Unit : null;
            return hasUnit;
        }

        private bool TryGetUnit<T>(UnitType unitType, out T unit) where T : UnitBase
        {
            bool hasUnit = _typeToUnitData.TryGetValue(unitType, out var unitData);
            unit = hasUnit ? unitData.Unit as T : null;
            return hasUnit;
        }


        #region Utility
            public void CollectAllUnits()
            {
                _units.Clear();

                foreach (UnitBase unit in FindObjectsOfType<UnitBase>(true))
                {
                    if (unit is Unit_ConveyerBelt)
                        continue;

                    _units.Add(unit);
                }
            }

            public void CollectAllConveyerGroup()
            {
                _conveyerGroup.Clear();
                _conveyerGroup.AddRange(FindObjectsOfType<GroupConveyerBelt>(true));
            }

        #endregion

    }
}
