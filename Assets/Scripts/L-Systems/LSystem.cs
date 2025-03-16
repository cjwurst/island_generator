using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEditor;

namespace LSystemEngine
{
    [CreateAssetMenu(menuName = "L-System")]
    public class LSystem : ScriptableObject
    {
        [SerializeField]
        Color background;
        [SerializeField]
        Color[] colors;
        [SerializeField]
        bool isGradient = false;
        [SerializeField]
        int gradientCount;
        ColorPicker colorPicker;
        public int colorIndex
        {
            get { return colorPicker.i; }
            set { colorPicker.i = value; }
        }

        [SerializeField]
        Material lineMaterial;
        [SerializeField]
        float lineWidthStart;
        [SerializeField]
        float lineWidthEnd;
        IDrawTool drawTool;

        public LRule[] rules;

        public DrawInstruction[] drawInstructions;

        public int iterationCount;
        public float iterationTime;
        public float drawTime;

        [SerializeField]
        float turnAngle;
        float runtimeTurnAngle;
        float turnRadians { get { return 2f * Mathf.PI * runtimeTurnAngle / 360f; } }
        public float drawDistance;

        public string axiom;

        float firstAngle;

        Vector2 turtlePosition = Vector2.zero;
        float turtleAngle = 0f;

        float savedAngle;

        public void Init()
        {
            Camera.main.backgroundColor = background;

            colorPicker = new ColorPicker(isGradient, gradientCount, colors);
            drawTool = new DisplayDrawTool(lineWidthStart, lineWidthEnd);

            runtimeTurnAngle = turnAngle;

            Reset();
        }

        public void Init(Texture2D texture, Vector2Int origin, float _firstAngle)
        {
            firstAngle = _firstAngle;

            colorPicker = new ColorPicker(isGradient, gradientCount, colors);
            drawTool = new TextureDrawTool(texture, origin);

            runtimeTurnAngle = turnAngle;
        }

        #if UNITY_EDITOR
        [MenuItem("Assets/Duplicate L-System")]
        static void Duplicate()
        {
            LSystem duplicate = Instantiate(Selection.activeObject as LSystem);

            string duplicandPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string localDuplicandName = Path.GetFileNameWithoutExtension(duplicandPath);
            string[] duplicandDirectoryPath = Path.GetDirectoryName(duplicandPath).Replace("\\", "/").Split("/".ToCharArray());
            string duplicandFolderName = duplicandDirectoryPath[duplicandDirectoryPath.Length - 1];
            UnityEngine.Object[] assets = Resources.LoadAll(duplicandFolderName, typeof(LSystem));
            int duplicateCount = 0;
            foreach (UnityEngine.Object candidate in assets) if (AssetIsDuplicate(candidate)) duplicateCount++;

            AssetDatabase.CreateAsset(duplicate, Path.ChangeExtension(duplicandPath, null) + string.Format("({0})", duplicateCount) + ".asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = duplicate;

            bool AssetIsDuplicate(UnityEngine.Object asset)
            {
                string localName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(asset));
                return localName.StartsWith(localDuplicandName + "(") || localName == localDuplicandName;
            }
        }
        [MenuItem("Assets/Duplicate L-System", true)]
        static bool AssetIsLSystem() { return Selection.activeObject is LSystem; }
        #endif

        public void Reset()
        {
            drawTool.Reset();
            colorPicker.Reset();
            StartLine(Vector3.zero, lineMaterial, colorPicker.color);
        }

        public virtual string IterateString(string s)
        {
            char[] chars = s.ToCharArray();
            string output = "";
            for (int i = 0; i < chars.Length; i++)
            {
                bool ruleApplied = false;
                foreach (LRule rule in rules)
                {
                    if (chars[i] == rule.input)
                    {
                        output += rule.output;
                        ruleApplied = true;
                    }
                }
                if (!ruleApplied)
                    output += chars[i];
            }
            //MonoBehaviour.print(output + "\nCharacter Count: " + output.Length + " Sign Count: " + output.Count((x) => x == '+' || x == '-'));
            return output;
        }

        void ResetTurtle()
        {
            turtlePosition = Vector3.zero;
            turtleAngle = firstAngle;
        }

        public virtual void DrawString(string s, bool willRender)
        {
            ResetTurtle();

            List<Tuple<float, Vector2>> savedStates = new List<Tuple<float, Vector2>>();
            List<int> savedColors = new List<int>();

            char[] drawChars = s.ToCharArray();
            for (int i = 0; i < drawChars.Length; i++)
            {
                foreach(DrawInstruction drawInstruction in drawInstructions)
                {
                    if (drawInstruction.input == drawChars[i])
                    {
                        switch (drawInstruction.output)
                        {
                            case DrawInstruction.Instruction.Forward:
                                if (!willRender) break;
                                DrawForward();
                                break;

                            case DrawInstruction.Instruction.TurnLeft:
                                if (!willRender) break;
                                turtleAngle += turnRadians;
                                break;

                            case DrawInstruction.Instruction.TurnRight:
                                if (!willRender) break;
                                turtleAngle -= turnRadians;
                                break;

                            case DrawInstruction.Instruction.HalfAngle:
                                runtimeTurnAngle /= 2f;
                                break;

                            case DrawInstruction.Instruction.Save:
                                if (!willRender) break;
                                savedStates.Add(new Tuple<float, Vector2>(turtleAngle, turtlePosition));
                                savedColors.Add(colorIndex);
                                break;

                            case DrawInstruction.Instruction.Load:
                                if (!willRender) break;
                                Tuple<float, Vector2> state = savedStates[savedStates.Count - 1];
                                savedStates.RemoveAt(savedStates.Count - 1);
                                turtleAngle = state.Item1;
                                turtlePosition = state.Item2;

                                colorIndex = savedColors[savedColors.Count - 1];
                                savedColors.RemoveAt(savedColors.Count - 1);

                                Jump(turtlePosition);
                                break;

                            case DrawInstruction.Instruction.ChangeColor:
                                if (!willRender) break;
                                ChangeColor();
                                break;
                        }
                    }
                }
            }
        }

