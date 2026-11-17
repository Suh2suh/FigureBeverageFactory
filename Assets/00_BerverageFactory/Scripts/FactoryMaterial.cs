using UnityEngine;

namespace FBFactory
{
    public class FactoryMaterial : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField, ReadOnly] private EquipmentBase _owner;

        public Rigidbody Rigidbody => _rigidbody;


        public EquipmentBase Owner => _owner;


        public void Own(EquipmentBase owner)
        {
            _owner = owner;
        }
    }
}
