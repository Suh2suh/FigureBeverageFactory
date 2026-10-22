using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// PET Material -> Bottle
    /// </summary>
    public class Unit_BlowModular : UnitBase
    {
        [SerializeField] private float _processTime = 1.0f;
        [SerializeField] private RawPETContainer _rawPETContainer;
        [SerializeField, ReadOnly] private bool _isProcessing;

        private float _processCompleteTime = 0f;
        private FactoryMaterial _producedProduct = null;


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
            if (_rawPETContainer.TryConsume(out var rawPET))
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);

                _producedProduct = Instantiate(rawPET.Prefab, transform.position, Quaternion.identity)
                                   .GetComponent<Beverage>();
                _producedProduct.Own(this);
                _producedProduct.gameObject.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + _processTime;
            }
            else
            {
                AddIssue(UnitIssueType.UpstreamUnavailable);
            }
        }


        protected override void OnResetting_PackML()
        {
            _processCompleteTime = 0f;

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

        protected override void OnAborting_PackML()
        {
            StateComplete();
        }

        protected override void OnClearing_PackML()
        {
            StateComplete();
        }

    }
}
