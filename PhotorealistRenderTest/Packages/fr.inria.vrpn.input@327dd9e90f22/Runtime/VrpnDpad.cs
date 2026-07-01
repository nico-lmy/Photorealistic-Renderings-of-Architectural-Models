using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;

namespace Vrpn.Input
{
    // InputControlLayoutAttribute attribute is only necessary if you want
    // to override default behavior that occurs when registering your device
    // as a layout.
    // The most common use of InputControlLayoutAttribute is to direct the system
    // to a custom "state struct" through the `stateType` property. See below for details.
    [Preserve, InputControlLayout(displayName = "VRPN Dpad", stateType = typeof(VrpnDpadState))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VrpnDpad : InputDevice, IInputUpdateCallbackReceiver
    {
        #region Fields

        [SerializeField]
        protected VrpnMetaDpad vrpnInput;

        protected VrpnButton upButton;
        protected VrpnButton downButton;
        protected VrpnButton leftButton;
        protected VrpnButton rightButton;

        #endregion

        #region Properties

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton UpButton
        {
            get
            {
                if (upButton == null && !string.IsNullOrEmpty(vrpnInput.UpButtonName))
                    upButton = InputSystem.GetDevice(vrpnInput.UpButtonName) as VrpnButton;
                return upButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton DownButton
        {
            get
            {
                if (downButton == null && !string.IsNullOrEmpty(vrpnInput.DownButtonName))
                    downButton = InputSystem.GetDevice(vrpnInput.DownButtonName) as VrpnButton;
                return downButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton LeftButton
        {
            get
            {
                if (leftButton == null && !string.IsNullOrEmpty(vrpnInput.LeftButtonName))
                    leftButton = InputSystem.GetDevice(vrpnInput.LeftButtonName) as VrpnButton;
                return leftButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton RightButton
        {
            get
            {
                if (rightButton == null && !string.IsNullOrEmpty(vrpnInput.RightButtonName))
                    rightButton = InputSystem.GetDevice(vrpnInput.RightButtonName) as VrpnButton;
                return rightButton;
            }
        }

        #endregion

        #region Constructors

        static VrpnDpad()
        {
            // RegisterLayout() adds a "control layout" to the system.
            // These can be layouts for individual controls (like sticks)
            // or layouts for entire devices (which are themselves
            // control) like in our case.
            // In this case, the layout is described through the InputControlLayout attribute of the class
            InputSystem.RegisterLayout<VrpnDpad>();
        }

        #endregion

        #region Methods

        public void OnUpdate()
        {
            // Get buttons states
            bool[] values = new bool[4]
            {
                UpButton != null && UpButton.IsActuated() ,
                DownButton != null && DownButton.IsActuated() ,
                LeftButton != null && LeftButton.IsActuated() ,
                RightButton != null && RightButton.IsActuated()
            };
            // Convert from bool array to ushort
            ushort shortvalue = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                    shortvalue += (ushort)(1 << i);
            }
            VrpnDpadState state = new VrpnDpadState
            {
                dpad = shortvalue
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
            foreach (VrpnMetaDpad inputSettings in VrpnInputSettings.Settings.Dpads)
            {
                VrpnDpad vrpnDevice = InputSystem.GetDevice(inputSettings.InputName) as VrpnDpad;
                if (vrpnDevice == null)
                    vrpnDevice = InputSystem.AddDevice<VrpnDpad>(inputSettings.InputName);
                if (vrpnDevice != null)
                {
                    vrpnDevice.vrpnInput = inputSettings;
                    Debug.Log("Added DPad device " + vrpnDevice.displayName);
                }
            }
        }

        #endregion
    }
}
