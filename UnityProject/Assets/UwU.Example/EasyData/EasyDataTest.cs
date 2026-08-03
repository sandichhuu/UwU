using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UwU.Data;
using UwU.EasyData;

public class EasyDataTest : MonoBehaviour
{
    [SerializeField] private string filePath;

    [ContextMenu("CreateTable")]
    private void CreateTable()
    {
        this.filePath = Path.Combine(Application.streamingAssetsPath, "test_table.bytes");

        var tableIO = new TableIO();

        tableIO.Table.tableName = "TestTable";

        tableIO.AddColumn("Id", ColumnType.Int);
        tableIO.AddColumn("Name", ColumnType.String);
        tableIO.AddColumn("StringValue", ColumnType.String);
        tableIO.AddColumn("IntValue", ColumnType.Int);

        tableIO.Append();
        tableIO.SetCellData(0, 0, BitConverter.GetBytes(1));
        tableIO.SetCellData(1, 0, "Alice");
        tableIO.SetCellData(2, 0, "Hello");
        tableIO.SetCellData(3, 0, BitConverter.GetBytes(100));

        tableIO.Append();
        tableIO.SetCellData(0, 1, BitConverter.GetBytes(2));
        tableIO.SetCellData(1, 1, "Bob");
        tableIO.SetCellData(2, 1, "World");
        tableIO.SetCellData(3, 1, BitConverter.GetBytes(200));

        var bytes = CompressionHelper.Compress(tableIO.WriteToBytes());
        File.WriteAllBytes(this.filePath, bytes);

        Debug.Log($"Table saved: {this.filePath}");
        Debug.Log($"Size: {bytes.Length} bytes");
    }

    [ContextMenu("LoadTable")]
    private void LoadTable()
    {
        this.filePath = Path.Combine(Application.streamingAssetsPath, "test_table.bytes");

        var bytes = CompressionHelper.Decompress(File.ReadAllBytes(this.filePath));
        var tableIO = new TableIO();
        tableIO.ReadFromBytes(bytes);

        var table = tableIO.Table;

        Debug.Log($"Table: {table.tableName}");
        Debug.Log($"Rows: {table.rowCount}");
        Debug.Log($"Columns: {table.columns.Count}");

        for (var row = 0; row < table.rowCount; row++)
        {
            var id = tableIO.GetCellData(0, row);
            var name = tableIO.GetString(1, row);
            var stringValue = tableIO.GetString(2, row);
            var intValue = tableIO.GetCellData(3, row);

            var idValue = BitConverter.ToInt32(id);
            var intValueValue = BitConverter.ToInt32(intValue);

            Debug.Log(
                $"Row {row}: " +
                $"Id={idValue}, " +
                $"Name={name}, " +
                $"StringValue={stringValue}, " +
                $"IntValue={intValueValue}"
            );
        }
    }
}