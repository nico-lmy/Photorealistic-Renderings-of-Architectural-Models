using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;

namespace Vrpn.Input
{
    // InputControlLayoutAttribute attribute is only necessary if you want
    // to override default behavior that occurs when registering your device
    // as a layout.
    // The most common use of InputControlLayoutAttribute is to direct the system
    // to a custom "state struct" through the `stateType` property. See below for details.
    [InputControlLayout(displayName = "VRPN Tracker", commonUsages = new[] { "LeftHand", "RightHand" }, stateType = typeof(VrpnTrackerState))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VrpnTracker : TrackedDevice, IInputUpdateCallbackReceiver
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

        #region Methods

        static VrpnTracker()
        {
            // RegisterLayout() adds a "control layout" to the system.
            // These can be layouts for individual controls (like sticks)
            // or layouts for entire devices (which are themselves
            // control) like in our case.
            // In this case, the layout is described through the InputControlLayout attribute of the class
            InputSystem.RegisterLayout<VrpnTracker>();
        }

        public void OnUpdate()
        {
            (Vector3, Quaternion) pos = VrpnManager.TrackerPosQuat(DeviceAdress, vrpnInput.Index);
            VrpnTrackerState state = new()
            {
                isTracked = true,
                position = pos.Item1,
                rotation = pos.Item2,
                trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation)
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
            // InputSystem.RemoveDevice
            foreach (VrpnInput inputSettings in VrpnInputSettings.Settings.Inputs.Where(input => input.InputType == ClusterInputType.Tracker))
            {
                VrpnTracker vrpnTracker = InputSystem.GetDevice(inputSettings.InputName) as VrpnTracker;
                vrpnTracker ??= InputSystem.AddDevice<VrpnTracker>(inputSettings.InputName);
                if (vrpnTracker != null)
                {
                    vrpnTracker.vrpnInput = inputSettings;
                    Debug.Log("Added Tracker device " + vrpnTracker.displayName);
                }
            }

            #endregion
        }
    }
}