using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using UwU.Grid;

namespace UwU.PathFinding.FlowField
{
    [ScriptedImporter(1, Config.FLOW_FIELD_GRID_MAP_DATA_EXT)]
    public class FlowFieldGridDataImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var gridData = GridData.FromBytes<FlowFieldGridData>(bytes);
            var dataPayload = ScriptableObject.CreateInstance<FlowFieldGridDataAsset>();
            dataPayload.data = gridData;
            ctx.AddObjectToAsset("main", dataPayload);
            ctx.SetMainObject(dataPayload);
        }
    }
}