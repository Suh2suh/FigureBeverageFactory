using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class ContainerBelt : UnitBase
    {
        [SerializeField] private Vector3 _moveDirection = Vector3.right;
        [SerializeField] private float _velocityCmPerSecond = 1f;

        private readonly List<Rigidbody> _thingsOnBelt = new();
        private float _elapsedTime;


        protected override void ExecuteProduction()
        {
            _elapsedTime += Time.deltaTime;

            while (_elapsedTime >= Time.fixedDeltaTime)
            {
                MoveBelt();
                _elapsedTime -= Time.fixedDeltaTime;
            }
        }

        private void MoveBelt()
        {
            Vector3 direction = transform.TransformDirection(_moveDirection.normalized);
            float velocityMeterPerSecond = _velocityCmPerSecond * 0.01f;
            Vector3 movement = direction * velocityMeterPerSecond * Time.fixedDeltaTime;

            for (int i = _thingsOnBelt.Count - 1; i >= 0; i--)
            {
                Rigidbody thing = _thingsOnBelt[i];

                if (thing == null)
                {
                    _thingsOnBelt.RemoveAt(i);
                    continue;
                }

                thing.MovePosition(thing.position + movement);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody thingOnBelt = other.attachedRigidbody;

            if (thingOnBelt != null && !_thingsOnBelt.Contains(thingOnBelt))
                _thingsOnBelt.Add(thingOnBelt);
        }


        private void OnTriggerExit(Collider other)
        {
            Rigidbody thingOnBelt = other.attachedRigidbody;

            if (thingOnBelt != null)
                _thingsOnBelt.Remove(thingOnBelt);
        }


        protected override void OnResetting_PackML()
        {
            _elapsedTime = 0f;
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


        private void OnDrawGizmos()
        {
            Vector3 direction = transform.TransformDirection(_moveDirection.normalized);
            Vector3 startPosition = transform.position;
            Vector3 endPosition = startPosition + direction;
            Vector3 arrowBack = endPosition - direction * 0.2f;
            Vector3 arrowSide = Vector3.Cross(direction, Vector3.up).normalized;

            if (arrowSide == Vector3.zero)
                arrowSide = Vector3.Cross(direction, Vector3.right).normalized;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(startPosition, endPosition);
            Gizmos.DrawLine(endPosition, arrowBack + arrowSide * 0.1f);
            Gizmos.DrawLine(endPosition, arrowBack - arrowSide * 0.1f);

#if UNITY_EDITOR
            GUIStyle style = new GUIStyle(UnityEditor.EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.white;

            string label = $"{_velocityCmPerSecond:0.##} cm/sec";
            Vector3 labelPosition = endPosition + Vector3.up * 0.2f;

            UnityEditor.Handles.Label(labelPosition, label, style);
#endif
        }
    }
}
