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
        [SerializeField] PETDefinition _rawPETDefinition;
        [SerializeField] int _leftMaterialCount;
        [SerializeField] TextMeshPro _countDisplayText;


        public bool TryConsume(out PETDefinition rawPET)
        {
            if (_leftMaterialCount <= 0)
            {
                rawPET = null;
                return false;
            }

            _leftMaterialCount--;
            _countDisplayText.text = _leftMaterialCount.ToString();

            rawPET = _rawPETDefinition;
            return true;
        }

        private void Awake()
        {
            _countDisplayText.text = _leftMaterialCount.ToString();
        }


    }
}
