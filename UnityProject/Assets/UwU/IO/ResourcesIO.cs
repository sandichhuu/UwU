using System.Collections;
using UnityEngine;
using UwU.Helpers;

namespace UwU.IO
{
    public class ResourcesIO
    {
        public static CoroutineHelper.CoroutineTask<byte[]> Load(string filePath)
        {
            return CoroutineHelper.Start<byte[]>(LoadRoutine());

            IEnumerator LoadRoutine()
            {
                var asset = Resources.Load<TextAsset>(filePath);
                if (asset == null)
                {
                    Debug.LogError($"[ResourcesIO] Not found: {filePath}");
                    yield return null;
                }
                else
                {
                    yield return asset.bytes;
                    Resources.UnloadAsset(asset);
                }
            }
        }

        public static void Unload(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset != null)
                Resources.UnloadAsset(asset);
        }
    }
}