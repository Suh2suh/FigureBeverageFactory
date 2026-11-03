using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FBFactory
{
    public class GroupConveyerBelt : MonoBehaviour
    {
        [SerializeField] private string _equipmentId;
        [SerializeField] private UnitBase _upstreamUnit;
        [SerializeField] private UnitBase _downstreamUnit;
        [SerializeField] private List<Unit_ConveyerBelt> _belts = new();
        [SerializeField, ReadOnly] private float _capacity;

        public string EquipmentId => _equipmentId;
        public UnitBase UpstreamUnit => _upstreamUnit;
        public UnitBase DownstreamUnit => _downstreamUnit;
        public IReadOnlyList<Unit_ConveyerBelt> Belts => _belts;
        public float Capacity => _capacity;

        private void Awake()
        {
            OnTransformChildrenChanged();
        }

        public void Start_PackML()
        {
            foreach (Unit_ConveyerBelt belt in _belts)
                belt.Start_PackML();
        }

        public void Stop_PackML()
        {
            foreach (Unit_ConveyerBelt belt in _belts)
                belt.Stop_PackML();
        }

        public bool InsertMaterial(FactoryMaterial material)
        {
            foreach (Unit_ConveyerBelt belt in _belts)
            {
                if (belt.GetCapacityStatus() == CapacityState.Full)
                    continue;

                return belt.InsertMaterial(material);
            }

            return false;
        }

        public CapacityState GetCapacityStatus()
        {
            if (_capacity <= 0f)
                return CapacityState.Empty;

            if (_capacity < 0.6f)
                return CapacityState.Normal;

            if (_capacity < 0.8f)
                return CapacityState.High;

            return CapacityState.Full;
        }

        private void FixedUpdate()
        {
            UpdateCapacity();
        }

        private void OnTransformChildrenChanged()
        {
            _belts.Clear();
            _belts.AddRange(GetComponentsInChildren<Unit_ConveyerBelt>());
        }

        private void OnValidate()
        {
            OnTransformChildrenChanged();
        }

        private void UpdateCapacity()
        {
            if (_belts.Count == 0)
            {
                _capacity = 0f;
                return;
            }

            float totalArea = 0f;
            float occupiedArea = 0f;

            foreach (Unit_ConveyerBelt belt in _belts)
            {
                totalArea += belt.BeltArea;
                occupiedArea += belt.Capacity * belt.BeltArea;
            }

            _capacity = totalArea <= 0f ? 0f : occupiedArea / totalArea;
        }
    }
}
