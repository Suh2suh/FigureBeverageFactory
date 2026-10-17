using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// TODO : For whatIf - vision 검사 시 figure type fault 내기 위해서 component 부착
    /// </summary>
    public class Figure : MonoBehaviour
    {
        [SerializeField] FigureType _type = FigureType.None;
        public FigureType Type => _type;
    }
}
