using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

namespace Vrpn.Input
{
    [InputControlLayout(displayName = "VRPN Button", stateType = typeof(VrpnButtonState))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VrpnButton : InputDevice, IInputUpdateCallbackReceiver
    {
        #region Fields

        [SerializeField]
        protected VrpnInput vrpnInput;

        protected string deviceAdress;

        #endregion

        #region Properties

        /// <summary>
        /// Cache the device adress
        /// </summary>
        public string DeviceAdress
        {
            get
            {
                if (string.IsNullOrEmpty(deviceAdress))
                    deviceAdress = vrpnInput.VrpnDeviceName + "@" + vrpnInput.VrpnServerUrl;
                return deviceAdress;
            }
        }

        #endregion

        #region Constructors

        static VrpnButton()
        {
            // RegisterLayout() adds a "control layout" to the system.
            // These can be layouts for individual controls (like sticks)
            // or layouts for entire devices (which are themselves
            // control) like in our case.
            // In this case, the layout is described through the InputControlLayout attribute of the class
            InputSystem.RegisterLayout<VrpnButton>();
        }

        #endregion

        #region Methods

        public void OnUpdate()
        {
            VrpnButtonState state = new VrpnButtonState
            {
                button = VrpnManager.Button(DeviceAdress, vrpnInput.Index)
            };
            InputSystem.QueueStateEvent(this, state);
        }

        // We still need a way to also trigger execution of the static constructor
        // in the player. This can be achieved by adding the RuntimeInitializeOnLoadMethod attribute
        // to any method.
        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        private static void InitializeInPlayer()
        {
            foreach (VrpnInput inputSettings in VrpnInputSettings.Settings.Inputs.Where(input => input.InputType == ClusterInputType.Button))
            {
                VrpnButton vrpnButton = InputSystem.GetDevice(inputSettings.InputName) as VrpnButton;
                if (vrpnButton == null)
                    vrpnButton = InputSystem.AddDevice<VrpnButton>(inputSettings.InputName);
                if (vrpnButton != null)
                {
                    vrpnButton.vrpnInput = inputSettings;
                    Debug.Log("Added Button device " + vrpnButton.displayName);
                }
            }
        }

        #endregion
    }
}
