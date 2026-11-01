using UnityEngine;

namespace FBFactory
{
    public class Beverage : FactoryMaterial
    {
        [SerializeField, ReadOnly] private MaterialScriptableObject _filling;
        [SerializeField, ReadOnly] private int _maxML = -1;
        [SerializeField, ReadOnly] private int _filledML = 0;
        [SerializeField, ReadOnly] private MaterialScriptableObject _figure;
        [SerializeField, ReadOnly] private Material _label = null;
        [SerializeField, ReadOnly] private string _code = string.Empty;

        [SerializeField] MeshRenderer _meshRenderer;
        [SerializeField] GameObject _lid;
        [SerializeField] Transform _figureSocket;

        public int MaxML => _maxML;
        public int FilledML => _filledML;
        public bool HasCap => _lid.gameObject.activeSelf;
        public Material Label => _label;
        public string Code => _code;
        public Transform FigureSocket => _figureSocket;
        public MaterialScriptableObject Filling => _filling;
        public MaterialScriptableObject Figure => _figure;


        public void SetMaxML(int maxML)
        {
            _maxML = maxML;
        }

        public void SetFilling(MaterialScriptableObject filling, int ML)
        {
            _filling = filling;
            _filledML = ML;
        }

        public void SetCapping(Quaternion rotation /*TODO : v2(des) - Cap Inspection with rotation*/)
        {
            _lid.gameObject.SetActive(true);
        }

        public void SetFigure(MaterialScriptableObject figure)
        {
            _figure = figure;
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
