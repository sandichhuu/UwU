using System;
using UnityEditor;
using UnityEngine;

namespace UwU.PathFinding.AStar
{
    [CustomEditor(typeof(AStarGridMapBehaviour))]
    public class AStarGridMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var gridMapBehaviour = this.target as AStarGridMapBehaviour;
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

        private void RenderPreview(AStarGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
        {
            var gridMap = gridMapBehaviour.GetGridMap();
            var width = gridMap.width;
            var height = gridMap.height;

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            if (GUILayout.Button("Find Path", GUILayout.Width(100), GUILayout.Height(22)))
            {
                gridMapBehaviour.FindPath();
            }
            EditorGUILayout.EndHorizontal();

            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var path = gridMapBehaviour.GetPath();

                for (var x = 0; x < width; x++)
                {
                    var gridCell = gridMap[x, y];
                    if (gridCell != null)
                    {
                        var originalColor = GUI.backgroundColor;
                        var index = gridMap.GetIndex(x, y);

                        if (gridCell.IsObstacle)
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }

                        if (path.Contains(index))
                        {
                            GUI.backgroundColor = Color.green;
                        }

                        if (index == gridMapBehaviour.GetStartIndex())
                        {
                            GUI.backgroundColor = Color.teal;
                        }

                        if (index == gridMapBehaviour.GetTargetIndex())
                        {
                            GUI.backgroundColor = Color.magenta;
                        }

                        if (GUILayout.Button(string.Empty, cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            gridMapBehaviour.SetStartIndex(index);
                            EditorUtility.SetDirty(gridMapBehaviour);
                        }

                        GUI.backgroundColor = originalColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderStartConfigGroup(AStarGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
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

                        if (gridCell.IsObstacle)
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }

                        if (index == gridMapBehaviour.GetStartIndex())
                        {
                            GUI.backgroundColor = Color.teal;
                        }

                        if (GUILayout.Button(string.Empty, cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            gridMapBehaviour.SetStartIndex(index);
                            EditorUtility.SetDirty(gridMapBehaviour);
                        }

                        GUI.backgroundColor = originalColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderTargetConfigGroup(AStarGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
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
                        var originalColor = GUI.backgroundColor;
                        var index = gridMap.GetIndex(x, y);

                        if (gridCell.IsObstacle)
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }

                        if (index == gridMapBehaviour.GetTargetIndex())
                        {
                            GUI.backgroundColor = Color.magenta;
                        }

                        if (GUILayout.Button(string.Empty, cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                        {
                            gridMapBehaviour.SetTargetIndex(index);
                            EditorUtility.SetDirty(gridMapBehaviour);
                        }

                        GUI.backgroundColor = originalColor;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderObstaclesConfigGroup(AStarGridMapBehaviour gridMapBehaviour, GUIStyle cellStyle)
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
    }
}