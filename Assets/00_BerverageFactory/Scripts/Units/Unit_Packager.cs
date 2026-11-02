using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class Unit_Packager : UnitBase
    {
        public override UnitType UnitType => UnitType.Packager;

        protected override float ProcessTime => 7.80f;
        protected override float ChangeoverTime => 30f;

        protected override void OnSetupMaterial(OrderMaterialSetup materialSetup)
        {
            // TODO: materialSetup 기반으로 Packager 재료를 설정한다.
        }
        [SerializeField, ReadOnly] private bool _isProcessing;
        [SerializeField] private int _beveragePerPack = 6;
        [SerializeField] private GameObject _beveragePackPrefab;

        private readonly List<Beverage> _infeedMaterials = new();
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
            bool hasEnoughMaterials = (_infeedMaterials.Count >= _beveragePerPack);
            if (hasEnoughMaterials)
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);

                var package = Package(_infeedMaterials.GetRange(0, _beveragePerPack));

                _producedProduct = package;
                _producedProduct.gameObject.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + ProcessTime;
            }
            else
            {
                AddIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private BeveragePack Package(List<Beverage> materials)
        {
            if (materials.Count < _beveragePerPack)
                return null;

            var beveragePack = Instantiate(_beveragePackPrefab, transform.position, Quaternion.identity)
                   .GetComponent<BeveragePack>();
            beveragePack.Own(this);
            for (int i = 0; i < _beveragePerPack; i++)
            {
                Beverage beverage = materials[i];
                beveragePack.AddToPack(beverage);

                _infeedMaterials.Remove(beverage);
                Destroy(beverage.gameObject);
            }
            return beveragePack;
        }


        private void OnTriggerEnter(Collider other)
        {
            Beverage beverage = other.GetComponentInChildren<Beverage>();

            if (beverage != null && !_infeedMaterials.Contains(beverage))
            {
                _infeedMaterials.Add(beverage);
            }
        }


        private void OnTriggerExit(Collider other)
        {
            Beverage beverage = other.GetComponentInChildren<Beverage>();

            if (beverage != null)
                _infeedMaterials.Remove(beverage);
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
