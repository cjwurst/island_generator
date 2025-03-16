using UnityEngine;
using UnityEditor;

namespace LSystemEngine
{
    [CustomPropertyDrawer(typeof(WeightedRuleOutput))]
    public class WeightedRuleOutputDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect outputRect = new Rect(position.x, position.y, 150f, position.height);
            Rect weightRect = new Rect(position.width - 35f, position.y, 50f, position.height);
            Rect labelRect = new Rect(weightRect.x - 10f, weightRect.y, weightRect.width, weightRect.height);

            EditorGUI.LabelField(labelRect, new GUIContent("P"));
            EditorGUI.PropertyField(outputRect, property.FindPropertyRelative("output"), GUIContent.none);
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("baseWeight"), GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(LRule))]
    public class LRuleDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;

            height += EditorGUIUtility.singleLineHeight;                                    // for input

            SerializedProperty outputArray = property.FindPropertyRelative("outputs");
            height += outputArray.arraySize * EditorGUIUtility.singleLineHeight;            // for outputs

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            SerializedProperty outputArray = property.FindPropertyRelative("outputs");

            Rect inputRect = new Rect(position.x, position.y, 50f, lineHeight);
            Rect addRect = new Rect(inputRect.x + inputRect.width + 10f, inputRect.y, inputRect.width - 10f, inputRect.height);
            Rect removeRect = new Rect(inputRect.x + inputRect.width + addRect.width + 20f, inputRect.y, inputRect.width - 10f, inputRect.height);
            Rect firstOutputRect = new Rect(position.x + 14f, position.y + lineHeight, position.width, lineHeight);

            EditorGUI.PropertyField(inputRect, property.FindPropertyRelative("input"), GUIContent.none);
            if (GUI.Button(addRect, new GUIContent("+")))
            {
                int i = outputArray.arraySize > 0 ? outputArray.arraySize - 1 : 0;
                outputArray.InsertArrayElementAtIndex(i);
            }
            if (GUI.Button(removeRect, new GUIContent("-")) && outputArray.arraySize > 0)
            {
                outputArray.DeleteArrayElementAtIndex(outputArray.arraySize - 1);
            }
            for (int i = 0; i < outputArray.arraySize; i++)
            {
                SerializedProperty rule = outputArray.GetArrayElementAtIndex(i);

                Rect outputRect = new Rect(firstOutputRect.x, firstOutputRect.y + i * lineHeight, firstOutputRect.width, firstOutputRect.height);

                EditorGUI.PropertyField(outputRect, rule, GUIContent.none);
            }
        }
    }

    [CustomPropertyDrawer(typeof(WeightedDrawOutput))]
    public class WeightedDrawOutputDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect outputRect = new Rect(position.x, position.y, 150f, position.height);
            Rect weightRect = new Rect(position.width - 35f, position.y, 50f, position.height);
            Rect labelRect = new Rect(weightRect.x - 10f, weightRect.y, weightRect.width, weightRect.height);

            EditorGUI.LabelField(labelRect, new GUIContent("P"));
            EditorGUI.PropertyField(outputRect, property.FindPropertyRelative("output"), GUIContent.none);
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("baseWeight"), GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(DrawInstruction))]
    public class DrawInstructionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;

            height += EditorGUIUtility.singleLineHeight;                                    // for input

            SerializedProperty outputArray = property.FindPropertyRelative("outputs");
            height += outputArray.arraySize * EditorGUIUtility.singleLineHeight;            // for outputs

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            SerializedProperty outputArray = property.FindPropertyRelative("outputs");

            Rect inputRect = new Rect(position.x, position.y, 50f, lineHeight);
            Rect addRect = new Rect(inputRect.x + inputRect.width + 10f, inputRect.y, inputRect.width - 10f, inputRect.height);
            Rect removeRect = new Rect(inputRect.x + inputRect.width + addRect.width + 20f, inputRect.y, inputRect.width - 10f, inputRect.height);
            //Rect arrowRect = new Rect(position.x + 30f, position.y, 100f, lineHeight);
            Rect firstOutputRect = new Rect(position.x + 14f, position.y + lineHeight, position.width, lineHeight);

            EditorGUI.PropertyField(inputRect, property.FindPropertyRelative("input"), GUIContent.none);
            if (GUI.Button(addRect, new GUIContent("+")))
            {
                int i = outputArray.arraySize > 0 ? outputArray.arraySize - 1 : 0;
                outputArray.InsertArrayElementAtIndex(i);
            }
            if (GUI.Button(removeRect, new GUIContent("-")) && outputArray.arraySize > 0)
            {
                outputArray.DeleteArrayElementAtIndex(outputArray.arraySize - 1);
            }
            for (int i = 0; i < outputArray.arraySize; i++)
            {
                SerializedProperty rule = outputArray.GetArrayElementAtIndex(i);

                Rect outputRect = new Rect(firstOutputRect.x, firstOutputRect.y + i * lineHeight, firstOutputRect.width, firstOutputRect.height);

                EditorGUI.PropertyField(outputRect, rule, GUIContent.none);
            }
        }
    }
}


