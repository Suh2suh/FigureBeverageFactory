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
        [SerializeField] private float _inspectionIntervalSec = 1.0f;
        [SerializeField] private Transform _point_bottleDiscard;
        [SerializeField] private Transform _point_bottleRollback;

        private List<Beverage> _targetBottles = new();
        private float _nextInspectionTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextInspectionTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ? _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                FigureInspectionResult inspectionResult = Inspect(target);
                if (inspectionResult == FigureInspectionResult.Fail_NoFigure)
                {
                    target.transform.position = _point_bottleRollback.position;
                }
                else if (inspectionResult == FigureInspectionResult.Fail_Incorrect)
                {
                    target.transform.position = _point_bottleDiscard.position;
                }

                _nextInspectionTime = Time.time + _inspectionIntervalSec;
            }
            else
            {
                SetIssue(UnitIssueType.UpstreamUnavailable);
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
                _targetBottles.Add(beverage);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var beverage = other.transform.GetComponentInChildren<Beverage>();
            if (beverage != null)
            {
                _targetBottles.Remove(beverage);
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
