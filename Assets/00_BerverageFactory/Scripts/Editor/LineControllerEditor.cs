using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace FBFactory.Editor
{
    [CustomEditor(typeof(LineController))]
    public class LineControllerEditor : UnityEditor.Editor
    {
        private enum PathSelection
        {
            Normal,
            Figure
        }

        private PathSelection _pathSelection;
        private bool _isBuilding;
        private ProcessingUnit _previousUnit;
        private readonly List<Unit_ConveyerBelt> _pendingConveyers = new();
        private readonly List<ConverterDirectionSetting> _pendingConverterDirections = new();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LineController lineController = (LineController)target;

            if (GUILayout.Button("CollectAllProcessingUnits"))
                lineController.CollectAllProcessingUnits();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Production Path Builder", EditorStyles.boldLabel);
            _pathSelection = (PathSelection)EditorGUILayout.EnumPopup("Path", _pathSelection);

            if (!_isBuilding)
            {
                if (GUILayout.Button("Start Path Builder"))
                    StartPathBuilder();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Scene에서 Unit → Conveyor/Converter → 다음 Unit 순서로 클릭하세요.",
                    MessageType.Info);

                EditorGUILayout.LabelField("Previous Unit", _previousUnit == null ? "None" : _previousUnit.name);
                EditorGUILayout.LabelField("Pending Conveyers", _pendingConveyers.Count.ToString());

                if (GUILayout.Button("Finish Path Builder"))
                    FinishPathBuilder();

                if (GUILayout.Button("Cancel Path Builder"))
                    CancelPathBuilder();
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            if (!_isBuilding || Event.current.type != EventType.MouseDown || Event.current.button != 0)
                return;

            GameObject clickedObject = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            if (clickedObject == null)
                return;

            ProcessingUnit clickedUnit = clickedObject.GetComponentInParent<ProcessingUnit>();
            Unit_ConveyerBelt clickedConveyer = clickedObject.GetComponentInParent<Unit_ConveyerBelt>();

            if (clickedUnit != null)
                SelectUnit(clickedUnit);
            else if (clickedConveyer != null)
                SelectConveyer(clickedConveyer);

            Event.current.Use();
            Repaint();
        }

        private void StartPathBuilder()
        {
            _isBuilding = true;
            _previousUnit = null;
            _pendingConveyers.Clear();
            _pendingConverterDirections.Clear();
            SceneView.RepaintAll();
        }

        private void FinishPathBuilder()
        {
            _isBuilding = false;
            _previousUnit = null;
            _pendingConveyers.Clear();
            _pendingConverterDirections.Clear();
            SceneView.RepaintAll();
        }

        private void CancelPathBuilder()
        {
            _isBuilding = false;
            _previousUnit = null;
            _pendingConveyers.Clear();
            _pendingConverterDirections.Clear();
            SceneView.RepaintAll();
        }

        private void SelectUnit(ProcessingUnit processingUnit)
        {
            if (_previousUnit == null)
            {
                _previousUnit = processingUnit;
                return;
            }

            if (_previousUnit == processingUnit || _pendingConveyers.Count == 0)
                return;

            AddConnection(_previousUnit, processingUnit);
            _previousUnit = processingUnit;
            _pendingConveyers.Clear();
            _pendingConverterDirections.Clear();
        }

        private void SelectConveyer(Unit_ConveyerBelt conveyer)
        {
            if (_previousUnit == null || _pendingConveyers.Contains(conveyer))
                return;

            _pendingConveyers.Add(conveyer);

            if (conveyer is ConverterConveyer converter)
            {
                _pendingConverterDirections.Add(new ConverterDirectionSetting
                {
                    Converter = converter,
                    LocalDirection = GetLocalMoveDirection(converter)
                });
            }
        }

        private void AddConnection(ProcessingUnit previousUnit, ProcessingUnit nextUnit)
        {
            SerializedObject serializedLineController = new SerializedObject(target);
            string pathFieldName = _pathSelection == PathSelection.Normal
                ? "_normalProductionPath"
                : "_figureProductionPath";
            SerializedProperty pathProperty = serializedLineController.FindProperty(pathFieldName);

            if (pathProperty == null || !pathProperty.hasVisibleChildren)
            {
                Debug.LogError("선택한 ProductionPath가 LineController에 설정되어 있지 않습니다.");
                return;
            }

            SerializedProperty connectionsProperty = pathProperty.FindPropertyRelative("Connections");
            int connectionIndex = connectionsProperty.arraySize++;
            SerializedProperty connectionProperty = connectionsProperty.GetArrayElementAtIndex(connectionIndex);

            connectionProperty.FindPropertyRelative("PrevUnit").objectReferenceValue = previousUnit;
            connectionProperty.FindPropertyRelative("NextUnit").objectReferenceValue = nextUnit;
            connectionProperty.FindPropertyRelative("Type").enumValueIndex = (int)ConnectionType.Production;

            SerializedProperty conveyersProperty = connectionProperty.FindPropertyRelative("Conveyers");
            conveyersProperty.arraySize = _pendingConveyers.Count;
            for (int index = 0; index < _pendingConveyers.Count; index++)
                conveyersProperty.GetArrayElementAtIndex(index).objectReferenceValue = _pendingConveyers[index];

            SerializedProperty converterDirectionsProperty = connectionProperty.FindPropertyRelative("ConverterDirections");
            converterDirectionsProperty.arraySize = _pendingConverterDirections.Count;
            for (int index = 0; index < _pendingConverterDirections.Count; index++)
            {
                SerializedProperty directionProperty = converterDirectionsProperty.GetArrayElementAtIndex(index);
                directionProperty.FindPropertyRelative("Converter").objectReferenceValue = _pendingConverterDirections[index].Converter;
                directionProperty.FindPropertyRelative("LocalDirection").vector3Value = _pendingConverterDirections[index].LocalDirection;
            }

            serializedLineController.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            Debug.Log($"[PathBuilder] {previousUnit.name} → {nextUnit.name} Connection 추가");
        }

        private static Vector3 GetLocalMoveDirection(ConverterConveyer converter)
        {
            SerializedObject serializedConverter = new SerializedObject(converter);
            SerializedProperty directionProperty = serializedConverter.FindProperty("_moveDirection");
            return directionProperty == null ? Vector3.right : directionProperty.vector3Value.normalized;

        }
    }
}