        void DrawForward()
        {
            Vector2 target = turtlePosition + new Vector2(Mathf.Cos(turtleAngle) * drawDistance, Mathf.Sin(turtleAngle) * drawDistance);
            drawTool.DrawLine(target);
            turtlePosition = target;
        }

        void ChangeColor()
        {
            colorPicker.Iterate();
            drawTool.StartLine(lineMaterial, colorPicker.color);
        }

        void Jump(Vector2 target)
        {
            drawTool.StartLine(target, lineMaterial, colorPicker.color);
        }

        public void StartLine(Vector3 start, Material material, Color color) { drawTool.StartLine(start, material, color); }

        interface IDrawTool
        {
            void StartLine(Vector3 start, Material material, Color color);
            void StartLine(Material material, Color color);
            void DrawLine(Vector3 end);
            void Reset();
        }

        class TextureDrawTool : IDrawTool
        {
            Texture2D texture;

            Vector2Int origin;

            Vector2Int currentPosition = Vector2Int.zero;
            Color currentColor = Color.white;

            public TextureDrawTool(Texture2D _texture, Vector2Int _origin)
            {
                texture = _texture;

                origin = _origin;
            }

            public void StartLine(Vector3 start, Material _, Color color)
            {
                currentPosition = Scale(start);
                currentColor = color;
            }
            public void StartLine(Material _, Color color)
            {
                StartLine(new Vector3(currentPosition.x, currentPosition.y, 0f), null, color);
            }

            public void DrawLine(Vector3 end)
            {
                var endPosition = Scale(end);
                DrawLineToTexture(texture, currentPosition.x, currentPosition.y, endPosition.x, endPosition.y, currentColor);
                currentPosition = endPosition;

                void DrawLineToTexture(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
                {
                    float xPix = x1;
                    float yPix = y1;

                    float width = x2 - x1;
                    float height = y2 - y1;
                    float length = Mathf.Abs(width);
                    if (Mathf.Abs(height) > length) length = Mathf.Abs(height);
                    int intLength = (int)length;
                    float dx = width / length;
                    float dy = height / length;
                    for (int i = 0; i <= intLength; i++)
                    {
                        if (Validate((int)xPix, (int)yPix))
                            a_Texture.SetPixel((int)xPix, (int)yPix, a_Color);

                        xPix += dx;
                        yPix += dy;
                    }

                    bool Validate(int x, int y)
                    {
                        if
                        (
                            x > a_Texture.width - 1
                            || x < 0
                            || y > a_Texture.height - 1
                            || y < 0
                        )
                            return false;
                        return true;
                    }
                }
            }

            Vector2Int Scale(Vector3 v)
            {
                var i = Mathf.RoundToInt(texture.width * v.x);
                var j = Mathf.RoundToInt(texture.height * v.y);
                return origin + new Vector2Int(i, j);
            }

            public void Reset() { }
        }

        class DisplayDrawTool : IDrawTool
        {
            List<GameObject> lines = new List<GameObject>();

            float lineWidthStart;
            float lineWidthEnd;

            public DisplayDrawTool(float _lineWidthStart, float _lineWidthEnd)
            {
                lineWidthStart = _lineWidthStart;
                lineWidthEnd = _lineWidthEnd;
            }

            public void StartLine(Vector3 start, Material material, Color color)
            {
                GameObject line = new GameObject();
                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();

                lineRenderer.material = material;
                lineRenderer.material.color = color;
                lineRenderer.startWidth = lineWidthStart;
                lineRenderer.endWidth = lineWidthEnd;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.receiveShadows = false;
                lineRenderer.useWorldSpace = false;
                lineRenderer.alignment = LineAlignment.TransformZ;
                lineRenderer.numCapVertices = 5;
                lineRenderer.numCornerVertices = 5;

                lineRenderer.positionCount = 1;
                lineRenderer.SetPosition(0, start);

                lines.Add(line);
            }
            public void StartLine(Material material, Color color)
            {
                LineRenderer lineRenderer = lines[lines.Count - 1].GetComponent<LineRenderer>();
                StartLine(lineRenderer.GetPosition(lineRenderer.positionCount - 1), material, color);
            }

            public void DrawLine(Vector3 end)
            {
                LineRenderer lineRenderer = lines[lines.Count - 1].GetComponent<LineRenderer>();
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, end);
            }

            public void Reset()
            {
                if (lines.Count == 0) return;
                foreach (GameObject line in lines) GameObject.Destroy(line);
                lines = new List<GameObject>();
            }
        }

        class ColorPicker
        {
            public Color color { get { return colors[i]; } }
            Color[] colors;
            public int i = 0;
            bool isGradient = false;

            public ColorPicker(bool _isGradient, int gradientCount, params Color[] _colors)
            {
                isGradient = _isGradient;
                if (!isGradient)
                {
                    colors = _colors;
                    return;
                }

                Color target = _colors[_colors.Length - 1];
                colors = new Color[gradientCount];
                float increment = 1f / (gradientCount - 1);
                for (int i = 0; i < gradientCount; i++)
                    colors[i] = Color.Lerp(_colors[0], target, i * increment);
            }

            public void Iterate()
            {
                i++;
                if (i == colors.Length)
                {
                    if (isGradient)
                    {
                        i = colors.Length - 1;
                        return;
                    }
                    i = 0;
                }
            }

            public void Reset() { i = 0; }
        }
    }
}


