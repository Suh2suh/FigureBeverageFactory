using System;
using UnityEngine;

namespace FBFactory
{
    public abstract class ProcessingUnit : EquipmentBase
    {
        protected virtual float ProcessTime => 0f;
        protected virtual float ChangeoverTime => 0f;

        private Action<ProcessingUnit> _onChangeOverComplete;
        private LineController _lineController;

        public abstract UnitType UnitType { get; }
        public virtual bool IsProcessing => false;
        protected LineController LineController => _lineController;

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

            OnSetupMaterial(materialSetup);
            Invoke(nameof(CompleteChangeOver), ChangeoverTime);
        }

        private void CompleteChangeOver()
        {
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
