using UnityEditor;
using UnityEngine;

namespace UwU.PathFinding.FlowField
{
    [CustomEditor(typeof(FlowFieldPathfinder))]
    public class FlowFieldEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var gridManager = this.target as FlowFieldPathfinder;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Grid Obstacle Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click vào ô để bật/tắt vật cản (Đỏ: Obstacle, Xanh dương: Start, Tím: Target, Xám: Trống).", MessageType.Info);

            if (GUILayout.Button("Init / Reset Default Map"))
            {
                gridManager.RecomputeFlowField();
                EditorUtility.SetDirty(gridManager);
            }

            if (gridManager.obstacles == null || gridManager.obstacles.Length != gridManager.width * gridManager.height)
            {
                gridManager.Initialize();
                EditorUtility.SetDirty(gridManager);
            }

            EditorGUILayout.Space(5);

            var width = gridManager.width;
            var height = gridManager.height;

            var cellStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };

            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (var x = 0; x < width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    var index = gridManager.GetIndex(pos);

                    var originalColor = GUI.backgroundColor;
                    var isStartOrTarget = gridManager.IsStartOrTarget(pos);

                    if (isStartOrTarget)
                    {
                        GUI.backgroundColor = (x == 0) ? Color.cyan : Color.magenta;
                    }
                    else if (gridManager.obstacles[index])
                    {
                        GUI.backgroundColor = Color.red;
                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                    }

                    var buttonText = "";
                    if (gridManager.distanceMap != null && index < gridManager.distanceMap.Length && gridManager.distanceMap[index] != -1)
                    {
                        buttonText = gridManager.distanceMap[index].ToString();
                    }

                    if (GUILayout.Button(buttonText, cellStyle, GUILayout.Width(22), GUILayout.Height(22)))
                    {
                        if (!isStartOrTarget)
                        {
                            gridManager.obstacles[index] = !gridManager.obstacles[index];
                            gridManager.RecomputeFlowField();
                            EditorUtility.SetDirty(gridManager);
                        }
                    }

                    GUI.backgroundColor = originalColor;
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}