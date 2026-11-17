using System;
using UnityEngine;

namespace FBFactory
{
    public abstract class EquipmentBase : MonoBehaviour
    {
        [SerializeField, ReadOnly] protected PackMLMode _mode = PackMLMode.Production;
        [SerializeField, ReadOnly] protected PackMLState _state = PackMLState.Stopped;
        [SerializeField, ReadOnly] protected UnitIssueType _issue = UnitIssueType.None;

        public PackMLState State => _state;

        private void Awake()
        {
            _state = PackMLState.Stopped;
        }

        public void SetMode(PackMLMode to)
        {
            _mode = to;
            Stop_PackML();
        }

        public void AddIssue(UnitIssueType issueType) => _issue |= issueType;

        public void RemoveIssue(UnitIssueType issueType) => _issue &= ~issueType;

        public void ClearIssue() => _issue = UnitIssueType.None;

        public bool Reset_PackML()
        {
            if (_state != PackMLState.Stopped)
                return false;

            _state = PackMLState.Resetting;
            OnResetting_PackML();
            return true;
        }

        protected abstract void OnResetting_PackML();

        public bool Start_PackML()
        {
            if (_state != PackMLState.Idle)
                return false;

            _state = PackMLState.Starting;
            OnStarting_PackML();
            return true;
        }

        protected abstract void OnStarting_PackML();

        public bool Stop_PackML()
        {
            if (_state != PackMLState.Execute)
                return false;

            _state = PackMLState.Stopping;
            OnStopping_PackML();
            return true;
        }

        protected abstract void OnStopping_PackML();

        public bool Abort_PackML(UnitIssueType issueType)
        {
            AddIssue(issueType);
            _state = PackMLState.Aborting;
            OnAborting_PackML();
            return true;
        }

        protected abstract void OnAborting_PackML();

        public bool Clear_PackML()
        {
            if (_state != PackMLState.Aborted)
                return false;

            _state = PackMLState.Clearing;
            OnClearing_PackML();
            return true;
        }

        protected abstract void OnClearing_PackML();

        protected void StateComplete()
        {
            switch (_state)
            {
                case PackMLState.Resetting:
                    _state = PackMLState.Idle;
                    break;
                case PackMLState.Stopping:
                    _state = PackMLState.Stopped;
                    break;
                case PackMLState.Starting:
                    _state = PackMLState.Execute;
                    break;
                case PackMLState.Aborting:
                    _state = PackMLState.Aborted;
                    break;
                case PackMLState.Clearing:
                    _state = PackMLState.Stopped;
                    ClearIssue();
                    break;
            }
        }
    }
}
