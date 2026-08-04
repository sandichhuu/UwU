using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace UwU.EasyData
{
    public class TableSetEditorWindow : EditorWindow
    {
        private string filePath = "";
        private TableSetIO tableSetIO;
        private bool isLoaded;
        private int selectedTableIndex = -1;
        private Vector2 scrollPos;
        private Vector2 tableListScroll;

        private int editingRow = -1;
        private int editingCol = -1;
        private string editBuffer = "";

        [MenuItem("Tools/TableSet Editor (.tbs)")]
        public static void ShowWindow()
        {
            var window = GetWindow<TableSetEditorWindow>("TableSet Editor");
            window.minSize = new Vector2(800, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("File (.tbs):", GUILayout.Width(65));
            this.filePath = EditorGUILayout.TextField(this.filePath, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Browse", EditorStyles.toolbarButton, GUILayout.Width(60)))
                BrowseFile();

            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50)))
                LoadTableSet();

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
                SaveTableSet();

            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                CreateNewTableSet();

            EditorGUILayout.EndHorizontal();

            if (!this.isLoaded || this.tableSetIO == null)
            {
                EditorGUILayout.HelpBox("No TableSet loaded. Browse, Load or Create New.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            DrawTableSidebar();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (this.selectedTableIndex >= 0 && this.selectedTableIndex < this.tableSetIO.TableIOs.Count)
            {
                DrawSelectedTableEditor();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a table from the left list to edit.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTableSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.LabelField("Tables in Set", EditorStyles.boldLabel);

            if (GUILayout.Button("+ Add Table"))
            {
                var newTableName = $"Table_{this.tableSetIO.TableSet.tables.Count + 1}";
                this.tableSetIO.AddTable(newTableName);
                if (this.selectedTableIndex == -1) this.selectedTableIndex = 0;
                Repaint();
            }

            EditorGUILayout.Space(2);
            this.tableListScroll = EditorGUILayout.BeginScrollView(this.tableListScroll, GUILayout.ExpandHeight(true));

            var tables = this.tableSetIO.TableSet.tables;
            for (var i = 0; i < tables.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var isSelected = (this.selectedTableIndex == i);

                if (GUILayout.Toggle(isSelected, tables[i].tableName, "Button", GUILayout.Height(25)))
                {
                    if (this.selectedTableIndex != i)
                    {
                        this.selectedTableIndex = i;
                        this.editingRow = -1;
                        this.editingCol = -1;
                    }
                }

                if (GUILayout.Button("✕", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Confirm", $"Delete table '{tables[i].tableName}'?", "Yes", "No"))
                    {
                        this.tableSetIO.RemoveTable(i);
                        if (this.selectedTableIndex >= this.tableSetIO.TableIOs.Count)
                            this.selectedTableIndex = this.tableSetIO.TableIOs.Count - 1;
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedTableEditor()
        {
            var tableIO = this.tableSetIO.TableIOs[this.selectedTableIndex];
            var table = tableIO.Table;

            EditorGUILayout.BeginHorizontal();
            table.tableName = EditorGUILayout.TextField("Table Name:", table.tableName, GUILayout.Width(300));
            EditorGUILayout.LabelField($"Rows: {table.rowCount} | Cols: {table.columns.Count}", GUILayout.ExpandWidth(true));

            if (GUILayout.Button("+ Column", GUILayout.Width(80)))
            {
                AddColumnPopup.ShowPopup((header, type) =>
                {
                    tableIO.AddColumn(header, type);
                    Repaint();
                });
            }

            if (GUILayout.Button("+ Row", GUILayout.Width(60)))
            {
                tableIO.Append();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            DrawTableGrid(tableIO);
            EditorGUILayout.EndScrollView();
        }

        private void DrawTableGrid(TableIO tableIO)
        {
            var columns = tableIO.Table.columns;
            var rowCount = tableIO.Table.rowCount;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("TYPE", EditorStyles.boldLabel, GUILayout.Width(40));
            for (var c = 0; c < columns.Count; c++)
            {
                var col = columns[c];
                var width = Extensions.IsDynamicType(col.columnType) ? 200f : 100f;
                EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
                var headerText = $"[{col.columnType}]";
                EditorGUILayout.LabelField(headerText, EditorStyles.boldLabel, GUILayout.MinWidth(Extensions.IsDynamicType(col.columnType) ? 30f : 50f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("#", EditorStyles.boldLabel, GUILayout.Width(40));

            for (var c = 0; c < columns.Count; c++)
            {
                var col = columns[c];
                var width = Extensions.IsDynamicType(col.columnType) ? 200f : 100f;

                EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
                col.header = EditorGUILayout.TextField(col.header, GUILayout.Width(width - 20));

                if (GUILayout.Button("✕", GUILayout.Width(20), GUILayout.Height(18)))
                {
                    if (EditorUtility.DisplayDialog("Confirm", $"Delete column '{col.header}'?", "Yes", "No"))
                    {
                        tableIO.RemoveColumn(c);
                        this.editingRow = -1;
                        this.editingCol = -1;
                        Repaint();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                        return;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            for (var r = 0; r < rowCount; r++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(r.ToString(), GUILayout.Width(40));

                for (var c = 0; c < columns.Count; c++)
                {
                    var width = Extensions.IsDynamicType(columns[c].columnType) ? 200f : 100f;

                    if (this.editingRow == r && this.editingCol == c && this.selectedTableIndex == GetActiveEditingTableIndex())
                    {
                        GUI.SetNextControlName($"tbs_cell_{r}_{c}");
                        this.editBuffer = EditorGUILayout.TextField(this.editBuffer, EditorStyles.toolbarTextField, GUILayout.Width(width));

                        var e = Event.current;
                        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
                        {
                            ApplyEdit(tableIO, r, c);
                            e.Use();
                        }

                        if (e.type == EventType.MouseDown)
                        {
                            ApplyEdit(tableIO, r, c);
                        }
                    }
                    else
                    {
                        var displayValue = GetCellDisplayValue(tableIO, c, r);
                        var cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(width));

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

                if (GUILayout.Button("✕", GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("Confirm", $"Delete row {r}?", "Yes", "No"))
                    {
                        tableIO.RemoveRow(r);
                        this.editingRow = -1;
                        this.editingCol = -1;
                        Repaint();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private int GetActiveEditingTableIndex() => this.selectedTableIndex;

        private void StartEditing(int row, int col, string currentValue)
        {
            this.editingRow = row;
            this.editingCol = col;
            this.editBuffer = currentValue;
            Repaint();

            EditorApplication.delayCall += () =>
            {
                EditorGUI.FocusTextInControl($"tbs_cell_{row}_{col}");
            };
        }

        private void ApplyEdit(TableIO tableIO, int row, int col)
        {
            try
            {
                var column = tableIO.Table.columns[col];

                switch (column.columnType)
                {
                    case ColumnType.SByte:
                        var sbyteVal = sbyte.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, new byte[] { (byte)sbyteVal });
                        break;

                    case ColumnType.Byte:
                        var byteVal = byte.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, new byte[] { byteVal });
                        break;

                    case ColumnType.Short:
                        var shortVal = short.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(shortVal));
                        break;

                    case ColumnType.UShort:
                        var ushortVal = ushort.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(ushortVal));
                        break;

                    case ColumnType.Int:
                        var intVal = int.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(intVal));
                        break;

                    case ColumnType.UInt:
                        var uintVal = uint.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(uintVal));
                        break;

                    case ColumnType.Long:
                        var longVal = long.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(longVal));
                        break;

                    case ColumnType.ULong:
                        var ulongVal = ulong.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(ulongVal));
                        break;

                    case ColumnType.Float:
                        var floatVal = float.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(floatVal));
                        break;

                    case ColumnType.Double:
                        var doubleVal = double.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(doubleVal));
                        break;

                    case ColumnType.Decimal:
                        var decimalVal = decimal.Parse(this.editBuffer);
                        var bits = decimal.GetBits(decimalVal);
                        var decBytes = new byte[16];
                        Buffer.BlockCopy(bits, 0, decBytes, 0, 16);
                        tableIO.SetCellData(col, row, decBytes);
                        break;

                    case ColumnType.Bool:
                        var boolVal = bool.Parse(this.editBuffer);
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(boolVal));
                        break;

                    case ColumnType.Char:
                        var charVal = string.IsNullOrEmpty(this.editBuffer) ? '\0' : this.editBuffer[0];
                        tableIO.SetCellData(col, row, BitConverter.GetBytes(charVal));
                        break;

                    case ColumnType.String:
                        if (string.IsNullOrEmpty(this.editBuffer))
                            tableIO.SetCellData(col, row, " ");
                        else
                            tableIO.SetCellData(col, row, this.editBuffer);
                        break;

                    case ColumnType.IntArray:
                        {
                            var values = ParseArrayInput<int>(this.editBuffer, int.Parse);
                            var bytes = MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
                            tableIO.SetCellData(col, row, bytes);
                            break;
                        }
                    case ColumnType.LongArray:
                        {
                            var values = ParseArrayInput<long>(this.editBuffer, long.Parse);
                            var bytes = MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
                            tableIO.SetCellData(col, row, bytes);
                            break;
                        }
                    case ColumnType.FloatArray:
                        {
                            var values = ParseArrayInput<float>(this.editBuffer, s => float.Parse(s, CultureInfo.InvariantCulture));
                            var bytes = MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
                            tableIO.SetCellData(col, row, bytes);
                            break;
                        }
                    case ColumnType.DoubleArray:
                        {
                            var values = ParseArrayInput<double>(this.editBuffer, s => double.Parse(s, CultureInfo.InvariantCulture));
                            var bytes = MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
                            tableIO.SetCellData(col, row, bytes);
                            break;
                        }
                    case ColumnType.BoolArray:
                        {
                            var values = ParseArrayInput<bool>(this.editBuffer,
                                s => s.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) || s.Trim() == "1");
                            var bytes = new byte[values.Length];
                            for (int i = 0; i < values.Length; i++) bytes[i] = (byte)(values[i] ? 1 : 0);
                            tableIO.SetCellData(col, row, bytes);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TableSet] Edit failed at [{row},{col}]: {ex.Message}");
                EditorUtility.DisplayDialog("Parse Exception", $"Invalid input '{this.editBuffer}' for type {tableIO.Table.columns[col].columnType}", "OK");
            }
            finally
            {
                this.editingRow = -1;
                this.editingCol = -1;
                Repaint();
            }
        }

        private string GetCellDisplayValue(TableIO tableIO, int colIndex, int rowIndex)
        {
            try
            {
                var column = tableIO.Table.columns[colIndex];

                switch (column.columnType)
                {
                    case ColumnType.SByte:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return span.Length > 0 ? ((sbyte)span[0]).ToString() : "0";
                        }
                    case ColumnType.Byte:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return span.Length > 0 ? span[0].ToString() : "0";
                        }
                    case ColumnType.Short:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToInt16(span).ToString();
                        }
                    case ColumnType.UShort:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToUInt16(span).ToString();
                        }
                    case ColumnType.Int:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToInt32(span).ToString();
                        }
                    case ColumnType.UInt:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToUInt32(span).ToString();
                        }
                    case ColumnType.Long:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToInt64(span).ToString();
                        }
                    case ColumnType.ULong:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToUInt64(span).ToString();
                        }
                    case ColumnType.Float:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToSingle(span).ToString("F4");
                        }
                    case ColumnType.Double:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToDouble(span).ToString("F4");
                        }
                    case ColumnType.Decimal:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            if (span.Length >= 16)
                            {
                                var bits = new int[4];
                                Buffer.BlockCopy(span.ToArray(), 0, bits, 0, 16);
                                return new decimal(bits).ToString();
                            }
                            return "0";
                        }
                    case ColumnType.Bool:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToBoolean(span).ToString();
                        }
                    case ColumnType.Char:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            return BitConverter.ToChar(span).ToString();
                        }
                    case ColumnType.String:
                        return tableIO.GetString(colIndex, rowIndex);

                    case ColumnType.IntArray:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            if (span.Length == 0) return "[]";
                            var count = span.Length / sizeof(int);
                            var arr = new int[count];
                            Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                            return "[" + string.Join(", ", arr.ToArray()) + "]";
                        }
                    case ColumnType.LongArray:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            if (span.Length == 0) return "[]";
                            var count = span.Length / sizeof(int);
                            var arr = new long[count];
                            Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                            return "[" + string.Join(", ", arr.ToArray()) + "]";
                        }
                    case ColumnType.FloatArray:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            if (span.Length == 0) return "[]";
                            var count = span.Length / sizeof(int);
                            var arr = new float[count];
                            Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                            return "[" + string.Join(", ", arr.ToArray()) + "]";
                        }
                    case ColumnType.DoubleArray:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            if (span.Length == 0) return "[]";
                            var count = span.Length / sizeof(int);
                            var arr = new double[count];
                            Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                            return "[" + string.Join(", ", arr.ToArray()) + "]";
                        }
                    case ColumnType.BoolArray:
                        {
                            var span = tableIO.GetCellData(colIndex, rowIndex);
                            if (span.Length == 0) return "[]";
                            var count = span.Length / sizeof(int);
                            var arr = new bool[count];
                            for (int i = 0; i < span.Length; i++)
                                arr[i] = span[i] != 0;
                            return "[" + string.Join(", ", arr.ToArray()) + "]";
                        }
                    default:
                        return "<unknown>";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError(ex.StackTrace);
                return "<error>";
            }
        }

        private void BrowseFile()
        {
            var path = EditorUtility.OpenFilePanel("Open TableSet", Application.persistentDataPath, Config.TABLE_SET_DATA_EXT);
            if (!string.IsNullOrEmpty(path))
                this.filePath = path;
        }

        private void LoadTableSet()
        {
            if (string.IsNullOrEmpty(this.filePath) || !File.Exists(this.filePath))
            {
                EditorUtility.DisplayDialog("Error", "File not found!", "OK");
                return;
            }

            try
            {
                var bytes = File.ReadAllBytes(this.filePath);
                this.tableSetIO = new TableSetIO();
                this.tableSetIO.ReadFromBytes(bytes);
                this.isLoaded = true;
                this.selectedTableIndex = this.tableSetIO.TableSet.tables.Count > 0 ? 0 : -1;
                this.editingRow = -1;
                this.editingCol = -1;
                Repaint();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Load Error", ex.Message, "OK");
            }
        }

        private void SaveTableSet()
        {
            if (!this.isLoaded) return;

            if (string.IsNullOrEmpty(this.filePath))
            {
                this.filePath = EditorUtility.SaveFilePanel("Save TableSet", Application.streamingAssetsPath, "new_tableset", Config.TABLE_SET_DATA_EXT);
                if (string.IsNullOrEmpty(this.filePath)) return;
            }

            try
            {
                var bytes = this.tableSetIO.WriteToBytes();
                File.WriteAllBytes(this.filePath, bytes);
                AssetDatabase.Refresh();
                Debug.Log($"[TableSet] Saved: {this.filePath} ({bytes.Length} bytes)");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Save Error", ex.Message, "OK");
            }
        }

        private void CreateNewTableSet()
        {
            this.tableSetIO = new TableSetIO();
            this.tableSetIO.AddTable("MasterTable");
            this.isLoaded = true;
            this.filePath = "";
            this.selectedTableIndex = 0;
            this.editingRow = -1;
            this.editingCol = -1;
            Repaint();
        }

        private static T[] ParseArrayInput<T>(string input, Func<string, T> parser)
        {
            if (string.IsNullOrWhiteSpace(input)) return Array.Empty<T>();

            var trimmed = input.Trim();
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                trimmed = trimmed[1..^1];

            if (string.IsNullOrWhiteSpace(trimmed)) return Array.Empty<T>();

            var parts = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new T[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                result[i] = parser(parts[i]);
            return result;
        }
    }
}