namespace UwU
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class AutoSetup : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var resourcesPath = Path.Combine(Application.dataPath, "Resources", "UwU");
            if (Directory.Exists(resourcesPath) == false)
                Directory.CreateDirectory(resourcesPath);

            var fileKeyPath = Path.Combine(resourcesPath, "secret.uwu");
            if (File.Exists(fileKeyPath) == false)
                File.WriteAllBytes(fileKeyPath, GenerateRandomByteArray());
        }

        private static byte[] GenerateRandomByteArray()
        {
            var result = new byte[4];
            System.Random rng = new(System.DateTime.UtcNow.Millisecond);
            rng.NextBytes(result);
            return result;
        }
    }
}