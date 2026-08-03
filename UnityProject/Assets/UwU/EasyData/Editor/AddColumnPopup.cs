using System;
using UnityEditor;
using UnityEngine;

namespace UwU.EasyData
{
    public class AddColumnPopup : EditorWindow
    {
        private string headerName = "NewColumn";
        private ColumnType selectedType = ColumnType.Int;
        private Action<string, ColumnType> onConfirm;

        public static void ShowPopup(Action<string, ColumnType> callback)
        {
            var window = ScriptableWizard.CreateInstance<AddColumnPopup>();
            window.onConfirm = callback;
            window.titleContent = new GUIContent("Add Column");
            window.minSize = new Vector2(300, 120);
            window.maxSize = new Vector2(300, 120);
            window.ShowUtility(); // Hiển thị dạng popup độc lập, giữ focus
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Create New Column", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Header:", GUILayout.Width(80));
            this.headerName = EditorGUILayout.TextField(this.headerName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type:", GUILayout.Width(80));
            this.selectedType = (ColumnType)EditorGUILayout.EnumPopup(this.selectedType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                this.Close();
            }

            if (GUILayout.Button("Add", GUILayout.Width(80)))
            {
                if (string.IsNullOrEmpty(this.headerName))
                {
                    EditorUtility.DisplayDialog("Error", "Column header cannot be empty!", "OK");
                }
                else
                {
                    this.onConfirm?.Invoke(this.headerName, this.selectedType);
                    this.Close();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}