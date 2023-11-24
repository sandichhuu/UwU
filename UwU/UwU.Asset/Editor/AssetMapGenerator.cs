using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UwU.Asset
{
    using UwU.Core;

    public class AssetMapGenerator : MonoBehaviour, IPreprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder => 1;

        [MenuItem("UwU/Asset System/Build Resources")]
        private static void Build()
        {
            Validate();
            GenegrateGUIDMap();
        }

        private static void GenegrateGUIDMap()
        {
            try
            {
                var mapFilePath = Path.Combine(Application.dataPath, "Resources", "resources.txt");
                var resourcesDirectories = ScanResourcesDirectories();

                using (var fileStream = new FileStream(mapFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        for (var i = 0; i < resourcesDirectories.Count; i++)
                        {
                            var resourcesDirectory = resourcesDirectories[i];
                            var allFiles = Directory.GetFiles(resourcesDirectory, "*", SearchOption.AllDirectories);

                            if (allFiles.Length > 0)
                            {
                                for (var j = 0; j < allFiles.Length; j++)
                                {
                                    var filePath = allFiles[j].Replace("\\", "/");

                                    if (filePath.EndsWith("resources.txt"))
                                    {
                                        continue;
                                    }

                                    var indexOfAsset = filePath.IndexOf("Assets/");
                                    var indexOfResource = filePath.IndexOf("Resources/");
                                    var internalFilePath = filePath.Substring(indexOfAsset);
                                    var internalResourcePath = filePath.Substring(10 + indexOfResource);
                                    var type = AssetDatabase.GetMainAssetTypeAtPath(internalFilePath);

                                    if (type != null)
                                    {
                                        writer.WriteLine($"{type}|{internalResourcePath}");
                                    }
                                }
                            }
                        }
                    }
                }

                UnityLogger.Print(true, "resources.txt is generated.");
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        private static void Validate()
        {
            var directory = Path.Combine(Application.dataPath, "Resources");
            var mapFilePath = Path.Combine(Application.dataPath, "Resources", "resources.txt");

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(mapFilePath))
            {
                File.Delete(mapFilePath);
            }
        }

        private static List<string> ScanResourcesDirectories()
        {
            var projectPath = Application.dataPath;
            var resourcesPaths = new List<string>();
            var paths = new List<string>();

            paths.AddRange(Directory.GetDirectories(projectPath));
            while (paths.Count > 0)
            {
                var subPath = paths[0].Replace("\\", "/");
                if (subPath.EndsWith("/Resources"))
                {
                    resourcesPaths.Add(subPath);
                }

                paths.AddRange(Directory.GetDirectories(subPath));
                paths.RemoveAt(0);
            }

            return resourcesPaths;
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            Build();
        }
    }
}