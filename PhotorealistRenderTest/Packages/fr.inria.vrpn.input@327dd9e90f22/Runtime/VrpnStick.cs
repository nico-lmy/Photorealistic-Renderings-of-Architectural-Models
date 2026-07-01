using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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
    [Preserve, InputControlLayout(displayName = "VRPN Stick", stateType = typeof(VrpnStickState))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VrpnStick : InputDevice, IInputUpdateCallbackReceiver
    {
        #region Fields

        [SerializeField]
        protected VrpnMetaStick vrpnInput;

        protected VrpnAxis verticalAxis;
        protected VrpnAxis horinzontalAxis;

        #endregion

        #region Properties

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnAxis VerticalAxis
        {
            get
            {
                if (verticalAxis == null && !string.IsNullOrEmpty(vrpnInput.VerticalAxisName))
                    verticalAxis = InputSystem.GetDevice(vrpnInput.VerticalAxisName) as VrpnAxis;
                return verticalAxis;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnAxis HorizontalAxis
        {
            get
            {
                if (horinzontalAxis == null && !string.IsNullOrEmpty(vrpnInput.HorizontalAxisName))
                    horinzontalAxis = InputSystem.GetDevice(vrpnInput.HorizontalAxisName) as VrpnAxis;
                return horinzontalAxis;
            }
        }

        [Preserve, InputControl(name = "stick", displayName = "Stick", layout = "Axis")]
        public Vector2Control stick { get; private set; }

        #endregion

        #region Constructors

        static VrpnStick()
        {
            // RegisterLayout() adds a "control layout" to the system.
            // These can be layouts for individual controls (like sticks)
            // or layouts for entire devices (which are themselves
            // control) like in our case.
            // In this case, the layout is described through the InputControlLayout attribute of the class
            InputSystem.RegisterLayout<VrpnStick>();
        }

        #endregion

        #region Methods

        public void OnUpdate()
        {
            VrpnStickState state = new VrpnStickState
            {
                stick = new Vector2(
                    HorizontalAxis != null ? HorizontalAxis.axis.ReadUnprocessedValue() : 0f,
                    VerticalAxis != null ? VerticalAxis.axis.ReadUnprocessedValue() : 0f
                )
            };
            InputSystem.QueueStateEvent(this, state);
        }

        protected override void FinishSetup()
        {
            stick = GetChildControl<Vector2Control>("stick");
            base.FinishSetup();
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
            foreach (VrpnMetaStick inputSettings in VrpnInputSettings.Settings.Sticks)
            {
                VrpnStick vrpnDevice = InputSystem.GetDevice(inputSettings.InputName) as VrpnStick;
                if (vrpnDevice == null)
                    vrpnDevice = InputSystem.AddDevice<VrpnStick>(inputSettings.InputName);
                if (vrpnDevice != null)
                {
                    vrpnDevice.vrpnInput = inputSettings;
                    Debug.Log("Added Stick device " + vrpnDevice.displayName);
                }
            }
        }

        #endregion
    }
}
