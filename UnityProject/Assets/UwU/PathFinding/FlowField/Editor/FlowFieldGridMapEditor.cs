using UnityEditor;
using UnityEngine;
using UwU.Grid;

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
            if (GUILayout.Button("Open File", GUILayout.Width(100), GUILayout.Height(22)))
            {
                gridMapBehaviour.ApplyGridData(OpenOpenPanel());
            }
            if (GUILayout.Button("Save File", GUILayout.Width(100), GUILayout.Height(22)))
            {
                OpenSavePanel(gridMapBehaviour.GetGridData());
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

        public FlowFieldGridData OpenOpenPanel()
        {
            var path = EditorUtility.OpenFilePanel(
                "Open Grid Data",
                "",
                Config.FLOW_FIELD_GRID_MAP_DATA_EXT
            );

            if (string.IsNullOrEmpty(path) == false)
            {
                try
                {
                    return GridData.FromFile<FlowFieldGridData>(path);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error: {e.Message}");
                    EditorUtility.DisplayDialog("Error", $"Open file failed.\nDetail: {e.Message}", "OK");
                }
            }

            return null;
        }

        private void OpenSavePanel(GridData gridData)
        {
            var path = EditorUtility.SaveFilePanel(
                "Save Grid Data",
                "",
                "NewFile",
                Config.FLOW_FIELD_GRID_MAP_DATA_EXT
            );

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    gridData.Save(path);
                    EditorUtility.DisplayDialog("Success", $"File saved at:\n{path}", "OK");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error: {e.Message}");
                    EditorUtility.DisplayDialog("Error", $"File save failed.\nDetail: {e.Message}", "OK");
                }
            }
        }
    }
}