using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    /// <summary>
    /// Bottle에 라벨 부착
    /// </summary>
    public class Unit_Labeler : UnitBase
    {
        public override UnitType UnitType => UnitType.Labeler;
        [SerializeField] private MaterialScriptableObject _labelMaterial;
        protected override float ProcessTime => 0.70f;
        protected override float ChangeoverTime => 30f;

        protected override void OnSetupMaterial(OrderMaterialSetup materialSetup)
        {
            if (materialSetup.TryGetMaterials(MaterialType.Label, out var materials))
                _labelMaterial = materials[0];
        }
        [SerializeField, ReadOnly] private bool _isProcessing;

        private List<Beverage> _infeedMaterials = new();
        private float _processCompleteTime = 0f;
        private FactoryMaterial _producedProduct;


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

                _downstreamBelt.InsertMaterial(_producedProduct);
                _producedProduct = null;
            }

            // 다음 제품 가공 시작
            Beverage bottle = (_infeedMaterials.Count > 0 ? _infeedMaterials[0] : null);
            if (bottle)
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);
                _infeedMaterials.Remove(bottle);

                Label(bottle);

                _producedProduct = bottle;
                _producedProduct.gameObject.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + ProcessTime;
            }
            else
            {
                AddIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private void Label(Beverage bottle)
        {
            // TODO : v4에서 일부는 라벨 미부착
            bottle.SetLabel(_labelMaterial.Material);
        }

        private void OnTriggerEnter(Collider other)
        {
            var beverage = other.transform.GetComponentInChildren<Beverage>();
            if (beverage != null && !_infeedMaterials.Contains(beverage))
            {
                beverage.Own(this);
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
