namespace UwU.EasyData
{
    using System.IO;
    using UnityEditor.AssetImporters;
    using UnityEngine;

    [ScriptedImporter(1, Config.TABLE_SET_DATA_EXT)]
    public class TableSetAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var tableSetIO = new TableSetIO();
            tableSetIO.ReadFromBytes(bytes);
            var dataPayload = ScriptableObject.CreateInstance<TableSetAsset>();
            dataPayload.tableSet = tableSetIO.TableSet;
            dataPayload.bytes = bytes;
            ctx.AddObjectToAsset("main", dataPayload);
            ctx.SetMainObject(dataPayload);
        }
    }
}