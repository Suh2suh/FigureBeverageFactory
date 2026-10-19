using System.Collections.Generic;
using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// Bottle에 Coding
    /// - 유통기한/제조일자/생산시간 => Time 기반 설정
    /// - LOT (하나의 production order 이하 만들어진 품목들 - sprint 느낌)
    /// </summary>
    public class Unit_Coder : UnitBase
    {
        [SerializeField] private string _code = "LOT001";  // v1에서는 고정 TODO : 추후 version에서 detailize
        [SerializeField] private float _codingIntervalSec = 1.0f;

        private List<Beverage> _targetBottles = new();
        private float _nextCodingTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextCodingTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ? _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                Code(target);
                _nextCodingTime = Time.time + _codingIntervalSec;
            }
            else
            {
                SetIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private void Code(Beverage bottle)
        {
            bottle.SetCode(_code);
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
            _nextCodingTime = Time.time;
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
