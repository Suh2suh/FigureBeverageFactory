using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    [CreateAssetMenu(menuName = "FBFactory/BOM Definition")]
    public class BOMScriptableObject : ScriptableObject
    {
        [SerializeField] private string _skuId;
        [SerializeField] private string _skuName;
        [SerializeField] private string _revision;
        [SerializeField] private float _baseQuantity;
        [SerializeField] private string _baseUnit;
        [SerializeField] private List<ComponentData> _components = new();

        public string SkuId => _skuId;
        public string SkuName => _skuName;
        public string Revision => _revision;
        public float BaseQuantity => _baseQuantity;
        public string BaseUnit => _baseUnit;
        public IReadOnlyList<ComponentData> Components => _components;

        public void SetData(BOMJsonData data, List<ComponentData> components)
        {
            _skuId = data.skuId;
            _skuName = data.skuName;
            _revision = data.revision;
            _baseQuantity = data.baseQuantity;
            _baseUnit = data.baseUnit;
            _components = components;
        }

        [Serializable]
        public class ComponentData
        {
            [SerializeField] private MaterialScriptableObject _material;
            [SerializeField] private float _quantity;
            [SerializeField] private string _unit;

            public MaterialScriptableObject Material => _material;
            public float Quantity => _quantity;
            public string Unit => _unit;

            public ComponentData(MaterialScriptableObject material, float quantity, string unit)
            {
                _material = material;
                _quantity = quantity;
                _unit = unit;
            }
        }
    }

    [Serializable]
    public class BOMJsonData
    {
        public string skuId;
        public string skuName;
        public string revision;
        public float baseQuantity;
        public string baseUnit;
        public List<BOMComponentJsonData> components;
    }

    [Serializable]
    public class BOMComponentJsonData
    {
        public string materialId;
        public float quantity;
        public string unit;
    }
}
