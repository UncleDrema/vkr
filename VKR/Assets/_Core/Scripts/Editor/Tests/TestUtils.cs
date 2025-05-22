using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor.Tests
{
    public static class TestUtils
    {
        public static T LoadAsset<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            Debug.Assert(guids.Length == 1);
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Debug.Assert(asset != null);
            return asset;
        }

        public static T[] LoadAllAssets<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            Debug.Assert(guids.Length > 0);

            var assets = new System.Collections.Generic.List<T>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }

            return assets.ToArray();
        }

        public static T GetPrivate<T>(object target, string fieldName)
        {
            Type type = target.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new ArgumentException($"Could not find '{fieldName}' field on type {type.Name}");
            return (T)field.GetValue(target);
        }
    }
}