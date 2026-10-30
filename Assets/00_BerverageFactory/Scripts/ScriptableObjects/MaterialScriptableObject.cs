using UnityEngine;

namespace FBFactory
{
    [CreateAssetMenu(menuName = "FBFactory/Material Definition")]
    public class MaterialScriptableObject : ScriptableObject
    {
        [SerializeField] private string _materialId;
        [SerializeField] private string _materialName;
        [SerializeField] private MaterialType _materialType;
        [SerializeField] private string _baseUnit;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Material _material;

        public string MaterialId => _materialId;
        public string MaterialName => _materialName;
        public MaterialType MaterialType => _materialType;
        public string BaseUnit => _baseUnit;
        public GameObject Prefab => _prefab;
        public Material Material => _material;

        public void SetData(string materialId, string materialName, MaterialType materialType, string baseUnit)
        {
            _materialId = materialId;
            _materialName = materialName;
            _materialType = materialType;
            _baseUnit = baseUnit;
        }
    }
}
