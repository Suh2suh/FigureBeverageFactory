using System.Collections.Generic;
using UnityEngine;


namespace FBFactory
{
    public class Unit_Inspector_Figure : UnitBase
    {
        public override UnitType UnitType => UnitType.Inspector_Figure;

        enum FigureInspectionResult
        {
            Pass,
            Fail_NoFigure,       // 피규어 없음
            Fail_Incorrect,  // 레시피와 다른 피규어 - TODO : 추후 version에서 RobotArm연결 시 사용
        }

        [SerializeField, ReadOnly] private MaterialScriptableObject _figureMaterial;
        protected override float ProcessTime => 0.90f;
        protected override float ChangeoverTime => 5f;

        protected override void OnSetupMaterial(OrderMaterialSetup materialSetup)
        {
            if (materialSetup.TryGetMaterials(MaterialType.Figure, out var materials))
                _figureMaterial = materials[0];
        }
        [SerializeField, ReadOnly] private bool _isProcessing;
        [SerializeField] private Transform _point_bottleDiscard;
        [SerializeField] private GroupConveyerBelt _rollbackConveyer;

        private List<Beverage> _infeedMaterials = new();
        private float _processCompleteTime = 0f;
        private (Beverage bottle, FigureInspectionResult result) _inspection;


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
            if (_inspection.bottle != null)
            {
                switch (_inspection.result)
                {
                    case FigureInspectionResult.Pass:
                    {
                        if (_downstreamBelt.GetCapacityStatus() == CapacityState.Full)
                        {
                            AddIssue(UnitIssueType.DownstreamUnavailable);
                            return;
                        }
                        RemoveIssue(UnitIssueType.DownstreamUnavailable);
                        _downstreamBelt.InsertMaterial(_inspection.bottle);
                    }
                    break;
                    case FigureInspectionResult.Fail_NoFigure:
                    {
                        if (_rollbackConveyer.GetCapacityStatus() == CapacityState.Full)
                        {
                            AddIssue(UnitIssueType.DownstreamUnavailable);
                            return;
                        }
                        RemoveIssue(UnitIssueType.DownstreamUnavailable);
                        _rollbackConveyer.InsertMaterial(_inspection.bottle);
                    }
                    break;
                    case FigureInspectionResult.Fail_Incorrect:
                    {
                        _inspection.bottle.gameObject.SetActive(true);
                        _inspection.bottle.Rigidbody.position = _point_bottleDiscard.position;
                    }
                    break;
                }
                _inspection.bottle = null;
            }

            // 다음 제품 가공 시작
            Beverage bottle = (_infeedMaterials.Count > 0 ? _infeedMaterials[0] : null);
            if (bottle)
            {
                RemoveIssue(UnitIssueType.UpstreamUnavailable);
                _infeedMaterials.Remove(bottle);

                FigureInspectionResult inspectionResult = Inspect(bottle);
                _inspection = (bottle, inspectionResult);

                _inspection.bottle = bottle;
                _inspection.bottle.gameObject.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + ProcessTime;
            }
            else
            {
                AddIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private FigureInspectionResult Inspect(Beverage bottle)
        {
            if (bottle.Figure == null)
            {
                return FigureInspectionResult.Fail_NoFigure;
            }
            if (bottle.Figure != _figureMaterial)
            {
                return FigureInspectionResult.Fail_Incorrect;
            }

            return FigureInspectionResult.Pass;
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
