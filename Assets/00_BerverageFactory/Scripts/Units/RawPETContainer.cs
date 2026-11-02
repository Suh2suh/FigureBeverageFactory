using TMPro;
using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// - 단순 원자재 Provider로 표현
    /// - Unit으로 지정하지 않음 (execute, stopped 등 상태 미필요)
    /// </summary>
    public class RawPETContainer : MonoBehaviour
    {
        [SerializeField] MaterialScriptableObject _rawPETMaterial;
        [SerializeField] int _leftMaterialCount;
        [SerializeField] TextMeshPro _countDisplayText;

        public void SetMaterial(MaterialScriptableObject material)
        {
            _rawPETMaterial = material;
        }


        public bool TryConsume(out MaterialScriptableObject rawPETMaterial)
        {
            if (_leftMaterialCount <= 0)
            {
                rawPETMaterial = null;
                return false;
            }

            _leftMaterialCount--;
            _countDisplayText.text = _leftMaterialCount.ToString();

            rawPETMaterial = _rawPETMaterial;
            return true;
        }

        private void Awake()
        {
            _countDisplayText.text = _leftMaterialCount.ToString();
        }


    }
}
