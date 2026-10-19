using System.Collections.Generic;
using UnityEngine;


namespace FBFactory
{
    public class Unit_FigureInserter : UnitBase
    {
        [SerializeField] private BeverageDefinition _beverageRecipe;
        [SerializeField] private float _insertIntervalSec = 1.0f;

        private List<Beverage> _targetBottles = new();
        private float _nextInsertTime = 0f;


        protected override void ExecuteProduction()
        {
            if (Time.time < _nextInsertTime)
                return;

            Beverage target = (_targetBottles.Count > 0 ? _targetBottles[0] : null);
            if (target)
            {
                _targetBottles.Remove(target);

                InsertFigure(target);
                _nextInsertTime = Time.time + _insertIntervalSec;
            }
            else
            {
                SetIssue(UnitIssueType.UpstreamUnavailable);
            }
        }

        private void InsertFigure(Beverage bottle)
        {
            // TODO : v2에서, figure은 ERP에 따라 유동적으로 붙일 것. (하나의 arm이 여러 피규어 중 하나를 선택 후 부착)
            var figure = Instantiate(FactoryDTO.Instance.FigureTable.GetFigurePrefab(_beverageRecipe.FigureType));
            figure.transform.parent = bottle.FigureSocket;
            figure.transform.localPosition = Vector3.zero;
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


        protected override void OnResetting_PackML()
        {
            _nextInsertTime = Time.time;
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
