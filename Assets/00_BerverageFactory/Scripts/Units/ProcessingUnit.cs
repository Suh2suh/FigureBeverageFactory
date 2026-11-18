using System;
using UnityEngine;

namespace FBFactory
{
    public abstract class ProcessingUnit : EquipmentBase
    {
        protected virtual float ProcessTime => 0f;
        protected virtual float ChangeoverTime => 0f;

        [SerializeField, ReadOnly] private float _processTime;
        [SerializeField, ReadOnly] private float _changeoverTime;
        [SerializeField, ReadOnly] private bool _isChangingOver;

        private Action<ProcessingUnit> _onChangeOverComplete;
        private LineController _lineController;

        public abstract UnitType UnitType { get; }
        public virtual bool IsProcessing => false;
        public bool IsChangingOver => _isChangingOver;
        protected LineController LineController => _lineController;

        private void OnValidate()
        {
            _processTime = ProcessTime;
            _changeoverTime = ChangeoverTime;
        }

        private void Update()
        {
            if (_mode != PackMLMode.Production ||
                _state != PackMLState.Execute)
                return;

            ExecuteProduction();
        }

        protected abstract void ExecuteProduction();

        public void SetupMaterial(OrderMaterialSetup materialSetup)
        {
            if (_state != PackMLState.Stopped)
                return;

            _isChangingOver = true;
            OnSetupMaterial(materialSetup);
            Invoke(nameof(CompleteChangeOver), ChangeoverTime);
        }

        private void CompleteChangeOver()
        {
            _isChangingOver = false;
            _onChangeOverComplete?.Invoke(this);
        }

        public void SetChangeOverCompleteCallback(Action<ProcessingUnit> callback)
        {
            _onChangeOverComplete = callback;
        }

        public void SetLineController(LineController lineController)
        {
            _lineController = lineController;
        }

        protected virtual void OnSetupMaterial(OrderMaterialSetup materialSetup)
        {
        }
    }
}
