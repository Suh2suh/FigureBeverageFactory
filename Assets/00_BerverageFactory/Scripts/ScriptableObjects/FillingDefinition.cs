using UnityEngine;

namespace FBFactory
{
    [CreateAssetMenu(
        fileName = "FillingDefinition",
        menuName = "FBFactory/Scriptable Objects/Filling Definition")]
    public class FillingDefinition : ScriptableObject
    {
        [SerializeField] private FlavorType _flavorType;
        [SerializeField] private int _fillAmount;

        public FlavorType Flavor => _flavorType;
        public int FillAmount => _fillAmount;
    }
}
