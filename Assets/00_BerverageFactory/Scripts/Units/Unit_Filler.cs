using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace FBFactory
{
    public class Unit_Filler : UnitBase
    {
        [SerializeField] private FillingDefinition _fillingDefinition;
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

                Fill(bottle);

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
                _infeedMaterials.Add(beverage);
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
