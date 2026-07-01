using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Vrpn.Input.Editor
{
    /// <summary>
    /// This Class adds new settings in the project settings window
    /// </summary>
    class VrpnInputSettingsProvider : SettingsProvider
    {
        #region Fields

        private const string Extension = "json";
        private readonly string LAST_JSON_PATH = "LastJsonPath";

        [SerializeField]
        VisualTreeAsset m_ItemAsset;

        [SerializeField]
        VisualElement rootVisualElement;

        #endregion

        #region Properties

        protected string LastPath
        {
            get => VrpnInputProjectSettings.HasKey(LAST_JSON_PATH) ? VrpnInputProjectSettings.GetString(LAST_JSON_PATH) : Application.dataPath;
            set
            {
                string directory = Path.GetDirectoryName(value);
                VrpnInputProjectSettings.SetString(LAST_JSON_PATH, directory);
            }
        }

        #endregion

        #region Constructors

        public VrpnInputSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
        : base(path, scope) { }

        #endregion

        #region Methods

        [SettingsProvider]
        public static SettingsProvider CreateVrpnSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            VrpnInputSettingsProvider provider = new VrpnInputSettingsProvider("Project/VRPN/Input", SettingsScope.Project)
            {
                label = "VRPN Input",

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "VRPN", "Input" })
            };
            return provider;
        }

        public override void OnDeactivate()
        {
            UnregisterFromChanges();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("VrpnInputSettings");
            TemplateContainer treeClone = visualTree.CloneTree();

            rootVisualElement = rootElement;

            rootElement.Add(treeClone);

            // Binding
            SerializedObject serializedInputSettings = new SerializedObject(VrpnInputSettings.Settings);
            rootElement.Q<PropertyField>("Inputs").Bind(serializedInputSettings);
            rootElement.Q<PropertyField>("Dpads").Bind(serializedInputSettings);
            rootElement.Q<PropertyField>("Sticks").Bind(serializedInputSettings);
            rootElement.Q<PropertyField>("XRControllers").Bind(serializedInputSettings);
            rootElement.Q<PropertyField>("XRHMDs").Bind(serializedInputSettings);

            rootElement.Q<Button>("Export").clicked += () => ExportToJson();
            rootElement.Q<Button>("Load").clicked += () => LoadFromJson();

            // Binding related update is performed asynchrouously, so register after it is performed to prevent handling inputs changes due to the first update from binding
            rootElement.schedule.Execute(() => RegisterToChanges());
        }

        private void RegisterToChanges()
        {
            if (rootVisualElement == null)
                return;
            // when any field is changed, save
            rootVisualElement.RegisterCallback<ChangeEvent<string>>(e => VrpnInputSettings.Save());
            rootVisualElement.RegisterCallback<ChangeEvent<int>>(e => VrpnInputSettings.Save());
            rootVisualElement.RegisterCallback<ChangeEvent<ClusterInputType>>(e => VrpnInputSettings.Save());
        }

        private void UnregisterFromChanges()
        {
            if (rootVisualElement == null)
                return;
            rootVisualElement.UnregisterCallback<ChangeEvent<string>>(e => VrpnInputSettings.Save());
            rootVisualElement.UnregisterCallback<ChangeEvent<int>>(e => VrpnInputSettings.Save());
            rootVisualElement.UnregisterCallback<ChangeEvent<ClusterInputType>>(e => VrpnInputSettings.Save());
        }

        private void ExportToJson()
        {
            string path = EditorUtility.SaveFilePanel("Export VRPN Devices to JSON", LastPath, "vrpn.json", Extension);
            VrpnInputSettings.Save();
            if (!string.IsNullOrEmpty(path) && VrpnInputSettings.ExportToJson(path))
                LastPath = path;
        }

        private void LoadFromJson()
        {
            string path = EditorUtility.OpenFilePanel("Load VRPN Devices from JSON", LastPath, Extension);
            if (!string.IsNullOrEmpty(path) && VrpnInputSettings.LoadFromJson(path))
            {
                LastPath = path;
                VrpnInputSettings.Save(true);
            }
        }

        #endregion
    }
}