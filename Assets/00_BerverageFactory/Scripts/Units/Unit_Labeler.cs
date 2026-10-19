using System.Collections.Generic;
using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// Bottle에 라벨 부착
    /// </summary>
    public class Unit_Labeler : UnitBase
    {
        [SerializeField] private BeverageDefinition _beverageRecipe;
        [SerializeField] private float _labelingIntervalSec = 0.5f;

        private List<Beverage> _targetBottles = new();
        private float _nextLabelingTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextLabelingTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ? _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                Label(target);
                _nextLabelingTime = Time.time + _labelingIntervalSec;
            }
            else
            {
                SetIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private void Label(Beverage bottle)
        {
            // TODO : 추후 version에서 일부는 라벨 미부착 (Krones 누락발생률 참고)
            bottle.SetLabel(_beverageRecipe.Label);
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
            _nextLabelingTime = Time.time;
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
