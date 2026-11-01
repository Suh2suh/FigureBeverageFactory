using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class OrderMaterialSetup
    {
        private Dictionary<MaterialType, List<MaterialScriptableObject>> _materialSetup;

        public void Configure(BOMScriptableObject bom)
        {
            _materialSetup = new();
            foreach (var component in bom.Components)
            {
                var materialType = component.Material.MaterialType;
                if (!_materialSetup.ContainsKey(materialType))
                {
                    _materialSetup[materialType] = new();
                }
                _materialSetup[materialType].Add(component.Material);
            }
        }

        public bool TryGetMaterials(MaterialType type, out IReadOnlyList<MaterialScriptableObject> materials)
        {
            bool hasMaterials = _materialSetup.ContainsKey(type);
            materials = (hasMaterials ? _materialSetup[type] : null);
            return hasMaterials;
        }
    }


    public class LineController : MonoBehaviour
    {
        [SerializeField] private List<UnitBase> _units = new();

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
            InitAllUnits();
        }

        private void InitAllUnits()
        {
            _units = new();
            _typeToUnitData = new();
            foreach (var unit in _units)
            {
                unit.Reset_PackML();

                if (unit is Unit_ConveyerBelt)
                    continue;

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
                Debug.Log("[LineController] orders from MES is empty");
                return;
            }

            _orderSequence = new();
            foreach (OrderData order in orders)
            {
                SetupOrder(order);
                _orderSequence.Add(order);
            }

            // TODO: v4 - 시작 로직 구체화 (order 간 gap 유동적으로 등)
            var firstOrderIndex = _orderSequence[0];
            var parsedFirstOrderData = _orders[firstOrderIndex];
            foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
            {
                if (type is UnitType.ConveyerBelt)
                    continue;

                var unitData = _typeToUnitData[type];
                unitData.ProcessingOrder = firstOrderIndex;
                unitData.Unit.SetupMaterial(parsedFirstOrderData.MaterialSetup);
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
            // 특정 unit에서, 현재 order의 last bottle을 처리 완료 했을 시 lineController에 알림
            // => 해당 unit을 다음 SKU의 BOM에 맞춰 changeOver
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
                _units.AddRange(FindObjectsOfType<UnitBase>(true));
            }

            public void StartAllUnits()
            {
                foreach (UnitBase unit in _units)
                {
                    unit.Reset_PackML();
                    unit.Start_PackML();
                }
            }

            public void StopAllUnits()
            {
                foreach (UnitBase unit in _units)
                    unit.Stop_PackML();
            }

        #endregion

    }
}
