using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FBFactory
{
    public class Unit_ConveyerBelt : UnitBase
    {
        [SerializeField] private Vector3 _moveDirection = Vector3.right;
        [SerializeField] private float _velocityCmPerSecond = 1f;
        [SerializeField] private Transform _inputPoint;
        [SerializeField] private Transform _outputPoint;
        [SerializeField, ReadOnly] private float _capacity;
        [SerializeField, ReadOnly] private CapacityState _status;

        private readonly LayerMask _conveyerBeltMask = 1 << 7;
        private readonly List<FactoryMaterial> _materials = new();
        private float _elapsedTime;


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

        protected override void ExecuteProduction()
        {
            //
        }

        private void FixedUpdate()
        {
            UpdateStatus();

            if (_state == PackMLState.Execute)
            {
                MoveBelt();
            }
        }

        private void MoveBelt()
        {
            Vector3 direction = transform.TransformDirection(_moveDirection.normalized);
            float velocityMeterPerSecond = _velocityCmPerSecond * 0.01f;
            Vector3 movement = direction * velocityMeterPerSecond * Time.fixedDeltaTime;

            for (int i = _materials.Count - 1; i >= 0; i--)
            {
                FactoryMaterial material = _materials[i];

                if (material == null)
                {
                    _materials.RemoveAt(i);
                    continue;
                }

                if (material.Owner != this)
                    continue;

                material.Rigidbody.MovePosition(material.Rigidbody.position + movement);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((_conveyerBeltMask.value & (1 << other.gameObject.layer)) != 0)
                return;

            Rigidbody materialRigidbody = other.attachedRigidbody;

            if (materialRigidbody == null)
                return;

            FactoryMaterial material = materialRigidbody.GetComponent<FactoryMaterial>();

            if (material != null && !_materials.Contains(material))
            {
                material.Own(this);
                _materials.Add(material);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody materialRigidbody = other.attachedRigidbody;

            if (materialRigidbody == null)
                return;

            FactoryMaterial material = materialRigidbody.GetComponent<FactoryMaterial>();

            if (material != null)
            {
                _materials.Remove(material);
            }
        }


        public bool InsertMaterial(FactoryMaterial material)
        {
            if (_status == CapacityState.Full)
                return false;

            material.gameObject.SetActive(true);
            material.Own(this);
            material.Rigidbody.position = _inputPoint.position;

            if (!_materials.Contains(material))
                _materials.Add(material);

            return true;
        }

        public CapacityState GetCapacityStatus()
        {
            return _status;
        }



        private void UpdateStatus()
        {
            _capacity = UpdateCapacity();

            if (_capacity <= 0f)
            {
                _status = CapacityState.Empty;
                return;
            }

            if (_capacity < 0.6f)
            {
                _status = CapacityState.Normal;
                return;
            }

            if (_capacity < 0.8f)
            {
                _status = CapacityState.High;
                return;
            }

            _status = CapacityState.Full;
        }

        #region ConveyerBelt Capacity Sensing

            private float UpdateCapacity()
            {
                Collider beltCollider = GetComponent<Collider>();

                if (beltCollider == null || !TryGetProjectedRect(beltCollider, out Rect beltRect))
                    return 0f;

                List<Rect> materialRects = new();

                for (int i = _materials.Count - 1; i >= 0; i--)
                {
                    FactoryMaterial material = _materials[i];

                    if (material == null)
                    {
                        _materials.RemoveAt(i);
                        continue;
                    }

                    Collider materialCollider = material.GetComponentInChildren<Collider>();

                    if (materialCollider == null || !TryGetProjectedRect(materialCollider, out Rect materialRect))
                        continue;

                    materialRect = Rect.MinMaxRect(
                        Mathf.Max(materialRect.xMin, beltRect.xMin),
                        Mathf.Max(materialRect.yMin, beltRect.yMin),
                        Mathf.Min(materialRect.xMax, beltRect.xMax),
                        Mathf.Min(materialRect.yMax, beltRect.yMax));

                    if (materialRect.width > 0f && materialRect.height > 0f)
                        materialRects.Add(materialRect);
                }

                float beltArea = beltRect.width * beltRect.height;

                if (beltArea <= 0f || materialRects.Count == 0)
                    return 0f;

                List<float> xPositions = new();

                for (int i = 0; i < materialRects.Count; i++)
                {
                    xPositions.Add(materialRects[i].xMin);
                    xPositions.Add(materialRects[i].xMax);
                }

                xPositions.Sort();

                float occupiedArea = 0f;

                for (int i = 0; i < xPositions.Count - 1; i++)
                {
                    float leftX = xPositions[i];
                    float rightX = xPositions[i + 1];
                    float width = rightX - leftX;

                    if (width <= 0f)
                        continue;

                    float centerX = (leftX + rightX) * 0.5f;
                    List<Vector2> zIntervals = new();

                    for (int j = 0; j < materialRects.Count; j++)
                    {
                        Rect materialRect = materialRects[j];

                        if (centerX >= materialRect.xMin && centerX <= materialRect.xMax)
                            zIntervals.Add(new Vector2(materialRect.yMin, materialRect.yMax));
                    }

                    zIntervals.Sort((left, right) => left.x.CompareTo(right.x));
                    float occupiedZ = 0f;
                    float currentZMin = 0f;
                    float currentZMax = 0f;

                    for (int j = 0; j < zIntervals.Count; j++)
                    {
                        Vector2 zInterval = zIntervals[j];

                        if (j == 0)
                        {
                            currentZMin = zInterval.x;
                            currentZMax = zInterval.y;
                            continue;
                        }

                        if (zInterval.x <= currentZMax)
                        {
                            currentZMax = Mathf.Max(currentZMax, zInterval.y);
                            continue;
                        }

                        occupiedZ += currentZMax - currentZMin;
                        currentZMin = zInterval.x;
                        currentZMax = zInterval.y;
                    }

                    if (zIntervals.Count > 0)
                        occupiedZ += currentZMax - currentZMin;

                    occupiedArea += width * occupiedZ;
                }

                return Mathf.Clamp01(occupiedArea / beltArea);
            }

            private bool TryGetProjectedRect(Collider collider, out Rect rect)
            {
                Vector3[] corners;

                if (collider is BoxCollider boxCollider)
                {
                    Vector3 halfSize = boxCollider.size * 0.5f;
                    Vector3 center = boxCollider.center;
                    Vector3[] localCorners =
                    {
                        center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                        center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                        center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                        center + new Vector3(-halfSize.x, halfSize.y, halfSize.z),
                        center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                        center + new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                        center + new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                        center + new Vector3(halfSize.x, halfSize.y, halfSize.z),
                    };

                    corners = new Vector3[localCorners.Length];

                    for (int i = 0; i < localCorners.Length; i++)
                        corners[i] = boxCollider.transform.TransformPoint(localCorners[i]);
                }
                else
                {
                    Bounds bounds = collider.bounds;
                    corners = new Vector3[]
                    {
                        new(bounds.min.x, bounds.min.y, bounds.min.z),
                        new(bounds.min.x, bounds.min.y, bounds.max.z),
                        new(bounds.min.x, bounds.max.y, bounds.min.z),
                        new(bounds.min.x, bounds.max.y, bounds.max.z),
                        new(bounds.max.x, bounds.min.y, bounds.min.z),
                        new(bounds.max.x, bounds.min.y, bounds.max.z),
                        new(bounds.max.x, bounds.max.y, bounds.min.z),
                        new(bounds.max.x, bounds.max.y, bounds.max.z),
                    };
                }

                float minX = float.MaxValue;
                float maxX = float.MinValue;
                float minZ = float.MaxValue;
                float maxZ = float.MinValue;

                for (int i = 0; i < corners.Length; i++)
                {
                    Vector3 localPoint = transform.InverseTransformPoint(corners[i]);
                    minX = Mathf.Min(minX, localPoint.x);
                    maxX = Mathf.Max(maxX, localPoint.x);
                    minZ = Mathf.Min(minZ, localPoint.z);
                    maxZ = Mathf.Max(maxZ, localPoint.z);
                }

                rect = Rect.MinMaxRect(minX, minZ, maxX, maxZ);
                return true;
            }

            private bool TryGetMaterialInterval(Transform material, Vector3 position, out Vector2 interval)
            {
                Collider materialCollider = material.GetComponentInChildren<Collider>();

                if (materialCollider == null)
                {
                    interval = default;
                    return false;
                }

                Vector3 direction = transform.TransformDirection(_moveDirection.normalized);
                Bounds bounds = materialCollider.bounds;
                Vector3 boundsCenter = bounds.center + position - material.position;
                float center = Vector3.Dot(boundsCenter - _inputPoint.position, direction);
                float extent = Mathf.Abs(direction.x) * bounds.extents.x +
                               Mathf.Abs(direction.y) * bounds.extents.y +
                               Mathf.Abs(direction.z) * bounds.extents.z;

                interval = new Vector2(center - extent, center + extent);
                return true;
            }

        #endregion

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
