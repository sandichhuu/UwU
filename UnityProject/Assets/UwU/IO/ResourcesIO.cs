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
                var request = Resources.LoadAsync<TextAsset>(filePath);
                yield return request;
                if (request.asset == null)
                {
                    Debug.LogError($"[ResourcesIO] Not found: {filePath}");
                    yield return null;
                    yield break;
                }

                var asset = request.asset as TextAsset;
                yield return asset.bytes;
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