using System.IO;
using UnityEngine;

namespace UwU.Asset
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, new string[] { "ini" })]
    public class TextImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            var text = File.ReadAllText(ctx.assetPath);
            var textAsset = new TextAsset(text);

            ctx.AddObjectToAsset("textAsset", textAsset);
            ctx.SetMainObject(textAsset);
        }
    }
}