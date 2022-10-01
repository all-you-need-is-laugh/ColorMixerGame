using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CountedIngredient))]
public class CountedIngredientDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var countRect = new Rect(position.x, position.y + 1, 30, position.height - 2);
        var ingredientRect = new Rect(position.x + 35, position.y + 1, position.width - 35, position.height - 2);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(countRect, property.FindPropertyRelative("count"), GUIContent.none);
        EditorGUI.PropertyField(ingredientRect, property.FindPropertyRelative("ingredient"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
