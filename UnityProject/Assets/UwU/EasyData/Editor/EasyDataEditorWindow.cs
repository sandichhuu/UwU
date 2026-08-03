using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UwU.Data;

namespace UwU.EasyData
{
    public class EasyDataEditorWindow : EditorWindow
    {
        private string filePath = "";
        private TableIO tableIO;
        private bool isLoaded;
        private Vector2 scrollPos;

        // Edit state
        private int editingRow = -1;
        private int editingCol = -1;
        private string editBuffer = "";

        [MenuItem("Tools/EasyData Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<EasyDataEditorWindow>("EasyData Editor");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5);

            // ── Toolbar ──────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("File:", GUILayout.Width(35));
            this.filePath = EditorGUILayout.TextField(this.filePath, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Browse", EditorStyles.toolbarButton, GUILayout.Width(60)))
                BrowseFile();

            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50)))
                LoadTable();

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
                SaveTable();

            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                CreateNewTable();

            EditorGUILayout.EndHorizontal();

            if (!this.isLoaded || this.tableIO == null)
            {
                EditorGUILayout.HelpBox("No Table loaded, Browse → Load or New.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);

            // ── Table Info ───────────────────────────────────────
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Table: {this.tableIO.Table.tableName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Rows: {this.tableIO.Table.rowCount}  |  Columns: {this.tableIO.Table.columns.Count}",
                GUILayout.Width(250));

            if (GUILayout.Button("+ Add Column", GUILayout.Width(100)))
            {
                AddColumnPopup.ShowPopup((header, type) =>
                {
                    AddNewColumn(header, type);
                });
            }

            if (GUILayout.Button("+ Add Row", GUILayout.Width(80)))
                AppendRow();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // ── Data Grid ────────────────────────────────────────
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            DrawTableGrid();
            EditorGUILayout.EndScrollView();
        }

        private void DrawTableGrid()
        {
            var columns = this.tableIO.Table.columns;
            int rowCount = this.tableIO.Table.rowCount;

            // Header row
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("#", EditorStyles.boldLabel, GUILayout.Width(40));
            for (int c = 0; c < columns.Count; c++)
            {
                var col = columns[c];
                string headerText = $"[{col.columnType}] {col.header}";
                EditorGUILayout.LabelField(headerText, EditorStyles.boldLabel,
                    GUILayout.MinWidth(GetColumnWidth(col)));
            }
            EditorGUILayout.LabelField("", GUILayout.Width(30)); // Delete button space
            EditorGUILayout.EndHorizontal();

            // Separator
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            // Data rows
            for (int r = 0; r < rowCount; r++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(r.ToString(), GUILayout.Width(40));

                for (int c = 0; c < columns.Count; c++)
                {
                    float width = GetColumnWidth(columns[c]);

                    if (this.editingRow == r && this.editingCol == c)
                    {
                        // Editing mode
                        GUI.SetNextControlName($"cell_{r}_{c}");
                        this.editBuffer = EditorGUILayout.TextField(this.editBuffer, GUILayout.MinWidth(width));

                        // Confirm edit on Enter or focus loss
                        Event e = Event.current;
                        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
                        {
                            ApplyEdit(r, c);
                            e.Use();
                        }

                        if (e.type == EventType.MouseDown)
                        {
                            ApplyEdit(r, c);
                        }
                    }
                    else
                    {
                        // Display mode — click to edit
                        string displayValue = GetCellDisplayValue(c, r);
                        var cellRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(width));

                        // Alternate row background
                        if (r % 2 == 1)
                            EditorGUI.DrawRect(cellRect, new Color(0, 0, 0, 0.05f));

                        EditorGUI.LabelField(cellRect, displayValue);

                        if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                        {
                            StartEditing(r, c, displayValue);
                            Event.current.Use();
                        }
                    }
                }

                // Delete row button
                if (GUILayout.Button("✕", GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("Confirm", $"Delete row {r}?", "Yes", "No"))
                        DeleteRow(r);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        // ── Edit Operations ─────────────────────────────────────────

        private void StartEditing(int row, int col, string currentValue)
        {
            this.editingRow = row;
            this.editingCol = col;
            this.editBuffer = currentValue;
            Repaint();

            // Defer focus to next frame
            EditorApplication.delayCall += () =>
            {
                EditorGUI.FocusTextInControl($"cell_{row}_{col}");
            };
        }

        private void ApplyEdit(int row, int col)
        {
            try
            {
                var column = this.tableIO.Table.columns[col];

                switch (column.columnType)
                {
                    case ColumnType.SByte:
                        sbyte sbyteVal = sbyte.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, new byte[] { (byte)sbyteVal });
                        break;

                    case ColumnType.Byte:
                        byte byteVal = byte.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, new byte[] { byteVal });
                        break;

                    case ColumnType.Short:
                        short shortVal = short.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(shortVal));
                        break;

                    case ColumnType.UShort:
                        ushort ushortVal = ushort.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(ushortVal));
                        break;

                    case ColumnType.Int:
                        int intVal = int.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(intVal));
                        break;

                    case ColumnType.UInt:
                        uint uintVal = uint.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(uintVal));
                        break;

                    case ColumnType.Long:
                        long longVal = long.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(longVal));
                        break;

