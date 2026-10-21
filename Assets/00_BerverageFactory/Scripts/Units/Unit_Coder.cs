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
        [SerializeField] private float _processTime = 1.0f;
        [SerializeField, ReadOnly] private bool _isProcessing;

        private List<Beverage> _infeedMaterials = new();
        private float _processCompleteTime = 0f;
        private GameObject _producedProduct;


        protected override void ExecuteProduction()
        {
            // 가공 중일 시 대기
            if (_isProcessing)
            {
                if (Time.time >= _processCompleteTime)
                    _isProcessing = false;
                else
                    return;
            }

            // 완제품 출하 시도
            if (_producedProduct != null)
            {
                if (_downstreamBelt.GetCapacityStatus() == CapacityState.Full)
                {
                    AddIssue(UnitIssueType.DownstreamUnavailable);
                    return;
                }
                RemoveIssue(UnitIssueType.DownstreamUnavailable);

                _downstreamBelt.InsertMaterial(_producedProduct.transform);
                _producedProduct = null;
            }

            // 다음 제품 가공 시작
            Beverage bottle = (_infeedMaterials.Count > 0 ? _infeedMaterials[0] : null);
            if (bottle)
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);
                _infeedMaterials.Remove(bottle);

                Code(bottle);

                _producedProduct = bottle.gameObject;
                _producedProduct.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + _processTime;
            }
            else
            {
                AddIssue(UnitIssueType.UpstreamUnavailable);
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
                _infeedMaterials.Add(beverage);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var beverage = other.transform.GetComponentInChildren<Beverage>();
            if (beverage != null)
            {
                _infeedMaterials.Remove(beverage);
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
            _processCompleteTime = Time.time;
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
