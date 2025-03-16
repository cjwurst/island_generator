using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Exhaustible))]
public class ExhaustibleDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var currentRect = new Rect(position.x, position.y, 30, position.height);
        var slashRect = new Rect(position.x + 35, position.y, 20, position.height);
        var totalRect = new Rect(position.x + 60, position.y, 30, position.height);

        EditorGUI.LabelField(currentRect, property.FindPropertyRelative("currentDisplay").intValue.ToString());
        EditorGUI.LabelField(slashRect, "/");
        EditorGUI.LabelField(totalRect, property.FindPropertyRelative("totalDisplay").intValue.ToString());

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
