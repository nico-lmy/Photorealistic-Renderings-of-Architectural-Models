using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Vrpn.Input.Editor
{
    [FilePath("ProjectSettings/VrpnInputSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class VrpnInputProjectSettings : ScriptableSingleton<VrpnInputProjectSettings>
    {
        #region Classes

        [Serializable]
        public class SerializableDictionary<Key, Value> : Dictionary<Key, Value>, ISerializationCallbackReceiver
        {
            #region Fields

            [HideInInspector] [SerializeField] private List<Key> _keys = new List<Key>();
            [HideInInspector] [SerializeField] private List<Value> _values = new List<Value>();

            #endregion

            #region Methods

            public void OnBeforeSerialize()
            {
                _keys.Clear();
                _values.Clear();

                foreach (KeyValuePair<Key, Value> kvp in this)
                {
                    _keys.Add(kvp.Key);
                    _values.Add(kvp.Value);
                }
            }

            public void OnAfterDeserialize()
            {
                Clear();

                for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
                {
                    Add(_keys[i], _values[i]);
                }
            }

            #endregion
        }

        [Serializable]
        protected class VrpnInputSettingsDictionary : SerializableDictionary<string, string> { }

        #endregion

        #region Fields

        [SerializeField]
        VrpnInputSettingsDictionary settingDictionary = new VrpnInputSettingsDictionary();

        #endregion

        #region Methods

        public static IEnumerable<string> GetKeys()
        {
            return instance.settingDictionary.Keys;
        }

        public static bool HasKey(string key)
        {
            return instance.settingDictionary.ContainsKey(key);
        }

        public static string GetString(string key)
        {
            return instance.settingDictionary[key];
        }

        public static void SetString(string key, string value)
        {
            instance.settingDictionary[key] = value;
            instance.Save(true);
        }

        public static new string ToString()
        {
            return JsonUtility.ToJson(instance, true);
        }

        #endregion
    }
}