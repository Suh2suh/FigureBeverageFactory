using UnityEngine;

namespace FBFactory
{
    /// <summary>
    /// 원자재 PET 정의
    /// </summary>
    [CreateAssetMenu( fileName = "PETDefinition", menuName = "FBFactory/Scriptable Objects/PET Definition")]
    public class PETDefinition : ScriptableObject
    {
        [SerializeField] private int _ml;

        [SerializeField] private GameObject _prefab;

        public int ML => _ml;
        public GameObject Prefab => _prefab;
    }
}