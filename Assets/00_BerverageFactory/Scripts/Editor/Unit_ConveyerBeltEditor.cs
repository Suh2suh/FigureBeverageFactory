using UnityEditor;
using UnityEngine;

namespace FBFactory.Editor
{
    [CustomEditor(typeof(Unit_ConveyerBelt))]
    public class Unit_ConveyerBeltEditor : UnityEditor.Editor
    {
        private SerializedProperty _inputPointProperty;
        private SerializedProperty _outputPointProperty;
        private SerializedProperty _moveDirectionProperty;

        private void OnEnable()
        {
            _inputPointProperty = serializedObject.FindProperty("_inputPoint");
            _outputPointProperty = serializedObject.FindProperty("_outputPoint");
            _moveDirectionProperty = serializedObject.FindProperty("_moveDirection");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "_inputPoint", "_outputPoint");

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_inputPointProperty);
            EditorGUILayout.PropertyField(_outputPointProperty);

            if (GUILayout.Button("Refresh Input/Output Point"))
                RefreshInputOutputPoint();

            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshInputOutputPoint()
        {
            Unit_ConveyerBelt belt = (Unit_ConveyerBelt)target;
            Collider beltCollider = belt.GetComponent<Collider>();

            if (beltCollider == null)
            {
                Debug.LogWarning("Unit_ConveyerBelt에 Collider가 필요합니다.", belt);
                return;
            }

            Vector3 direction = _moveDirectionProperty.vector3Value.normalized;

            if (direction == Vector3.zero)
            {
                Debug.LogWarning("Move Direction은 0이 될 수 없습니다.", belt);
                return;
            }

            Bounds localBounds = GetLocalBounds(beltCollider);
            Vector3 localDirection = direction.normalized;
            float extent = Mathf.Abs(localDirection.x) * localBounds.extents.x +
                           Mathf.Abs(localDirection.y) * localBounds.extents.y +
                           Mathf.Abs(localDirection.z) * localBounds.extents.z;

            Vector3 localInputPosition = localBounds.center - localDirection * extent;
            Vector3 localOutputPosition = localBounds.center + localDirection * extent;

            Transform inputPoint = GetOrCreatePoint(belt, _inputPointProperty.objectReferenceValue as Transform, "InputPoint");
            Transform outputPoint = GetOrCreatePoint(belt, _outputPointProperty.objectReferenceValue as Transform, "OutputPoint");

            Undo.RecordObjects(new Object[] { inputPoint, outputPoint }, "Refresh Conveyor Points");
            inputPoint.localPosition = localInputPosition;
            outputPoint.localPosition = localOutputPosition;

            _inputPointProperty.objectReferenceValue = inputPoint;
            _outputPointProperty.objectReferenceValue = outputPoint;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(belt);
        }

        private static Transform GetOrCreatePoint(Unit_ConveyerBelt belt, Transform point, string pointName)
        {
            if (point != null)
                return point;

            GameObject pointObject = new GameObject(pointName);
            Undo.RegisterCreatedObjectUndo(pointObject, $"Create {pointName}");
            pointObject.transform.SetParent(belt.transform);
            pointObject.transform.localRotation = Quaternion.identity;
            pointObject.transform.localScale = Vector3.one;

            return pointObject.transform;
        }

        private static Bounds GetLocalBounds(Collider collider)
        {
            if (collider is BoxCollider boxCollider)
                return new Bounds(boxCollider.center, boxCollider.size);

            Bounds worldBounds = collider.bounds;
            Vector3 localMin = collider.transform.InverseTransformPoint(worldBounds.min);
            Vector3 localMax = collider.transform.InverseTransformPoint(worldBounds.max);
            Bounds localBounds = new Bounds((localMin + localMax) * 0.5f, localMax - localMin);

            return localBounds;
        }
    }
}
