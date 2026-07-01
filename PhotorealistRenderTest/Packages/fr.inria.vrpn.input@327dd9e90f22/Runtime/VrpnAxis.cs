using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;

namespace Vrpn.Input
{
    [InputControlLayout(displayName = "VRPN Axis", stateType = typeof(VrpnAxisState))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VrpnAxis : InputDevice, IInputUpdateCallbackReceiver
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

        [Preserve, InputControl(offset = 0, name = "axis", displayName = "Axis", layout = "Axis")]
        public AxisControl axis { get; private set; }

        #endregion

        #region Constructors

        static VrpnAxis()
        {
            // RegisterLayout() adds a "control layout" to the system.
            // These can be layouts for individual controls (like sticks)
            // or layouts for entire devices (which are themselves
            // control) like in our case.
            // In this case, the layout is described through the InputControlLayout attribute of the class
            InputSystem.RegisterLayout<VrpnAxis>();
        }

        #endregion

        #region Methods

        public void OnUpdate()
        {
            VrpnAxisState state = new VrpnAxisState
            {
                axis = (float)VrpnManager.Analog(DeviceAdress, vrpnInput.Index)
            };
            InputSystem.QueueStateEvent(this, state);
        }

        protected override void FinishSetup()
        {
            axis = GetChildControl<AxisControl>("axis");
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
            foreach (VrpnInput inputSettings in VrpnInputSettings.Settings.Inputs.Where(input => input.InputType == ClusterInputType.Axis))
            {
                VrpnAxis vrpnAxis = InputSystem.GetDevice(inputSettings.InputName) as VrpnAxis;
                if (vrpnAxis == null)
                    vrpnAxis = InputSystem.AddDevice<VrpnAxis>(inputSettings.InputName);
                if (vrpnAxis != null)
                {
                    vrpnAxis.vrpnInput = inputSettings;
                    Debug.Log("Added Axis device " + vrpnAxis.displayName);
                }
            }
        }

        #endregion
    }
}