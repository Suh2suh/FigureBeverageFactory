using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace FBFactory
{
    public class Unit_Filler : UnitBase
    {
        [SerializeField] private FillingDefinition _fillingDefinition;
        [SerializeField] private float _fillingIntervalSec = 1.0f;

        private List<Beverage> _targetBottles = new();
        private float _nextFillingTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextFillingTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ? _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                Fill(target);
                _nextFillingTime = Time.time + _fillingIntervalSec;
            }
            else
            {
                SetIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private void Fill(Beverage bottle)
        {
            int fillAmount = math.max(bottle.MaxML, _fillingDefinition.FillAmount);
            // ㄴ Reason - Max - Bottle capacity를 초과한 filling이 들어갔을 경우 (what-if version scenario)
            // ㄴ TODO: 추후 what-if에서 일부 beverage는 fill less

            bottle.SetFilling(_fillingDefinition.Flavor, fillAmount);
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
            _nextFillingTime = Time.time;
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
