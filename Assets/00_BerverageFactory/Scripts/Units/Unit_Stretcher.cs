using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class Unit_Stretcher : UnitBase
    {
        [SerializeField] private float _processTime = 0.5f;
        [SerializeField, ReadOnly] private bool _isProcessing;

        private readonly List<BeveragePallet> _infeedMaterials = new();
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
            BeveragePallet pallet = (_infeedMaterials.Count > 0 ? _infeedMaterials[0] : null);
            if (pallet)
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);
                _infeedMaterials.Remove(pallet);

                Stretch(pallet);

                _producedProduct = pallet.gameObject;
                _producedProduct.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + _processTime;
            }
            else
            {
                AddIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private void Stretch(BeveragePallet pallet)
        {
            pallet.SetStretched();
        }


        private void OnTriggerEnter(Collider other)
        {
            BeveragePallet beveragePallet = other.GetComponentInChildren<BeveragePallet>();

            if (beveragePallet != null && !_infeedMaterials.Contains(beveragePallet))
                _infeedMaterials.Add(beveragePallet);
        }


        private void OnTriggerExit(Collider other)
        {
            BeveragePallet beveragePallet = other.GetComponentInChildren<BeveragePallet>();

            if (beveragePallet != null)
                _infeedMaterials.Remove(beveragePallet);
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
