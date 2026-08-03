using System;
using System.Collections;
using UnityEngine;

namespace UwU.IO
{
    public class ResourcesIO
    {
        public static IEnumerator Load(string resourcePath, Action<byte[]> onComplete)
        {
            var request = Resources.LoadAsync<TextAsset>(resourcePath);
            yield return request;
            if (request.asset == null)
            {
                Debug.LogError($"[ResourcesIO] Not found: {resourcePath}");
                onComplete?.Invoke(null);
                yield break;
            }

            var asset = request.asset as TextAsset;
            onComplete?.Invoke(asset.bytes);
        }

        public static void Unload(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset != null)
                Resources.UnloadAsset(asset);
        }
    }
}