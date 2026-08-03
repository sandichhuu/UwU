using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UwU.IO
{
    public static class StreamingAssetsIO
    {
        public static IEnumerator Load(string fileName, System.Action<byte[]> onComplete)
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);

            using var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[StreamingAssetsIO] Error '{fileName}': {request.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            onComplete?.Invoke(request.downloadHandler.data);
        }

        public static bool Exists(string fileName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.LogWarning("[StreamingAssetsIO] Exists() not working on Android");
            return true;
#else
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            return File.Exists(path);
#endif
        }
    }
}