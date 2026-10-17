using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class LineController : MonoBehaviour
    {
        [SerializeField] private List<UnitBase> _units = new();


        public void CollectAllUnits()
        {
            _units.Clear();
            _units.AddRange(FindObjectsOfType<UnitBase>(true));
        }


        public void StartAllUnits()
        {
            foreach (UnitBase unit in _units)
            {
                unit.Reset_PackML();
                unit.Start_PackML();
            }
        }


        public void StopAllUnits()
        {
            foreach (UnitBase unit in _units)
                unit.Stop_PackML();
        }
    }
}