                    case ColumnType.ULong:
                        ulong ulongVal = ulong.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(ulongVal));
                        break;

                    case ColumnType.Float:
                        float floatVal = float.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(floatVal));
                        break;

                    case ColumnType.Double:
                        double doubleVal = double.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(doubleVal));
                        break;

                    case ColumnType.Decimal:
                        decimal decimalVal = decimal.Parse(this.editBuffer);
                        int[] bits = decimal.GetBits(decimalVal);
                        byte[] decBytes = new byte[16];
                        Buffer.BlockCopy(bits, 0, decBytes, 0, 16);
                        this.tableIO.SetCellData(col, row, decBytes);
                        break;

                    case ColumnType.Bool:
                        bool boolVal = bool.Parse(this.editBuffer);
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(boolVal));
                        break;

                    case ColumnType.Char:
                        char charVal = string.IsNullOrEmpty(this.editBuffer) ? '\0' : this.editBuffer[0];
                        this.tableIO.SetCellData(col, row, BitConverter.GetBytes(charVal));
                        break;

                    case ColumnType.String:
                        this.tableIO.SetCellData(col, row, this.editBuffer);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EasyData] Edit failed at [{row},{col}]: {ex.Message}");
                EditorUtility.DisplayDialog("Parse Exception", $"Invalid input '{this.editBuffer}' for type {this.tableIO.Table.columns[col].columnType}", "OK");
            }
            finally
            {
                this.editingRow = -1;
                this.editingCol = -1;
                Repaint();
            }
        }

        private string GetCellDisplayValue(int colIndex, int rowIndex)
        {
            try
            {
                var column = this.tableIO.Table.columns[colIndex];

                switch (column.columnType)
                {
                    case ColumnType.SByte:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return span.Length > 0 ? ((sbyte)span[0]).ToString() : "0";
                        }
                    case ColumnType.Byte:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return span.Length > 0 ? span[0].ToString() : "0";
                        }
                    case ColumnType.Short:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToInt16(span).ToString();
                        }
                    case ColumnType.UShort:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToUInt16(span).ToString();
                        }
                    case ColumnType.Int:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToInt32(span).ToString();
                        }
                    case ColumnType.UInt:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToUInt32(span).ToString();
                        }
                    case ColumnType.Long:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToInt64(span).ToString();
                        }
                    case ColumnType.ULong:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToUInt64(span).ToString();
                        }
                    case ColumnType.Float:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToSingle(span).ToString("F4");
                        }
                    case ColumnType.Double:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToDouble(span).ToString("F4");
                        }
                    case ColumnType.Decimal:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            if (span.Length >= 16)
                            {
                                int[] bits = new int[4];
                                Buffer.BlockCopy(span.ToArray(), 0, bits, 0, 16);
                                return new decimal(bits).ToString();
                            }
                            return "0";
                        }
                    case ColumnType.Bool:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToBoolean(span).ToString();
                        }
                    case ColumnType.Char:
                        {
                            var span = this.tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToChar(span).ToString();
                        }
                    case ColumnType.String:
                        return this.tableIO.GetString(colIndex, rowIndex);

                    default:
                        return "<unknown>";
                }
            }
            catch
            {
                return "<error>";
            }
        }

        // ── File Operations ─────────────────────────────────────────

        private void BrowseFile()
        {
            string path = EditorUtility.OpenFilePanel("Open EasyData Table", Application.streamingAssetsPath, Config.TABLE_DATA_EXT);
            if (!string.IsNullOrEmpty(path))
                this.filePath = path;
        }

        private void LoadTable()
        {
            if (string.IsNullOrEmpty(this.filePath) || !File.Exists(this.filePath))
            {
                EditorUtility.DisplayDialog("Error", "File not found!", "OK");
                return;
            }

            try
            {
                var bytes = CompressionHelper.Decompress(File.ReadAllBytes(this.filePath));
                this.tableIO = new TableIO();
                this.tableIO.ReadFromBytes(bytes);
                this.isLoaded = true;
                this.editingRow = -1;
                this.editingCol = -1;
                Repaint();
                Debug.Log($"[EasyData] Loaded: {this.filePath} ({this.tableIO.Table.rowCount} rows)");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Load Error", ex.Message, "OK");
                Debug.LogError($"[EasyData] Load failed: {ex}");
            }
        }

        private void SaveTable()
        {
            if (!this.isLoaded) return;

            if (string.IsNullOrEmpty(this.filePath))
            {
                this.filePath = EditorUtility.SaveFilePanel("Save EasyData Table",
                    Application.streamingAssetsPath, "new_table", "bytes");
                if (string.IsNullOrEmpty(this.filePath)) return;
            }

            try
            {
                var bytes = CompressionHelper.Compress(this.tableIO.WriteToBytes());
                File.WriteAllBytes(this.filePath, bytes);
                AssetDatabase.Refresh();
                Debug.Log($"[EasyData] Saved: {this.filePath} ({bytes.Length} bytes)");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Save Error", ex.Message, "OK");
            }
        }

        private void CreateNewTable()
        {
            this.tableIO = new TableIO();
            this.tableIO.Table.tableName = "NewTable";
            this.tableIO.AddColumn("Id", ColumnType.Int);
            this.tableIO.AddColumn("Name", ColumnType.String);
            this.isLoaded = true;
            this.filePath = "";
            this.editingRow = -1;
            this.editingCol = -1;
            Repaint();
        }

        private void AddNewColumn(string header, ColumnType columnType)
        {
            Debug.Log($"AddNewColumn: {header} | {columnType}");
            this.tableIO.AddColumn(header, columnType);
        }

        private void AppendRow()
        {
            this.tableIO.Append();
            Repaint();
        }

        private void DeleteRow(int rowIndex)
        {
            this.tableIO.RemoveRow(rowIndex);
            this.editingRow = -1;
            this.editingCol = -1;
            Repaint();
        }

        private static float GetColumnWidth(Column col)
        {
            return col.columnType == ColumnType.String ? 200f : 100f;
        }
    }
}