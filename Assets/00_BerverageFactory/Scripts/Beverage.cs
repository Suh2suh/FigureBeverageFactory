using FBFactory;
using Unity.Collections;
using UnityEngine;

namespace FBFactory
{
    public class Beverage : FactoryMaterial
    {
        [SerializeField, ReadOnly] private int _maxML = -1;
        [SerializeField, ReadOnly] private int _filledML = 0;
        [SerializeField, ReadOnly] private FlavorType _fillingFalvor;
        [SerializeField, ReadOnly] private Material _label = null;
        [SerializeField, ReadOnly] private string _code = string.Empty;

        [SerializeField] MeshRenderer _meshRenderer;
        [SerializeField] GameObject _lid;
        [SerializeField] Transform _figureSocket;

        public int MaxML => _maxML;
        public int FilledML => _filledML;
        public FlavorType Flavor => _fillingFalvor;
        public bool HasCap => _lid.gameObject.activeSelf;
        public Material Label => _label;
        public string Code => _code;
        public Transform FigureSocket => _figureSocket;


        public void SetMaxML(int maxML)
        {
            _maxML = maxML;
        }

        public void SetFilling(FlavorType filling, int ML)
        {
            _fillingFalvor = filling; 
            _filledML = ML;
        }

        public void SetCapping(Quaternion rotation /*추후 Cap Inspection 사용*/)
        {
            _lid.gameObject.SetActive(true);
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
