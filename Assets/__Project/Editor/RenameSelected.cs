using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class RenameSelected : EditorWindow {
    private static readonly Vector2Int _size = new Vector2Int(300, 80);

    private string _gameObjectPrefix;
    private int _startIndex = 1;

    [MenuItem("Tools/Rename Selected")]
    public static void ShowWindow() {
        var window = GetWindow<RenameSelected>();
        window.minSize = _size;
    }

    // Disable the menu item if there is no Hierarchy GameObject selection.
    [MenuItem("Tools/Rename Selected", true)]
    static bool Validate() {
        return Selection.objects.Length > 0;
    }

    private void OnGUI() {
        if (Selection.objects.Length > 0 && string.IsNullOrEmpty(_gameObjectPrefix)) {
            var regex = new Regex(" ([([{])?\\d.*$");
            _gameObjectPrefix = regex.Replace(Selection.objects[0].name, string.Empty).Trim();
        }

        _gameObjectPrefix = EditorGUILayout.TextField("Selected Prefix", _gameObjectPrefix).Trim();
        _startIndex = EditorGUILayout.IntField("Start Index", _startIndex);

        GUI.enabled = false;
        EditorGUILayout.LabelField($"Selected {Selection.objects.Length} objects");
        GUI.enabled = true;

        if (GUILayout.Button("Rename Objects")) {
            GameObject[] unsortedGameObjects = Selection.gameObjects;
            int objectsCount = unsortedGameObjects.Length;
            var sortedGameObjects = new GameObject[objectsCount];
            for (var i = 0; i < objectsCount; i++) {
                sortedGameObjects[i] = unsortedGameObjects[i];
            }
            // sort gameobjects by sibling index
            Array.Sort(sortedGameObjects, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            Undo.RecordObjects((UnityEngine.Object[]) sortedGameObjects, "Rename objects");

            for (int i = 0; i < objectsCount; i++) {
                sortedGameObjects[i].name = $"{_gameObjectPrefix} ({_startIndex + i})";
            }

            Close();
        }
    }
}
