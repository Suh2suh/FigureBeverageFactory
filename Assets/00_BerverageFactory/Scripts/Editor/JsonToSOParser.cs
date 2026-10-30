using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FBFactory.Editor
{
    public class JsonToSOParser : EditorWindow
    {
        private const string k_bomFolder = "Assets/00_BerverageFactory/Data/BOMDefinitions";
        private const string k_materialFolder = "Assets/00_BerverageFactory/Data/MaterialDefinitions";

        private TextAsset _bomJson;
        private TextAsset _materialMasterJson;
        private int _tab;

        [MenuItem("FBFactory/JSON To ScriptableObject")]
        private static void Open()
        {
            GetWindow<JsonToSOParser>("JSON To SO");
        }

        private void OnGUI()
        {
            _tab = GUILayout.Toolbar(_tab, new[] { "BOM", "Material" });
            EditorGUILayout.Space();

            if (_tab == 0)
                DrawBOMTab();
            else
                DrawMaterialTab();
        }

        private void DrawBOMTab()
        {
            _bomJson = (TextAsset)EditorGUILayout.ObjectField("BOM JSON", _bomJson, typeof(TextAsset), false);
            _materialMasterJson = (TextAsset)EditorGUILayout.ObjectField("Material Master JSON", _materialMasterJson, typeof(TextAsset), false);

            EditorGUILayout.Space();
            if (GUILayout.Button("Parse BOM"))
                ParseBOM();
        }

        private void DrawMaterialTab()
        {
            _materialMasterJson = (TextAsset)EditorGUILayout.ObjectField("Material Master JSON", _materialMasterJson, typeof(TextAsset), false);

            EditorGUILayout.Space();
            if (GUILayout.Button("Parse Material Master"))
                ParseMaterials(false);
        }

        private void ParseBOM()
        {
            if (_bomJson == null)
            {
                ShowError("BOM JSON을 지정해줘.");
                return;
            }

            if (!TryParse(_bomJson.text, out BOMJsonRoot root) || root.boms == null)
            {
                ShowError("BOM JSON 구조가 예상과 달라. 최상위에 boms 배열이 필요해.");
                return;
            }

            EnsureFolder(k_bomFolder);

            Dictionary<string, MaterialScriptableObject> materials = LoadMaterials();
            HashSet<string> missingIds = new();

            foreach (BOMJsonData bom in root.boms)
            {
                if (bom == null || string.IsNullOrWhiteSpace(bom.skuId) || bom.components == null)
                {
                    ShowError("BOM 항목에 skuId 또는 components가 없어.");
                    return;
                }

                foreach (BOMComponentJsonData component in bom.components)
                {
                    if (component == null || string.IsNullOrWhiteSpace(component.materialId))
                    {
                        ShowError($"{bom.skuId}의 component에 materialId가 없어.");
                        return;
                    }

                    if (!materials.ContainsKey(component.materialId))
                        missingIds.Add(component.materialId);
                }
            }

            if (missingIds.Count > 0)
            {
                if (_materialMasterJson == null)
                {
                    ShowError("BOM이 참조하는 Material SO가 없고 Material Master JSON도 지정되지 않았어.");
                    return;
                }

                ParseMaterials(true, missingIds);
                materials = LoadMaterials();
            }

            foreach (BOMJsonData bom in root.boms)
            {
                List<BOMScriptableObject.ComponentData> components = new();

                foreach (BOMComponentJsonData component in bom.components)
                {
                    if (!materials.TryGetValue(component.materialId, out MaterialScriptableObject material))
                    {
                        ShowError($"Material Master JSON에 {component.materialId}가 없어.");
                        return;
                    }

                    components.Add(new BOMScriptableObject.ComponentData(material, component.quantity, component.unit));
                }

                string path = $"{k_bomFolder}/{bom.skuId}.asset";
                BOMScriptableObject asset = AssetDatabase.LoadAssetAtPath<BOMScriptableObject>(path);

                if (asset == null)
                {
                    asset = CreateInstance<BOMScriptableObject>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                asset.SetData(bom, components);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"BOM ScriptableObject {root.boms.Count}개를 생성/갱신했어.");
        }

        private void ParseMaterials(bool createMissingOnly, HashSet<string> materialIds = null)
        {
            if (_materialMasterJson == null)
            {
                ShowError("Material Master JSON을 지정해줘.");
                return;
            }

            if (!TryParse(_materialMasterJson.text, out MaterialJsonRoot root) || root.materials == null)
            {
                ShowError("Material Master JSON 구조가 예상과 달라. 최상위에 materials 배열이 필요해.");
                return;
            }

            EnsureFolder(k_materialFolder);
            int count = 0;

            foreach (MaterialJsonData data in root.materials)
            {
                if (data == null || string.IsNullOrWhiteSpace(data.materialId))
                {
                    ShowError("Material 항목에 materialId가 없어.");
                    return;
                }

                string path = $"{k_materialFolder}/{data.materialId}.asset";
                MaterialScriptableObject asset = AssetDatabase.LoadAssetAtPath<MaterialScriptableObject>(path);

                if (asset == null)
                {
                    if (createMissingOnly && (materialIds == null || !materialIds.Contains(data.materialId)))
                        continue;

                    asset = CreateInstance<MaterialScriptableObject>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                if (!TryParseMaterialType(data.materialType, out MaterialType materialType))
                {
                    ShowError($"{data.materialId}의 materialType '{data.materialType}'을(를) MaterialType으로 변환할 수 없어.");
                    return;
                }

                asset.SetData(data.materialId, data.materialName, materialType, data.baseUnit);
                EditorUtility.SetDirty(asset);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!createMissingOnly)
                Debug.Log($"Material ScriptableObject {count}개를 생성/갱신했어.");
        }

        private static bool TryParseMaterialType(string value, out MaterialType materialType)
        {
            if (Enum.TryParse(value, true, out materialType))
                return true;

            if (string.Equals(value, "FILLING", StringComparison.OrdinalIgnoreCase))
            {
                materialType = MaterialType.Filling;
                return true;
            }

            if (string.Equals(value, "FIGURE", StringComparison.OrdinalIgnoreCase))
            {
                materialType = MaterialType.Figure;
                return true;
            }

            materialType = default;
            return false;
        }

        private static Dictionary<string, MaterialScriptableObject> LoadMaterials()
        {
            Dictionary<string, MaterialScriptableObject> result = new();
            string[] guids = AssetDatabase.FindAssets("t:MaterialScriptableObject", new[] { k_materialFolder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MaterialScriptableObject asset = AssetDatabase.LoadAssetAtPath<MaterialScriptableObject>(path);

                if (asset != null && !string.IsNullOrWhiteSpace(asset.MaterialId))
                    result[asset.MaterialId] = asset;
            }

            return result;
        }

        private static bool TryParse<T>(string json, out T result) where T : class
        {
            try
            {
                result = JsonUtility.FromJson<T>(json);
                return result != null;
            }
            catch (Exception exception)
            {
                result = null;
                Debug.LogException(exception);
                return false;
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/00_BerverageFactory/Data", folder.Substring(folder.LastIndexOf('/') + 1));
        }

        private static void ShowError(string message)
        {
            Debug.LogError(message);
            EditorUtility.DisplayDialog("JSON To SO", message, "확인");
        }

        [Serializable]
        private class BOMJsonRoot
        {
            public List<BOMJsonData> boms;
        }

        [Serializable]
        private class MaterialJsonRoot
        {
            public List<MaterialJsonData> materials;
        }

        [Serializable]
        private class MaterialJsonData
        {
            public string materialId;
            public string materialName;
            public string materialType;
            public string baseUnit;
        }
    }
}
