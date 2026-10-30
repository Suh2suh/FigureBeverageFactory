using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class LineController : MonoBehaviour
    {
        [SerializeField] private List<UnitBase> _units = new();
        private Dictionary<UnitType, UnitBase> _typeToUnit = new();
        private IReadOnlyList<OrderData> _orders;
        private Dictionary<OrderData, Dictionary<UnitType, UnitSetup>> _orderToSetup = new();
        private struct UnitSetup
        {
            public bool IsUsed;
            public MaterialScriptableObject Material;
        }


        private void Awake()
        {
            foreach (var unit in _units)
            {
                if (unit is Unit_ConveyerBelt)
                    continue;

                _typeToUnit[unit.UnitType] = unit;
            }
        }

        public void InitOrders(IReadOnlyList<OrderData> orders)
        {
            _orders = orders;

            foreach (var order in _orders)
            {
                SetupOrder(order);
            }
        }

        private void SetupOrder(OrderData order)
        {
            Dictionary<MaterialType, MaterialScriptableObject> _typeToMaterial = new();
            foreach (var component in order.BOM.Components)
            {
                _typeToMaterial[component.Material.MaterialType] = component.Material;
            }
            bool HasMaterial(MaterialType type)
                => _typeToMaterial.ContainsKey(type);
            MaterialScriptableObject GetMaterial(MaterialType type)
                => _typeToMaterial.TryGetValue(type, out var material) ? material : null;

            Dictionary<UnitType, UnitSetup> unitSetups = new();
            // common route
            unitSetups[UnitType.BlowModular] = new() { IsUsed = true, Material = GetMaterial(MaterialType.PET) };
            unitSetups[UnitType.Filler] = new() { IsUsed = true, Material = GetMaterial(MaterialType.Filling) }; 
            unitSetups[UnitType.Capper] = new() { IsUsed = true, Material = GetMaterial(MaterialType.Cap) };
            unitSetups[UnitType.Inspector_FillingCapping] = new() { IsUsed = true, Material = GetMaterial(MaterialType.Filling) };
            unitSetups[UnitType.Labeler] = new() { IsUsed = true, Material = GetMaterial(MaterialType.Label) };
            unitSetups[UnitType.Coder] = new() { IsUsed = true };
            unitSetups[UnitType.Inspector_LabelingCoding] = new() { IsUsed = true, Material = GetMaterial(MaterialType.Label) };
            unitSetups[UnitType.Packager] = new() { IsUsed = true };
            unitSetups[UnitType.Palletizer] = new() { IsUsed = true };
            unitSetups[UnitType.Stretcher] = new() { IsUsed = true };
            // extra route
            unitSetups[UnitType.FigureInserter] = new() { IsUsed = HasMaterial(MaterialType.Figure), Material = GetMaterial(MaterialType.Figure) };
            unitSetups[UnitType.Inspector_Figure] = new() { IsUsed = HasMaterial(MaterialType.Figure), Material = GetMaterial(MaterialType.Figure) };

            _orderToSetup[order] = unitSetups;
        }

        public void OnLastBottleProcessed(Beverage beverage, UnitType unitType)
        {
            // 특정 unit에서, 현재 order의 last bottle을 처리 완료 했을 시 lineController에 알림
            // => 해당 unit을 다음 SKU의 BOM에 맞춰 changeOver


        }

        private bool TryGetUnit<T>(UnitType unitType, out UnitBase unit) where T : UnitBase
            => _typeToUnit.TryGetValue(unitType, out unit);


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
