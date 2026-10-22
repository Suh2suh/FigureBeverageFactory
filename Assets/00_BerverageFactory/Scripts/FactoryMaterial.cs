using UnityEngine;

namespace FBFactory
{
    public class FactoryMaterial : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;

        private UnitBase _owner;

        public UnitBase Owner => _owner;


        public void Own(UnitBase owner)
        {
            _owner = owner;
        }
    }
}
