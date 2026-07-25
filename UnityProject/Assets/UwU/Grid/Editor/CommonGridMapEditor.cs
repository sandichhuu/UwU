using UnityEditor;
using UnityEngine;

namespace UwU.Grid
{
    [CustomEditor(typeof(CommonGridMapBehaviour))]
    public class CommonGridMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var gridMapBehaviour = this.target as CommonGridMapBehaviour;
            var gridMap = gridMapBehaviour.GetGridMap();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Obstacle Config", EditorStyles.boldLabel);

            var width = gridMap.width;
            var height = gridMap.height;

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