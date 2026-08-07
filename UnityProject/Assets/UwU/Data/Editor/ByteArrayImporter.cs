namespace UwU.Data
{
    using System.IO;
    using UnityEditor.AssetImporters;
    using UnityEngine;

    [ScriptedImporter(1, "uwu")]
    public class ByteArrayImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var dataPayload = ScriptableObject.CreateInstance<ByteArray>();
            dataPayload.bytes = bytes;
            ctx.AddObjectToAsset("main", dataPayload);
            ctx.SetMainObject(dataPayload);
        }
    }
}