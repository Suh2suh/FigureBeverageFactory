using FBFactory;
using Unity.Collections;
using UnityEngine;

namespace FBFactory
{
    public class Beverage : MonoBehaviour
    {
        [SerializeField, ReadOnly] private FillingType _filling;
        [SerializeField, ReadOnly] private int _maxML = -1;
        [SerializeField, ReadOnly] private int _filledML = 0;
        [SerializeField, ReadOnly] private Material _label = null;
        [SerializeField, ReadOnly] private string _code = string.Empty;

        [SerializeField] MeshRenderer _meshRenderer;


        public void SetMaxML(int maxML)
        {
            _maxML = maxML;
        }

        public void SetFilling(FillingType filling, int ML)
        {
            _filling = filling; 
            _filledML = ML;
        }

        public void SetLabel(Material label)
        {
            _meshRenderer.material = label;
        }

        public void SetCode(string code)
        {
            _code = code;
        }


    }
}
