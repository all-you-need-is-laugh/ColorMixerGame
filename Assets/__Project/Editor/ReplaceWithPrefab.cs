using UnityEditor;
using UnityEngine;

public class ReplaceWithPrefab : EditorWindow {
    private static readonly Vector2Int _size = new Vector2Int(500, 80);

    [SerializeField]
    private GameObject _prefab;

    private static bool _keepOpened;

    [MenuItem("Tools/Replace With Prefab")]
    static void ShowWindow() {
        var window = EditorWindow.GetWindow<ReplaceWithPrefab>();
        window.minSize = window.maxSize = _size;
    }

    // Disable the menu item if there is no Hierarchy GameObject selection.
    [MenuItem("Tools/Replace With Prefab", true)]
    static bool Validate() {
        return Selection.objects.Length > 0;
    }

    private void OnGUI() {
        _prefab = (GameObject) EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
        _keepOpened = EditorGUILayout.Toggle("Keep opened", _keepOpened);

        GUI.enabled = false;
        EditorGUILayout.LabelField($"Selected {Selection.objects.Length} objects");
        GUI.enabled = true;

        if (GUILayout.Button("Replace")) {
            GameObject[] selection = Selection.gameObjects;

            foreach (var selected in selection) {
                GameObject newObject = (GameObject) PrefabUtility.InstantiatePrefab(_prefab);

                if (newObject == null) {
                    Debug.LogError("Error instantiating prefab");
                    break;
                }

                Undo.RegisterCreatedObjectUndo(newObject, "Replace with prefab");
                newObject.name = selected.name;
                Undo.SetTransformParent(newObject.transform, selected.transform.parent, "Specify parent to new object");
                Undo.RecordObject(newObject.transform, "Copy object transform");
                newObject.transform.localPosition = selected.transform.localPosition;
                newObject.transform.localRotation = selected.transform.localRotation;
                newObject.transform.localScale = selected.transform.localScale;
                newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                Undo.DestroyObjectImmediate(selected);
            }

            if (!_keepOpened) {
                Close();
            }
        }
    }
}
