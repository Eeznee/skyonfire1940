#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NWH.NUI
{
    public static class EditorCache
    {
        // Workaround to get around the issue of .NET 2.0, asmdef and dynamic.
        private static Dictionary<string, float> heightCache = new Dictionary<string, float>();
        private static Dictionary<string, ReorderableList> reorderableListCache = new Dictionary<string, ReorderableList>();
        private static Dictionary<string, bool> guiWasEnabledCache = new Dictionary<string, bool>();
        private static Dictionary<string, NUIEditor> nuiEditorCache = new Dictionary<string, NUIEditor>();
        private static Dictionary<string, bool> isExpandedCache = new Dictionary<string, bool>();
        private static Dictionary<string, int> tabIndexCache = new Dictionary<string, int>();
        private static Dictionary<string, Texture2D> texture2DCache = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SerializedProperty> serializedPropertyCache = new Dictionary<string, SerializedProperty>();


        public static bool GetHeightCacheValue(string key, ref float value)
        {
            if (string.IsNullOrEmpty(key) || !heightCache.ContainsKey(key))
            {
                return false;
            }

            value = heightCache[key];
            return true;
        }

        public static bool GetReorderableListCacheValue(string key, ref ReorderableList value)
        {
            if (string.IsNullOrEmpty(key) || !reorderableListCache.ContainsKey(key))
            {
                return false;
            }

            value = reorderableListCache[key];
            return true;
        }

        public static bool GetGuiWasEnabledCValue(string key, ref bool value)
        {
            if (string.IsNullOrEmpty(key) || !guiWasEnabledCache.ContainsKey(key))
            {
                return false;
            }

            value = guiWasEnabledCache[key];
            return true;
        }

        public static bool GetNUIEditorCacheValue(string key, ref NUIEditor value)
        {
            if (string.IsNullOrEmpty(key) || !nuiEditorCache.ContainsKey(key))
            {
                return false;
            }

            value = nuiEditorCache[key];
            return true;
        }

        public static bool GetIsExpandedCacheValue(string key, ref bool value)
        {
            if (string.IsNullOrEmpty(key) || !isExpandedCache.ContainsKey(key))
            {
                return false;
            }

            value = isExpandedCache[key];
            return true;
        }

        public static bool GetTabIndexCacheValue(string key, ref float value)
        {
            if (string.IsNullOrEmpty(key) || !tabIndexCache.ContainsKey(key))
            {
                return false;
            }

            value = tabIndexCache[key];
            return true;
        }

        public static bool GetTexture2DCacheValue(string key, ref Texture2D value)
        {
            if (string.IsNullOrEmpty(key) || !texture2DCache.ContainsKey(key))
            {
                return false;
            }

            value = texture2DCache[key];
            return true;
        }

        public static bool GetSerializedPropertyCacheValue(string key, ref SerializedProperty value)
        {
            if (string.IsNullOrEmpty(key) || !serializedPropertyCache.ContainsKey(key))
            {
                return false;
            }

            value = serializedPropertyCache[key];
            return true;
        }





        public static bool SetHeightCacheValue(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!heightCache.ContainsKey(key))
            {
                heightCache.Add(key, value);
            }
            else
            {
                heightCache[key] = value;
            }

            return true;
        }

        public static bool SetReorderableListCacheValue(string key, ReorderableList value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!reorderableListCache.ContainsKey(key))
            {
                reorderableListCache.Add(key, value);
            }
            else
            {
                reorderableListCache[key] = value;
            }

            return true;
        }

        public static bool SetGuiWasEnabledCacheValue(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!guiWasEnabledCache.ContainsKey(key))
            {
                guiWasEnabledCache.Add(key, value);
            }
            else
            {
                guiWasEnabledCache[key] = value;
            }

            return true;
        }

        public static bool SetNUIEditorCacheValue(string key, NUIEditor value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!nuiEditorCache.ContainsKey(key))
            {
                nuiEditorCache.Add(key, value);
            }
            else
            {
                nuiEditorCache[key] = value;
            }

            return true;
        }

        public static bool SetIsExpandedCacheValue(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!isExpandedCache.ContainsKey(key))
            {
                isExpandedCache.Add(key, value);
            }
            else
            {
                isExpandedCache[key] = value;
            }

            return true;
        }

        public static bool SetTabIndexCacheValue(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!tabIndexCache.ContainsKey(key))
            {
                tabIndexCache.Add(key, value);
            }
            else
            {
                tabIndexCache[key] = value;
            }

            return true;
        }

        public static bool SetTexture2DCacheValue(string key, Texture2D value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!texture2DCache.ContainsKey(key))
            {
                texture2DCache.Add(key, value);
            }
            else
            {
                texture2DCache[key] = value;
            }

            return true;
        }

        public static bool SetSerializedPropertyCacheValue(string key, SerializedProperty value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!serializedPropertyCache.ContainsKey(key))
            {
                serializedPropertyCache.Add(key, value);
            }
            else
            {
                serializedPropertyCache[key] = value;
            }

            return true;
        }

        // // Store data for each property as property drawer gets reused multiple times and local values overwritten
        // private static readonly Dictionary<string, dynamic> Cache = new Dictionary<string, dynamic>
        // {
        //     {"height", new Dictionary<string, float>()},
        //     {"ReorderableList", new Dictionary<string, ReorderableList>()},
        //     {"guiWasEnabled", new Dictionary<string, bool>()},
        //     {"NUIEditor", new Dictionary<string, NUIEditor>()},
        //     {"isExpanded", new Dictionary<string, bool>()},
        //     {"tabIndex", new Dictionary<string, int>()},
        //     {"Texture2D", new Dictionary<string, Texture2D>()},
        //     {"SerializedProperty", new Dictionary<string, SerializedProperty>()},
        // };

        // public static bool GetCachedValue<T>(string variableName, ref T value, string key)
        // {
        //     if (string.IsNullOrEmpty(key))
        //     {
        //         return false;
        //     }
        //
        //     if (!Cache.ContainsKey(variableName) || !Cache[variableName].ContainsKey(key))
        //     {
        //         return false;
        //     }
        //
        //     value = Cache[variableName][key];
        //     return true;
        // }
        //
        //
        // public static bool SetCachedValue<T>(string variableName, T value, string key)
        // {
        //     if (string.IsNullOrEmpty(key))
        //     {
        //         return false;
        //     }
        //
        //     if (Cache.ContainsKey(variableName))
        //     {
        //         if (!Cache[variableName].ContainsKey(key))
        //         {
        //             Cache[variableName].Add(key, value);
        //         }
        //         else
        //         {
        //             Cache[variableName][key] = value;
        //         }
        //
        //         return true;
        //     }
        //
        //     return false;
        // }
    }
}

#endif
