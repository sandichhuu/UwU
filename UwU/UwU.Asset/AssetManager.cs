using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UwU.Asset
{
    public class AssetManager
    {
        private Dictionary<string, List<Type>> assetDict;

        private List<string> paths;
        private List<UnityEngine.Object> objects;
        private List<Type> types;

        public void Setup()
        {
            paths = new List<string>();
            objects = new List<UnityEngine.Object>();
            types = new List<Type>();
            assetDict = new Dictionary<string, List<Type>>();

            var resourceMapData = Resources.Load("resources") as TextAsset;
            var lines = resourceMapData.text
                        .Replace("\r\n", "\n")
                        .Replace("\r", "\n")
                        .Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line) == false)
                {
                    var parts = line.Split('|');
                    var type = Type.GetType(parts[0]);
                    var path = parts[1];

                    if (assetDict.ContainsKey(path))
                    {
                        assetDict[path].Add(type);
                    }
                    else
                    {
                        assetDict.Add(path, new List<Type>() { type });
                    }
                }
            }
        }

        private int IndexOfCache(string address, Type type)
        {
            var index = -1;

            for (var i = 0; i < paths.Count; i++)
            {
                if (paths[i] == address &&
                    types[i] == type)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private int AddCache(string address, Type type, UnityEngine.Object obj)
        {
            paths.Add(address);
            types.Add(type);
            objects.Add(obj);

            return paths.Count - 1;
        }

        public string GetGlobalPath(string address)
        {
            return address;
        }

        public bool HasAsset(string address)
        {
            return assetDict.ContainsKey(address);
        }

        public T Load<T>(string address) where T : UnityEngine.Object
        {
            T item;
            var requestType = typeof(T);
            var cacheIndex = IndexOfCache(address, requestType);

            if (cacheIndex != -1)
            {
                item = objects[cacheIndex] as T;
            }
            else
            {
                var indexOfDot = address.LastIndexOf('.');
                item = Resources.Load<T>(address.Substring(0, indexOfDot));
                item.name = address.Replace("\\", "/");
                AddCache(address, requestType, item);
            }
            return item;
        }

        public List<T> LoadAll<T>(string directory, SearchOption searchOption) where T : UnityEngine.Object
        {
            List<T> list;

            if (searchOption == SearchOption.AllDirectories)
            {
                list = LoadAll_SearchAllSubFolder<T>(directory);
            }
            else
            {
                list = LoadAll_SearchTop<T>(directory);
            }

            return list;
        }

        private List<T> LoadAll_SearchAllSubFolder<T>(string directory) where T : UnityEngine.Object
        {
            var requestType = typeof(T);

            List<T> result;
            if (requestType == typeof(Texture2D))
            {
                result = LoadAllTexture2D_SearchSubFolder(directory) as List<T>;
            }
            else if (requestType == typeof(Texture))
            {
                result = LoadAllTexture_SearchSubFolder(directory) as List<T>;
            }
            else if (requestType == typeof(Sprite))
            {
                result = LoadAllSprite_SearchSubFolder(directory) as List<T>;
            }
            else if (requestType == typeof(AudioClip))
            {
                result = LoadAllAudioClip_SearchSubFolder(directory) as List<T>;
            }
            else if (requestType == typeof(TextAsset))
            {
                result = LoadAllTextAsset_SearchSubFolder(directory) as List<T>;
            }
            else if (requestType == typeof(Font))
            {
                result = LoadAllFont_SearchSubFolder(directory) as List<T>;
            }
            else if (requestType == typeof(TMP_FontAsset))
            {
                result = LoadAllTMPFont_SearchSubFolder(directory) as List<T>;
            }
            else
            {
                throw new System.Exception("Not support type: " + typeof(T).FullName);
            }

            return result;
        }

        private List<T> LoadAll_SearchTop<T>(string directory) where T : UnityEngine.Object
        {
            var requestType = typeof(T);

            List<T> result;
            if (requestType == typeof(Texture2D))
            {
                result = LoadAllTexture2D_SearchTop(directory) as List<T>;
            }
            else if (requestType == typeof(Texture))
            {
                result = LoadAllTexture_SearchTop(directory) as List<T>;
            }
            else if (requestType == typeof(Sprite))
            {
                result = LoadAllSprite_SearchTop(directory) as List<T>;
            }
            else if (requestType == typeof(AudioClip))
            {
                result = LoadAllAudioClip_SearchTop(directory) as List<T>;
            }
            else if (requestType == typeof(TextAsset))
            {
                result = LoadAllTextAsset_SearchTop(directory) as List<T>;
            }
            else if (requestType == typeof(Font))
            {
                result = LoadAllFont_SearchTop(directory) as List<T>;
            }
            else if (requestType == typeof(TMP_FontAsset))
            {
                result = LoadAllTMPFont_SearchTop(directory) as List<T>;
            }
            else
            {
                throw new System.Exception("Not support type: " + typeof(T).FullName);
            }

            return result;
        }

        private List<Texture2D> LoadAllTexture2D_SearchTop(string directory)
        {
            var result = new List<Texture2D>();
            var requestType = typeof(Texture2D);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".png") || address.EndsWith(".jpg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var subPath = address.Substring(directory.Length + 1);
                    if (subPath.Any(c => c == '/') == false)
                    {
                        var cacheIndex = IndexOfCache(address, requestType);
                        if (cacheIndex != -1)
                        {
                            result.Add(objects[cacheIndex] as Texture2D);
                        }
                        else
                        {
                            try
                            {
                                var indexOfDot = address.LastIndexOf('.');
                                var actualAddress = address.Substring(0, indexOfDot);
                                var item = Resources.Load<Texture2D>(actualAddress);
                                AddCache(actualAddress, requestType, item);
                                result.Add(item);
                            }
                            catch
                            {
                                Debug.LogError("Load Failed: " + address);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<Texture> LoadAllTexture_SearchTop(string directory)
        {
            var result = new List<Texture>();
            var requestType = typeof(Texture);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".png") || address.EndsWith(".jpg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var subPath = address.Substring(directory.Length + 1);
                    if (subPath.Any(c => c == '/') == false)
                    {
                        var cacheIndex = IndexOfCache(address, requestType);
                        if (cacheIndex != -1)
                        {
                            result.Add(objects[cacheIndex] as Texture);
                        }
                        else
                        {
                            try
                            {
                                var indexOfDot = address.LastIndexOf('.');
                                var actualAddress = address.Substring(0, indexOfDot);
                                var item = Resources.Load<Texture>(actualAddress);
                                AddCache(actualAddress, requestType, item);
                                result.Add(item);
                            }
                            catch
                            {
                                Debug.LogError("Load Failed: " + address);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<Sprite> LoadAllSprite_SearchTop(string directory)
        {
            var result = new List<Sprite>();
            var requestType = typeof(Sprite);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".png") || address.EndsWith(".jpg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var subPath = address.Substring(directory.Length + 1);
                    if (subPath.Any(c => c == '/') == false)
                    {
                        var cacheIndex = IndexOfCache(address, requestType);
                        if (cacheIndex != -1)
                        {
                            result.Add(objects[cacheIndex] as Sprite);
                        }
                        else
                        {
                            try
                            {
                                var indexOfDot = address.LastIndexOf('.');
                                var actualAddress = address.Substring(0, indexOfDot);
                                var item = Resources.Load<Sprite>(actualAddress);
                                AddCache(actualAddress, requestType, item);
                                result.Add(item);
                            }
                            catch
                            {
                                Debug.LogError("Load Failed: " + address);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<AudioClip> LoadAllAudioClip_SearchTop(string directory)
        {
            var result = new List<AudioClip>();
            var requestType = typeof(AudioClip);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".wav") || address.EndsWith(".mp3") || address.EndsWith(".ogg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var subPath = address.Substring(directory.Length + 1);
                    if (subPath.Any(c => c == '/') == false)
                    {
                        var cacheIndex = IndexOfCache(address, requestType);
                        if (cacheIndex != -1)
                        {
                            result.Add(objects[cacheIndex] as AudioClip);
                        }
                        else
                        {
                            try
                            {
                                var indexOfDot = address.LastIndexOf('.');
                                var actualAddress = address.Substring(0, indexOfDot);
                                var item = Resources.Load<AudioClip>(actualAddress);
                                AddCache(actualAddress, requestType, item);
                                result.Add(item);
                            }
                            catch
                            {
                                Debug.LogError("Load Failed: " + address);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<TextAsset> LoadAllTextAsset_SearchTop(string directory)
        {
            var result = new List<TextAsset>();
            var requestType = typeof(TextAsset);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".txt") || address.EndsWith(".json") || address.EndsWith(".ini");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var subPath = address.Substring(directory.Length + 1);
                    if (subPath.Any(c => c == '/') == false)
                    {
                        var cacheIndex = IndexOfCache(address, requestType);
                        if (cacheIndex != -1)
                        {
                            result.Add(objects[cacheIndex] as TextAsset);
                        }
                        else
                        {
                            try
                            {
                                var indexOfDot = address.LastIndexOf('.');
                                var actualAddress = address.Substring(0, indexOfDot);
                                var item = Resources.Load<TextAsset>(actualAddress);
                                AddCache(actualAddress, requestType, item);
                                result.Add(item);
                            }
                            catch
                            {
                                Debug.LogError("Load Failed: " + address);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private List<Font> LoadAllFont_SearchTop(string directory)
        {
            var result = new List<Font>();
            var requestType = typeof(Font);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".otf") || address.EndsWith(".ttf");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var subPath = address.Substring(directory.Length + 1);
                    if (subPath.Any(c => c == '/') == false)
                    {
                        var cacheIndex = IndexOfCache(address, requestType);
                        if (cacheIndex != -1)
                        {
                            result.Add(objects[cacheIndex] as Font);
                        }
                        else
                        {
                            try
                            {
                                var indexOfDot = address.LastIndexOf('.');
                                var actualAddress = address.Substring(0, indexOfDot);
                                var item = Resources.Load<Font>(actualAddress);
                                AddCache(actualAddress, requestType, item);
                                result.Add(item);
                            }
                            catch
                            {
                                Debug.LogError("Load Failed: " + address);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<TMP_FontAsset> LoadAllTMPFont_SearchTop(string directory)
        {
            var result = new List<TMP_FontAsset>();
            var requestType = typeof(TMP_FontAsset);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".asset");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var subPath = address.Substring(directory.Length + 1);
                    if (subPath.Any(c => c == '/') == false)
                    {
                        var cacheIndex = IndexOfCache(address, requestType);
                        if (cacheIndex != -1)
                        {
                            result.Add(objects[cacheIndex] as TMP_FontAsset);
                        }
                        else
                        {
                            try
                            {
                                var indexOfDot = address.LastIndexOf('.');
                                var actualAddress = address.Substring(0, indexOfDot);
                                var item = Resources.Load<TMP_FontAsset>(actualAddress);
                                AddCache(actualAddress, requestType, item);
                                result.Add(item);
                            }
                            catch
                            {
                                Debug.LogError("Load Failed: " + address);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<TextAsset> LoadAllTextAsset_SearchSubFolder(string directory)
        {
            var result = new List<TextAsset>();
            var requestType = typeof(TextAsset);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".txt") || address.EndsWith(".json") || address.EndsWith(".ini");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var cacheIndex = IndexOfCache(address, requestType);

                    if (cacheIndex != -1)
                    {
                        result.Add(objects[cacheIndex] as TextAsset);
                    }
                    else
                    {
                        var indexOfDot = address.LastIndexOf('.');
                        var actualAddress = address.Substring(0, indexOfDot).Replace("\\", "/");
                        var item = Resources.Load<TextAsset>(actualAddress);
                        item.name = actualAddress;
                        AddCache(actualAddress, requestType, item);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private List<AudioClip> LoadAllAudioClip_SearchSubFolder(string directory)
        {
            var result = new List<AudioClip>();
            var requestType = typeof(AudioClip);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".wav") || address.EndsWith(".mp3") || address.EndsWith(".ogg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var cacheIndex = IndexOfCache(address, requestType);

                    if (cacheIndex != -1)
                    {
                        result.Add(objects[cacheIndex] as AudioClip);
                    }
                    else
                    {
                        var indexOfDot = address.LastIndexOf('.');
                        var actualAddress = address.Substring(0, indexOfDot).Replace("\\", "/");
                        var item = Resources.Load<AudioClip>(actualAddress);
                        item.name = actualAddress;
                        AddCache(actualAddress, requestType, item);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private List<Sprite> LoadAllSprite_SearchSubFolder(string directory)
        {
            var result = new List<Sprite>();
            var requestType = typeof(Sprite);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".png") || address.EndsWith(".jpg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var cacheIndex = IndexOfCache(address, requestType);

                    if (cacheIndex != -1)
                    {
                        result.Add(objects[cacheIndex] as Sprite);
                    }
                    else
                    {
                        var indexOfDot = address.LastIndexOf('.');
                        var actualAddress = address.Substring(0, indexOfDot).Replace("\\", "/");
                        var item = Resources.Load<Sprite>(actualAddress);
                        item.name = actualAddress;
                        AddCache(actualAddress, requestType, item);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private List<Texture> LoadAllTexture_SearchSubFolder(string directory)
        {
            var result = new List<Texture>();
            var requestType = typeof(Texture);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".png") || address.EndsWith(".jpg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var cacheIndex = IndexOfCache(address, requestType);

                    if (cacheIndex != -1)
                    {
                        result.Add(objects[cacheIndex] as Texture);
                    }
                    else
                    {
                        var indexOfDot = address.LastIndexOf('.');
                        var actualAddress = address.Substring(0, indexOfDot).Replace("\\", "/");
                        var item = Resources.Load<Texture>(actualAddress);
                        item.name = actualAddress;
                        AddCache(actualAddress, requestType, item);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private List<Texture2D> LoadAllTexture2D_SearchSubFolder(string directory)
        {
            var result = new List<Texture2D>();
            var requestType = typeof(Texture2D);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".png") || address.EndsWith(".jpg");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var cacheIndex = IndexOfCache(address, requestType);

                    if (cacheIndex != -1)
                    {
                        result.Add(objects[cacheIndex] as Texture2D);
                    }
                    else
                    {
                        var indexOfDot = address.LastIndexOf('.');
                        var actualAddress = address.Substring(0, indexOfDot).Replace("\\", "/");
                        var item = Resources.Load<Texture2D>(actualAddress);
                        item.name = actualAddress;
                        AddCache(actualAddress, requestType, item);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private List<Font> LoadAllFont_SearchSubFolder(string directory)
        {
            var result = new List<Font>();
            var requestType = typeof(Font);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".ttf") || address.EndsWith(".otf");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var cacheIndex = IndexOfCache(address, requestType);

                    if (cacheIndex != -1)
                    {
                        result.Add(objects[cacheIndex] as Font);
                    }
                    else
                    {
                        var indexOfDot = address.LastIndexOf('.');
                        var actualAddress = address.Substring(0, indexOfDot).Replace("\\", "/");
                        var item = Resources.Load<Font>(actualAddress);
                        item.name = actualAddress;
                        AddCache(actualAddress, requestType, item);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private List<TMP_FontAsset> LoadAllTMPFont_SearchSubFolder(string directory)
        {
            var result = new List<TMP_FontAsset>();
            var requestType = typeof(TMP_FontAsset);

            foreach (var assetInfo in assetDict)
            {
                var address = assetInfo.Key;
                var isCorrectFileExtension = address.EndsWith(".asset");
                if (address.StartsWith(directory) && isCorrectFileExtension)
                {
                    var cacheIndex = IndexOfCache(address, requestType);

                    if (cacheIndex != -1)
                    {
                        result.Add(objects[cacheIndex] as TMP_FontAsset);
                    }
                    else
                    {
                        var indexOfDot = address.LastIndexOf('.');
                        var actualAddress = address.Substring(0, indexOfDot).Replace("\\", "/");
                        var item = Resources.Load<TMP_FontAsset>(actualAddress);
                        item.name = actualAddress;
                        AddCache(actualAddress, requestType, item);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public void Unload(string address)
        {
            for (var i = paths.Count - 1; i >= 0; i--)
            {
                var path = paths[i];
                if (address == path)
                {
                    var obj = objects[i];

                    paths.RemoveAt(i);
                    objects.RemoveAt(i);
                    types.RemoveAt(i);

                    UnityEngine.Object.Destroy(obj);
                }
            }
        }

        public bool HasDirectory(string directory)
        {
            return assetDict.Keys.Any(p => p.StartsWith(directory));
        }
    }
}