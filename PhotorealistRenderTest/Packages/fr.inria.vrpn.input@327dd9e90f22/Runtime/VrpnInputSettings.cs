using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

#if UNITY_EDITOR

using UnityEditorInternal;

#endif

using UnityEngine;

namespace Vrpn.Input
{
    /// <summary>
    /// Stores VRPN Devices. Can be configured through the project settings and via a configuration file
    /// </summary>
    public class VrpnInputSettings : ScriptableObject
    {
        #region Fields

        public const string INPUT_SETTINGS_DIRECTORY = "/Settings/Resources/";
        public const string INPUT_SETTING_FILE_NAME = "VrpnInput";
        public const string INPUT_SETTINGS_PATH = INPUT_SETTINGS_DIRECTORY + INPUT_SETTING_FILE_NAME + ".asset";
        public const string VRPN_INPUT_PARAMETER = "-vrpn_input";

        private static VrpnInputSettings settings;

        // Fields that will be visible in the editor

        [SerializeField]
        private List<VrpnInput> inputs = new List<VrpnInput>();

        [SerializeField]
        private List<VrpnMetaDpad> dpads = new List<VrpnMetaDpad>();

        [SerializeField]
        private List<VrpnMetaStick> sticks = new List<VrpnMetaStick>();

        [SerializeField]
        private List<VrpnMetaXRController> xrControllers = new List<VrpnMetaXRController>();

        [SerializeField]
        private List<VrpnMetaXRHMD> xrHMDs = new List<VrpnMetaXRHMD>();

        public static VrpnInputSettings Settings
        {
            get
            {
                if (settings == null)
                    GetOrCreateSettings();
                return settings;
            }
        }

        public List<VrpnInput> Inputs { get => inputs; private set => inputs = value; }

        public List<VrpnMetaDpad> Dpads { get => dpads; private set => dpads = value; }

        public List<VrpnMetaStick> Sticks { get => sticks; private set => sticks = value; }

        public List<VrpnMetaXRController> XRControllers { get => xrControllers; private set => xrControllers = value; }

        public List<VrpnMetaXRHMD> XRHMDs { get => xrHMDs; private set => xrHMDs = value; }

        public static void Save(bool force = false)
        {
#if UNITY_EDITOR
            if (force)
                EditorUtility.SetDirty(Settings);
            AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath("Assets/" + INPUT_SETTINGS_PATH));
#endif
        }

        public static bool ExportToJson(string path)
        {
            string jsonString = JsonUtility.ToJson(Settings, true);
            File.WriteAllText(path, jsonString);
            return true;
        }

        public static bool LoadFromJson(string path)
        {
            if (File.Exists(path))
            {
                // Read the entire file and save its contents.
                string fileContent = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(fileContent, Settings);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Create a default configuration
        /// </summary>
        private static void GetOrCreateSettings()
        {
            if (!Directory.Exists(Application.dataPath + INPUT_SETTINGS_DIRECTORY))
                Directory.CreateDirectory(Application.dataPath + INPUT_SETTINGS_DIRECTORY);
            settings = Resources.Load<VrpnInputSettings>(INPUT_SETTING_FILE_NAME);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<VrpnInputSettings>();
                settings.hideFlags = HideFlags.DontUnloadUnusedAsset;
                settings.Inputs = new List<VrpnInput>();
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(Settings, "Assets/" + INPUT_SETTINGS_PATH);
#endif
            }
        }

        // Load specified or local vrpn.json file if it exists on runtime
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void LoadJsonFile()
        {
            List<string> args = Environment.GetCommandLineArgs().ToList();

            Debug.LogFormat("The application will parse the command line to load the specified vrpn input json file. If not file is spcified, the application will load the vrpn.json file in the execution folder if it exists\n" +
                    "\t Use {0} \"file_path\" to specify a vrpn input json file"
                    , VRPN_INPUT_PARAMETER);

            if (args.Any(arg => arg.ToLower().Equals(VRPN_INPUT_PARAMETER)))
            {
                int lastIndex = 0;
                while (lastIndex != -1) // -1 means none was found in the last try
                {
                    lastIndex = args.FindIndex(lastIndex + 1, arg => arg.ToLower().Equals(VRPN_INPUT_PARAMETER));
                    if (lastIndex >= 0 && lastIndex + 1 < args.Count) // +1 because we need to ensure the argument is there
                    {
                        if (LoadFromJson(args[lastIndex + 1]))
                            Debug.Log("Loaded " + args[lastIndex + 1] + " file");
                        else
                            Debug.LogError("Failed to load " + args[lastIndex + 1] + " file");
                        break;
                    }
                }
                // don't try to load the local file
                return;
            }

            if (LoadFromJson("vrpn.json"))
                Debug.Log("Loaded local vrpn.json file");

            Debug.Log("Using the following VRPN input configuration : \n" + JsonUtility.ToJson(Settings, true));
        }

        #endregion
    }
}