using System.Collections.Generic;
using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// Label - material id 확인 - TODO : 추후 flavor별로 나누어서 vision?
    /// Code - 생산일자/유통기한 확인
    /// </summary>
    public class Unit_Inspector_LabelingCoding : UnitBase
    {
        [SerializeField] private BeverageDefinition _beverageRecipe;
        [SerializeField] private float _inspectionIntervalSec = 1.0f;
        [SerializeField] private Transform _point_bottleDiscard;

        private List<Beverage> _targetBottles = new();
        private float _nextInspectionTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextInspectionTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ? _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                bool isPassed = Inspect(target);
                if (!isPassed)
                {
                    target.transform.position = _point_bottleDiscard.position;
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
            if (!bottle.Label != _beverageRecipe.Label)
                return false;
            if (bottle.Code == string.Empty) // TODO: 추후 version에서 detailize - v1에서 욕심 X
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
