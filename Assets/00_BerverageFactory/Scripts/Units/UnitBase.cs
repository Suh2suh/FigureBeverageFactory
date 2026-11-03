using Unity.Collections;
using UnityEngine;
using System;

namespace FBFactory
{
    public abstract class UnitBase : MonoBehaviour
    {
        protected virtual float ProcessTime => 0f;
        protected virtual float ChangeoverTime => 0f;

        public abstract UnitType UnitType { get; }

        [SerializeField, ReadOnly] protected PackMLMode _mode = PackMLMode.Production;
        [SerializeField, ReadOnly] protected PackMLState _state = PackMLState.Stopped;
        [SerializeField, ReadOnly] protected UnitIssueType _issue = UnitIssueType.None;
        [SerializeField] protected GroupConveyerBelt _upstreamBelt;
        [SerializeField] protected GroupConveyerBelt _downstreamBelt;

        private Action<UnitBase> _onChangeOverComplete;

        public PackMLState State => _state;
        public GroupConveyerBelt UpstreamBelt => _upstreamBelt;
        public GroupConveyerBelt DownstreamBelt => _downstreamBelt;


        private void Awake()
        {
            _state = PackMLState.Stopped;
        }

        private void Update()
        {
            if (_mode != PackMLMode.Production ||
                _state != PackMLState.Execute)
            {
                return;
            }

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

        public void SetChangeOverCompleteCallback(Action<UnitBase> callback)
        {
            _onChangeOverComplete = callback;
        }

        protected virtual void OnSetupMaterial(OrderMaterialSetup materialSetup)
        {
        }


        public void SetMode(PackMLMode to)
        {
            _mode = to;

            Stop_PackML();
        }

        public void AddIssue(UnitIssueType issueType)
        {
            _issue |= issueType;
        }

        public void RemoveIssue(UnitIssueType issueType)
        {
            _issue &= ~issueType;
        }

        public void ClearIssue()
        {
            _issue = UnitIssueType.None;
        }


        /// <summary>
        /// 초기화 - 운전 준비
        /// </summary>
        public bool Reset_PackML()
        {
            if (_state != PackMLState.Stopped)
                return false;

            // pending for stateComplete
            _state = PackMLState.Resetting;  // -> Idle
            OnResetting_PackML();

            return true;
        }
        protected abstract void OnResetting_PackML();

        /// <summary>
        /// 작동 시작
        /// </summary>
        public bool Start_PackML()
        {
            if (_state != PackMLState.Idle)
                return false;

            // pending for stateComplete
            _state = PackMLState.Starting;  // -> Execute
            OnStarting_PackML();

            return true;
        }
        protected abstract void OnStarting_PackML();

        /// <summary>
        /// 작동 정지 (정상 정지)
        /// </summary>
        public bool Stop_PackML()
        {
            if (_state != PackMLState.Execute)
                return false;

            // pending for stateComplete
            _state = PackMLState.Stopping; // -> Stopped
            OnStopping_PackML();

            return true;
        }
        protected abstract void OnStopping_PackML();

        /// <summary>
        /// 작동 정지 {비정상 정지(비정상 상황 감지)}
        /// </summary>
        public bool Abort_PackML(UnitIssueType issueType)
        {
            AddIssue(issueType);

            // pending for stateComplete
            _state = PackMLState.Aborting;  // -> Aborted
            OnAborting_PackML();

            return true;
        }
        protected abstract void OnAborting_PackML();

        /// <summary>
        /// 비정상 정지 상황 복구
        /// </summary>
        public bool Clear_PackML()
        {
            if (_state != PackMLState.Aborted)
                return false;

            // pending for stateComplete
            _state = PackMLState.Clearing;  // -> Stopped
            OnClearing_PackML();

            return true;
        }
        protected abstract void OnClearing_PackML();


        // v1 이후 버전에서 구조 변경 시, Event로 뺄 여지가 있음
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
                default:
                    break;
            }
        }


    }
}
