using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace FBFactory
{
    public class Unit_Capper : UnitBase
    {
        [SerializeField] private float _fillingIntervalSec = 1.0f;

        private List<Beverage> _targetBottles = new();
        private float _nextCappingTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextCappingTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ? _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                Cap(target);
                _nextCappingTime = Time.time + _fillingIntervalSec;
            }
            else
            {
                SetIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private void Cap(Beverage bottle)
        {
            bottle.SetCapping(Quaternion.identity);
        }

        private void OnTriggerEnter(Collider other)
        {
            var beverage = other.transform.GetComponentInChildren<Beverage>();
            if (beverage != null)
            {
                _targetBottles.Add(beverage);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var beverage = other.transform.GetComponentInChildren<Beverage>();
            if (beverage != null)
            {
                _targetBottles.Remove(beverage);
            }
        }


        protected override void OnAborting_PackML()
        {
            StateComplete();
        }

        protected override void OnClearing_PackML()
        {
            StateComplete();
        }

        protected override void OnResetting_PackML()
        {
            StateComplete();
        }

        protected override void OnStarting_PackML()
        {
            StateComplete();
        }

        protected override void OnStopping_PackML()
        {
            StateComplete();
        }


    }
}
