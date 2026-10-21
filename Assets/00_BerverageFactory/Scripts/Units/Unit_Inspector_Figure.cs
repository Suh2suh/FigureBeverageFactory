using System.Collections.Generic;
using UnityEngine;


namespace FBFactory
{
    /// <summary>
    /// Filling - 채워진 양만 확인
    /// Capping - 뚜껑 존재 여부만 확인
    /// </summary>
    public class Unit_Inspector_Figure : UnitBase
    {
        enum FigureInspectionResult
        {
            Pass,
            Fail_NoFigure,       // 피규어 없음
            Fail_Incorrect,  // 레시피와 다른 피규어 - TODO : 추후 version에서 RobotArm연결 시 사용
        }

        [SerializeField] private BeverageDefinition _beverageRecipe;
        [SerializeField] private float _processTime = 1.0f;
        [SerializeField, ReadOnly] private bool _isProcessing;
        [SerializeField] private Transform _point_bottleDiscard;
        [SerializeField] private Unit_ConveyerBelt _rollbackConveyer;

        private List<Beverage> _infeedMaterials = new();
        private float _processCompleteTime = 0f;
        private (GameObject bottle, FigureInspectionResult result) _inspection;


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
                        _downstreamBelt.InsertMaterial(_inspection.bottle.transform);
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
                        _rollbackConveyer.InsertMaterial(_inspection.bottle.transform);
                    }
                    break;
                    case FigureInspectionResult.Fail_Incorrect:
                    {
                        _inspection.bottle.SetActive(true);
                        _inspection.bottle.transform.position = _point_bottleDiscard.position;
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
                _inspection = (bottle.gameObject, inspectionResult);

                _inspection.bottle = bottle.gameObject;
                _inspection.bottle.SetActive(false);
                _isProcessing = true;
                _processCompleteTime = Time.time + _processTime;
            }
            else
            {
                AddIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private FigureInspectionResult Inspect(Beverage bottle)
        {
            var figure = bottle.FigureSocket.GetComponentInChildren<Figure>();
            if (figure == null)
            {
                return FigureInspectionResult.Fail_NoFigure;
            }
            if (figure.Type != _beverageRecipe.FigureType)
            {
                return FigureInspectionResult.Fail_Incorrect;
            }

            return FigureInspectionResult.Pass;
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
