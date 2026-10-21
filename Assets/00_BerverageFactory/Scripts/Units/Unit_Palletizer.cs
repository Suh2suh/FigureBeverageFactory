using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class Unit_Palletizer : UnitBase
    {
        [SerializeField] private float _processTime = 0.5f;
        [SerializeField, ReadOnly] private bool _isProcessing;
        [SerializeField] private int _packPerPallet = 8;
        [SerializeField] private GameObject _beveragePalletPrefab;

        private readonly List<BeveragePack> _infeedMaterials = new();
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
            bool hasEnoughMaterials = (_infeedMaterials.Count >= _packPerPallet);
            if (hasEnoughMaterials)
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);

                var package = Palletize(_infeedMaterials.GetRange(0, _packPerPallet));

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

        private BeveragePallet Palletize(List<BeveragePack> materials)
        {
            if (materials.Count < _packPerPallet)
                return null;

            BeveragePallet beveragePallet = Instantiate(_beveragePalletPrefab, transform.position, Quaternion.identity)
                                            .GetComponent<BeveragePallet>();
            for (int i = 0; i < _packPerPallet; i++)
            {
                BeveragePack beveragePack = materials[i];
                beveragePallet.AddToPallet(beveragePack);

                _infeedMaterials.RemoveAt(0);
                Destroy(beveragePack.gameObject);
            }
            return beveragePallet;
        }

        private void OnTriggerEnter(Collider other)
        {
            BeveragePack beveragePack = other.GetComponentInChildren<BeveragePack>();

            if (beveragePack != null && !_infeedMaterials.Contains(beveragePack))
            {
                beveragePack.gameObject.SetActive(false);
                _infeedMaterials.Add(beveragePack);
            }
        }


        private void OnTriggerExit(Collider other)
        {
            BeveragePack beveragePack = other.GetComponentInChildren<BeveragePack>();

            if (beveragePack != null)
                _infeedMaterials.Remove(beveragePack);
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
