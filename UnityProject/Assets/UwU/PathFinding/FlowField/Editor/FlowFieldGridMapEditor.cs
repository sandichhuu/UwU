using UnityEditor;
using UnityEngine;

namespace UwU.PathFinding.FlowField
{
    [CustomEditor(typeof(FlowFieldGridMapBehaviour))]
    public class FlowFieldGridMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var gridMapBehaviour = this.target as FlowFieldGridMapBehaviour;
            var cellStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };

            RenderPreview(gridMapBehaviour, cellStyle);
            RenderObstaclesConfigGroup(gridMapBehaviour, cellStyle);
            RenderStartConfigGroup(gridMapBehaviour, cellStyle);
            RenderTargetConfigGroup(gridMapBehaviour, cellStyle);
        }

        private void RenderStartConfigGroup(FlowFieldGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
        {
            var gridMap = gridMapBehaviour.GetGridMap();
            var width = gridMap.width;
            var height = gridMap.height;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Start Config", EditorStyles.boldLabel);

            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (var x = 0; x < width; x++)
                {
                    var gridCell = gridMap[x, y];
                    if (gridCell != null)
                    {
                        var originalColor = GUI.backgroundColor;
                        var index = gridMap.GetIndex(x, y);
                        var location = new Vector2Int(x, y);

                        if (gridCell.IsObstacle)
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }

                        if (gridMapBehaviour.startCells.Contains(location))
                        {
                            GUI.backgroundColor = Color.teal;
                        }

                        if (GUILayout.Button(string.Empty, cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            gridMapBehaviour.ToggleStartCell(location);
                            EditorUtility.SetDirty(gridMapBehaviour);
                        }

                        GUI.backgroundColor = originalColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderTargetConfigGroup(FlowFieldGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
        {
            var gridMap = gridMapBehaviour.GetGridMap();
            var width = gridMap.width;
            var height = gridMap.height;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Target Config", EditorStyles.boldLabel);

            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (var x = 0; x < width; x++)
                {
                    var gridCell = gridMap[x, y];
                    if (gridCell != null)
                    {
                        var location = new Vector2Int(x, y);
                        var originalColor = GUI.backgroundColor;

                        if (gridCell.IsObstacle)
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }

                        if (gridMapBehaviour.targetCells.Contains(location))
                        {
                            GUI.backgroundColor = Color.magenta;
                        }

                        if (GUILayout.Button(string.Empty, cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            gridMapBehaviour.ToggleTargetCell(location);
                            EditorUtility.SetDirty(gridMapBehaviour);
                        }

                        GUI.backgroundColor = originalColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderObstaclesConfigGroup(FlowFieldGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
        {
            var gridMap = gridMapBehaviour.GetGridMap();
            var width = gridMap.width;
            var height = gridMap.height;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Obstacle Config", EditorStyles.boldLabel);

            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (var x = 0; x < width; x++)
                {
                    var gridCell = gridMap[x, y];
                    if (gridCell != null)
                    {
                        var originalColor = GUI.backgroundColor;
                        if (gridCell.IsObstacle)
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }

                        if (GUILayout.Button(string.Empty, cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            gridMap.Toggle(x, y);
                            EditorUtility.SetDirty(gridMapBehaviour);
                        }

                        GUI.backgroundColor = originalColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderPreview(FlowFieldGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
        {
            var gridMap = gridMapBehaviour.GetGridMap();
            var width = gridMap.width;
            var height = gridMap.height;

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            if (GUILayout.Button("Compute", GUILayout.Width(100), GUILayout.Height(22)))
            {
                gridMapBehaviour.Compute();
            }
            EditorGUILayout.EndHorizontal();

            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (var x = 0; x < width; x++)
                {
                    var gridCell = gridMap[x, y];
                    if (gridCell != null)
                    {
                        var originalColor = GUI.backgroundColor;
                        var location = new Vector2Int(x, y);

                        if (gridCell.IsObstacle)
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }

                        if (gridMapBehaviour.startCells.Contains(location))
                        {
                            GUI.backgroundColor = Color.teal;
                        }

                        if (gridMapBehaviour.targetCells.Contains(location))
                        {
                            GUI.backgroundColor = Color.magenta;
                        }

                        if (GUILayout.Button($"{gridCell.distance}", cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                        {
                        }

                        GUI.backgroundColor = originalColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}