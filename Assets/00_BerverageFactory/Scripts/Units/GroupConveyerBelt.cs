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

        private Unit_ConveyerBelt InputBelt => _belts[0];
        private Unit_ConveyerBelt _outputBelt => _belts[^1];


        public void Reset_PackML()
            => _belts.ForEach(belt => belt.Reset_PackML());

        public void Start_PackML()
            => _belts.ForEach(belt => belt.Start_PackML());

        public void Stop_PackML()
            => _belts.ForEach(belt => belt.Stop_PackML());

        public bool InsertMaterial(FactoryMaterial material)
        {
            foreach (Unit_ConveyerBelt belt in _belts)
            {
                if (GetCapacityStatus() == CapacityState.Full)
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
                belt.UpdateStatus();
                totalArea += belt.BeltArea;
                occupiedArea += belt.Capacity * belt.BeltArea;
            }

            _capacity = totalArea <= 0f ? 0f : occupiedArea / totalArea;
        }


    }
}
