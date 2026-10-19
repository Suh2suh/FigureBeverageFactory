using UnityEngine;

namespace FBFactory
{
    /// <summary>
    /// 음료 완성품
    /// </summary>
    [CreateAssetMenu(fileName = "BeverageDefinition", menuName = "FBFactory/Scriptable Objects/Beverage Definition")]
    public class BeverageDefinition : ScriptableObject
    {
        [SerializeField] private PETDefinition _petDefinition;
        [SerializeField] private FillingDefinition _fillingDefinition;
        [SerializeField] private FigureType _figureType = FigureType.None;
        [SerializeField] private Material _label;

        public PETDefinition PET => _petDefinition;
        public FillingDefinition Filling => _fillingDefinition;
        public FigureType FigureType => _figureType;
        public Material Label => _label;
    }
}