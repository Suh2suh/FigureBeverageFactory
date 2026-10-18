using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// Filling - 채워진 양만 확인
    /// Capping - 뚜껑 존재 여부만 확인
    /// </summary>
    public class Unit_Inspector_FillingCapping : UnitBase
    {
        [SerializeField] private FillingDefinition _fillingDefinition;
        [SerializeField] private float _inspectionIntervalSec = 1.0f;
        [SerializeField] private Transform _inspectionRejectPoint;

        private List<Beverage> _targetBottles = new();
        private float _nextInspectionTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextInspectionTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ?  _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                bool isPassed = Inspect(target);
                if (!isPassed)
                {
                    target.transform.position = _inspectionRejectPoint.position;
                }

                _nextInspectionTime = Time.time + _inspectionIntervalSec;
            }
            else
            {
                SetIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private bool Inspect(Beverage bottle)
        {
            if (!bottle.HasCap)
                return false;
            if (bottle.FilledML < _fillingDefinition.FillAmount)  // TODO : Threshold?
                return false;

            return true;
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
