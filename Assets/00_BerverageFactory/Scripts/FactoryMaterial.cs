using UnityEngine;

namespace FBFactory
{
    public class FactoryMaterial : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField, ReadOnly] private UnitBase _owner;

        public Rigidbody Rigidbody => _rigidbody;


        public UnitBase Owner => _owner;


        public void Own(UnitBase owner)
        {
            _owner = owner;
        }
    }
}
