using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR;

namespace Vrpn.Input
{
    // InputControlLayoutAttribute attribute is only necessary if you want
    // to override default behavior that occurs when registering your device
    // as a layout.
    // The most common use of InputControlLayoutAttribute is to direct the system
    // to a custom "state struct" through the `stateType` property. See below for details.
    [Preserve, InputControlLayout(displayName = "VRPN XR HMD", stateType = typeof(VrpnXRHMDState))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VrpnXRHMD : XRHMD, IInputUpdateCallbackReceiver
    {
        #region Fields

        [SerializeField]
        protected VrpnMetaXRHMD vrpnInput;

        protected VrpnTracker devicePose;

        #endregion

        #region Properties

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnTracker DevicePoseTracker
        {
            get
            {
                if (devicePose == null && !string.IsNullOrEmpty(vrpnInput.DevicePoseTrackerName))
                    devicePose = InputSystem.GetDevice(vrpnInput.DevicePoseTrackerName) as VrpnTracker;
                return devicePose;
            }
        }

        #endregion

        #region Constructors

        static VrpnXRHMD()
        {
            // RegisterLayout() adds a "control layout" to the system.
            // These can be layouts for individual controls (like sticks)
            // or layouts for entire devices (which are themselves
            // control) like in our case.
            // In this case, the layout is described through the InputControlLayout attribute of the class
            InputSystem.RegisterLayout<VrpnXRHMD>();
        }

        #endregion

        #region Methods

        public void OnUpdate()
        {
            VrpnXRHMDState state = new()
            {
                isTracked = true, // seems like this is important !
                trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation),
                position = DevicePoseTracker != null ? DevicePoseTracker.devicePosition.ReadUnprocessedValue() : Vector3.zero,
                rotation = DevicePoseTracker != null ? DevicePoseTracker.deviceRotation.ReadUnprocessedValue() : Quaternion.identity
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
            foreach (VrpnMetaXRHMD inputSettings in VrpnInputSettings.Settings.XRHMDs)
            {
                VrpnXRHMD vrpnDevice = InputSystem.GetDevice(inputSettings.InputName) as VrpnXRHMD;
                vrpnDevice ??= InputSystem.AddDevice<VrpnXRHMD>(inputSettings.InputName);
                if (vrpnDevice != null)
                {
                    vrpnDevice.vrpnInput = inputSettings;
                    Debug.Log("Added XR HMD device " + vrpnDevice.displayName);
                }
            }
        }

        #endregion
    }
}
