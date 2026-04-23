using UnityEngine;
using UnityEditor;

namespace Vanhaodev.UIManager.Editor
{
    [CustomEditor(typeof(UILibrary))]
    public class UILibraryEditor : UnityEditor.Editor
    {
        private SerializedProperty _screens;
        private SerializedProperty _popups;
        private bool _screensFoldout = true;
        private bool _popupsFoldout = true;

        private void OnEnable()
        {
            _screens = serializedObject.FindProperty("_screens");
            _popups = serializedObject.FindProperty("_popups");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);

            // Header
            EditorGUILayout.LabelField("UI Library", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Register your Screen and Popup prefabs here.\n" +
                "ID can be left empty - it will use the class name by default.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Screens Section
            _screensFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_screensFoldout, $"Screens ({_screens.arraySize})");
            if (_screensFoldout)
            {
                EditorGUI.indentLevel++;
                DrawArrayWithAddRemove(_screens, "Screen");
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Popups Section
            _popupsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_popupsFoldout, $"Popups ({_popups.arraySize})");
            if (_popupsFoldout)
            {
                EditorGUI.indentLevel++;
                DrawArrayWithAddRemove(_popups, "Popup");
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            // Utility buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Auto-fill Empty IDs"))
            {
                AutoFillIds();
            }

            if (GUILayout.Button("Validate"))
            {
                ValidateLibrary();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawArrayWithAddRemove(SerializedProperty array, string elementName)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                var idProp = element.FindPropertyRelative("Id");
                var prefabProp = element.FindPropertyRelative("Prefab");

                EditorGUILayout.BeginHorizontal();

                // Index label
                EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));

                // ID field
                EditorGUILayout.PropertyField(idProp, GUIContent.none, GUILayout.Width(150));

                // Prefab field
                EditorGUILayout.PropertyField(prefabProp, GUIContent.none);

                // Remove button
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    array.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button($"+ Add {elementName}"))
            {
                array.InsertArrayElementAtIndex(array.arraySize);
                var newElement = array.GetArrayElementAtIndex(array.arraySize - 1);
                newElement.FindPropertyRelative("Id").stringValue = "";
                newElement.FindPropertyRelative("Prefab").objectReferenceValue = null;
            }
        }

        private void AutoFillIds()
        {
            serializedObject.Update();

            FillIdsForArray(_screens);
            FillIdsForArray(_popups);

            serializedObject.ApplyModifiedProperties();
            Debug.Log("[UILibrary] Auto-filled empty IDs from prefab names.");
        }

        private void FillIdsForArray(SerializedProperty array)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                var idProp = element.FindPropertyRelative("Id");
                var prefabProp = element.FindPropertyRelative("Prefab");

                if (string.IsNullOrEmpty(idProp.stringValue) && prefabProp.objectReferenceValue != null)
                {
                    var prefab = prefabProp.objectReferenceValue;
                    idProp.stringValue = prefab.GetType().Name;
                }
            }
        }

        private void ValidateLibrary()
        {
            var library = (UILibrary)target;
            int issues = 0;

            // Check screens
            for (int i = 0; i < _screens.arraySize; i++)
            {
                var element = _screens.GetArrayElementAtIndex(i);
                var prefabProp = element.FindPropertyRelative("Prefab");

                if (prefabProp.objectReferenceValue == null)
                {
                    Debug.LogWarning($"[UILibrary] Screen at index {i} has null prefab.");
                    issues++;
                }
            }

            // Check popups
            for (int i = 0; i < _popups.arraySize; i++)
            {
                var element = _popups.GetArrayElementAtIndex(i);
                var prefabProp = element.FindPropertyRelative("Prefab");

                if (prefabProp.objectReferenceValue == null)
                {
                    Debug.LogWarning($"[UILibrary] Popup at index {i} has null prefab.");
                    issues++;
                }
            }

            if (issues == 0)
                Debug.Log("[UILibrary] Validation passed. No issues found.");
            else
                Debug.LogWarning($"[UILibrary] Validation found {issues} issue(s).");
        }
    }
}
