using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// PET Material -> Bottle
    /// </summary>
    public class BlowModular : UnitBase
    {
        [SerializeField] float _blowInterval;  // TODO : 추후 version에서 data 기반 조정?
        [SerializeField] RawPETContainer _rawPETContainer;
        [SerializeField] Transform _blowPosition;

        private float _nextBlowTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextBlowTime)
                return;

            bool hasPET = _rawPETContainer.TryConsume(out var rawPET);
            if (!hasPET)
            {
                Abort_PackML(UnitIssueType.UpstreamUnavailable);
                return;
            }

            _nextBlowTime = Time.time + _blowInterval;
            Instantiate(rawPET.Prefab, _blowPosition.position, Quaternion.identity);
        }


        protected override void OnResetting_PackML()
        {
            _nextBlowTime = 0f;

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
