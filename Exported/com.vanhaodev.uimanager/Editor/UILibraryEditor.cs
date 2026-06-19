using UnityEditor;
using UnityEngine;

namespace vanhaodev.uimanager.editor
{
    /// <summary>
    /// Tabbed inspector for <see cref="UILibrary"/>. One tab per feature, each showing only that
    /// feature's prefab(s) and settings. Rendering only — no serialization is changed.
    /// </summary>
    [CustomEditor(typeof(UILibrary))]
    public class UILibraryEditor : Editor
    {
        private const string TabStateKey = "vanhaodev.uimanager.lib.tab";

        private static readonly string[] Tabs =
        {
            "Screen", "Popup", "Toast", "Loading", "Floating", "Flyout", "Click"
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int tab = SessionState.GetInt(TabStateKey, 0);
            tab = GUILayout.Toolbar(tab, Tabs);
            SessionState.SetInt(TabStateKey, tab);
            EditorGUILayout.Space(6);

            switch (tab)
            {
                case 0: Body("Full-screen pages, one shown at a time.",
                    "_screens"); break;
                case 1: Body("Dialogs stacked over the current screen.",
                    "_popups"); break;
                case 2: Body("Short auto-dismissing messages.",
                    "_toasts", "_maxConcurrentToasts", "_toastSpacing", "_toastPadding"); break;
                case 3: Body("Full-screen overlay that blocks input while work runs.",
                    "_loadingBlocks"); break;
                case 4: Body("Pop-and-float labels (e.g. \"+100\").",
                    "_floatingTexts", "_floatingShowInterval", "_floatingMaxPerSource", "_floatingScreenPadding"); break;
                case 5: Body("Icons that fly into a counter.",
                    "_flyoutIconPrefab", "_flyoutConfig"); break;
                case 6: Body("Ripple played at each touch point.",
                    "_clickEffectConfig"); break;
            }

            serializedObject.ApplyModifiedProperties();

            DrawPrefabRestore();
        }

        // Keep the GUID snapshot fresh while healthy, and offer a one-click restore when Unity drops
        // a prefab reference (a list slot turns into "None").
        private void DrawPrefabRestore()
        {
            var lib = (UILibrary)target;

            if (lib.SyncPrefabGuids())
                EditorUtility.SetDirty(lib);

            EditorGUILayout.Space(10);

            bool missing = lib.HasMissingPrefabReferences();
            if (missing)
                EditorGUILayout.HelpBox(
                    "Some prefab references are missing. Click Reload to put them back from the saved paths.",
                    MessageType.Warning);

            using (new EditorGUI.DisabledScope(!missing))
            {
                if (GUILayout.Button("Reload missing prefab references"))
                {
                    Undo.RecordObject(lib, "Reload Prefab References");
                    lib.ReloadPrefabReferences();
                    EditorUtility.SetDirty(lib);
                    serializedObject.Update();
                }
            }
        }

        // A short note describing the feature, then its fields.
        private void Body(string hint, params string[] propNames)
        {
            if (!string.IsNullOrEmpty(hint))
            {
                var note = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
                EditorGUILayout.LabelField(hint, note);
            }

            EditorGUILayout.Space(2);
            foreach (var name in propNames)
            {
                var prop = serializedObject.FindProperty(name);
                if (prop != null)
                    EditorGUILayout.PropertyField(prop, includeChildren: true);
            }
            EditorGUILayout.Space(2);
        }
    }
}
