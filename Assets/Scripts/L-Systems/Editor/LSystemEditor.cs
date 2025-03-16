using UnityEngine;
using UnityEditor;

namespace LSystemEngine
{
    [CustomEditor(typeof(LSystem))]
    public class LSystemEditor : Editor
    {
        bool showDraw = true;
        bool showSystem = true;
        bool showIterationRules = false;
        bool showDrawInstructions = false;

        public override void OnInspectorGUI()
        {
            SerializedProperty background = serializedObject.FindProperty("background");
            SerializedProperty colors = serializedObject.FindProperty("colors");
            SerializedProperty isGradient = serializedObject.FindProperty("isGradient");
            SerializedProperty gradientCount = serializedObject.FindProperty("gradientCount");
            SerializedProperty lineMaterial = serializedObject.FindProperty("lineMaterial");
            SerializedProperty lineWidthStart = serializedObject.FindProperty("lineWidthStart");
            SerializedProperty lineWidthEnd = serializedObject.FindProperty("lineWidthEnd");
            SerializedProperty rules = serializedObject.FindProperty("rules");
            SerializedProperty drawInstructions = serializedObject.FindProperty("drawInstructions");
            SerializedProperty iterationCount = serializedObject.FindProperty("iterationCount");
            SerializedProperty iterationTime = serializedObject.FindProperty("iterationTime");
            SerializedProperty drawTime = serializedObject.FindProperty("drawTime");
            SerializedProperty drawDistance = serializedObject.FindProperty("drawDistance");
            SerializedProperty turnAngle = serializedObject.FindProperty("turnAngle");
            SerializedProperty axiom = serializedObject.FindProperty("axiom");

            GUIStyle style = EditorStyles.foldout;
            style.fontStyle = FontStyle.Bold;
            if (showDraw = EditorGUILayout.Foldout(showDraw, "Draw Options", style))
            {
                style.fontStyle = FontStyle.Normal;
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(background, new GUIContent("Background Color"));
                EditorGUILayout.PropertyField(colors, new GUIContent("Draw Colors"), true);
                EditorGUILayout.PropertyField(isGradient, new GUIContent("Gradient"));
                if (isGradient.boolValue) EditorGUILayout.PropertyField(gradientCount, new GUIContent("Gradient Samples"));

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(lineMaterial, new GUIContent("Line Material"));
                EditorGUILayout.LabelField("Line Width");
                GUILayout.BeginHorizontal();
                GUILayout.Space(14f + (EditorGUI.indentLevel + 1) * 14f);
                GUILayout.Label("Start:");
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(lineWidthStart, GUIContent.none);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(14f + (EditorGUI.indentLevel + 1) * 14f);
                GUILayout.Label("End:");
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(lineWidthEnd, GUIContent.none);
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(drawTime);
                EditorGUILayout.PropertyField(iterationTime);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        
            style.fontStyle = FontStyle.Bold;
            if (showSystem = EditorGUILayout.Foldout(showSystem, "System Rules", style))
            {
                style.fontStyle = FontStyle.Normal;
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                if (showIterationRules = EditorGUILayout.Foldout(showIterationRules, "Iteration Rules", style))
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+"))
                    {
                        int i = rules.arraySize > 0 ? rules.arraySize - 1 : 0;
                        rules.InsertArrayElementAtIndex(i);
                    }
                    if (GUILayout.Button("-"))
                        rules.DeleteArrayElementAtIndex(rules.arraySize - 1);
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < rules.arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(rules.GetArrayElementAtIndex(i));
                        EditorGUILayout.Space();
                    }
                }
                else GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (showDrawInstructions = EditorGUILayout.Foldout(showDrawInstructions, "Draw Rules", style))
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+"))
                    {
                        int i = drawInstructions.arraySize > 0 ? drawInstructions.arraySize - 1 : 0;
                        drawInstructions.InsertArrayElementAtIndex(i);
                    }
                    if (GUILayout.Button("-"))
                        drawInstructions.DeleteArrayElementAtIndex(drawInstructions.arraySize - 1);
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < drawInstructions.arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(drawInstructions.GetArrayElementAtIndex(i));
                        EditorGUILayout.Space();
                    }
                }
                else GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(drawDistance, new GUIContent("Distance Increment"));
                EditorGUILayout.PropertyField(turnAngle, new GUIContent("Initial Turn Angle"));

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(axiom, new GUIContent("Axiom"));
                EditorGUILayout.PropertyField(iterationCount);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
