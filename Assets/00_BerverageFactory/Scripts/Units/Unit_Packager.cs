using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class Unit_Packager : UnitBase
    {
        [SerializeField] private float _processTime = 0.5f;
        [SerializeField, ReadOnly] private bool _isProcessing;
        [SerializeField] private int _beveragePerPack = 6;
        [SerializeField] private GameObject _beveragePackPrefab;

        private readonly List<Beverage> _infeedMaterials = new();
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
            bool hasEnoughMaterials = (_infeedMaterials.Count >= _beveragePerPack);
            if (hasEnoughMaterials)
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);

                var package = Package(_infeedMaterials.GetRange(0, _beveragePerPack));

                _producedProduct = package.gameObject;
                _producedProduct.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + _processTime;
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
                _infeedMaterials.Add(beverage);
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
