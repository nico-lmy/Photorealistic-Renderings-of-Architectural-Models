using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;


using TrackingState = UnityEngine.XR.InputTrackingState;

namespace Vrpn.Input
{
    // InputControlLayoutAttribute attribute is only necessary if you want
    // to override default behavior that occurs when registering your device
    // as a layout.
    // The most common use of InputControlLayoutAttribute is to direct the system
    // to a custom "state struct" through the `stateType` property. See below for details.
    [Preserve, InputControlLayout(displayName = "VRPN XR Controller", commonUsages = new[] { "LeftHand", "RightHand" }, stateType = typeof(VrpnXRControllerState))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VrpnXRController : XRController, IInputUpdateCallbackReceiver
    {
        #region Fields

        [SerializeField]
        protected VrpnMetaXRController vrpnInput;

        protected VrpnButton primaryButton;
        protected VrpnButton secondaryButton;
        protected VrpnStick primaryStick;
        protected VrpnStick secondaryStick;
        protected VrpnAxis trigger;
        protected VrpnAxis grip;
        protected VrpnButton startButton;
        protected VrpnButton primaryStickButton;
        protected VrpnButton secondaryStickButton;
        protected VrpnButton gripPressed;
        protected VrpnButton primaryStickPressed;
        protected VrpnButton secondaryStickPressed;
        protected VrpnButton primaryButtonTouched;
        protected VrpnButton secondaryButtonTouched;
        protected VrpnButton primaryStickTouched;
        protected VrpnButton secondaryStickTouched;
        protected VrpnButton triggerTouched;
        protected VrpnButton gripTouched;
        protected VrpnTracker devicePose;
        protected VrpnAxis batteryLevel;

        #endregion

        #region Properties

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton PrimaryButton
        {
            get
            {
                if (primaryButton == null && !string.IsNullOrEmpty(vrpnInput.PrimaryButtonName))
                    primaryButton = InputSystem.GetDevice(vrpnInput.PrimaryButtonName) as VrpnButton;
                return primaryButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton SecondaryButton
        {
            get
            {
                if (secondaryButton == null && !string.IsNullOrEmpty(vrpnInput.SecondaryButtonName))
                    secondaryButton = InputSystem.GetDevice(vrpnInput.SecondaryButtonName) as VrpnButton;
                return secondaryButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnStick PrimaryStick
        {
            get
            {
                if (primaryStick == null && !string.IsNullOrEmpty(vrpnInput.PrimaryStickName))
                    primaryStick = InputSystem.GetDevice(vrpnInput.PrimaryStickName) as VrpnStick;
                return primaryStick;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnStick SecondaryStick
        {
            get
            {
                if (secondaryStick == null && !string.IsNullOrEmpty(vrpnInput.SecondaryStickName))
                    secondaryStick = InputSystem.GetDevice(vrpnInput.SecondaryStickName) as VrpnStick;
                return secondaryStick;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnAxis TriggerAxis
        {
            get
            {
                if (trigger == null && !string.IsNullOrEmpty(vrpnInput.TriggerAxisName))
                    trigger = InputSystem.GetDevice(vrpnInput.TriggerAxisName) as VrpnAxis;
                return trigger;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnAxis GripAxis
        {
            get
            {
                if (grip == null && !string.IsNullOrEmpty(vrpnInput.GripAxisName))
                    grip = InputSystem.GetDevice(vrpnInput.GripAxisName) as VrpnAxis;
                return grip;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton StartButton
        {
            get
            {
                if (startButton == null && !string.IsNullOrEmpty(vrpnInput.StartButtonName))
                    startButton = InputSystem.GetDevice(vrpnInput.StartButtonName) as VrpnButton;
                return startButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton PrimaryStickButton
        {
            get
            {
                if (primaryStickButton == null && !string.IsNullOrEmpty(vrpnInput.PrimaryStickButtonName))
                    primaryStickButton = InputSystem.GetDevice(vrpnInput.PrimaryStickButtonName) as VrpnButton;
                return primaryStickButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton SecondaryStickButton
        {
            get
            {
                if (secondaryStickButton == null && !string.IsNullOrEmpty(vrpnInput.SecondaryStickButtonName))
                    secondaryStickButton = InputSystem.GetDevice(vrpnInput.SecondaryStickButtonName) as VrpnButton;
                return secondaryStickButton;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton GripPressedButton
        {
            get
            {
                if (gripPressed == null && !string.IsNullOrEmpty(vrpnInput.GripPressedButtonName))
                    gripPressed = InputSystem.GetDevice(vrpnInput.GripPressedButtonName) as VrpnButton;
                return gripPressed;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton PrimaryStickPressedButton
        {
            get
            {
                if (primaryStickPressed == null && !string.IsNullOrEmpty(vrpnInput.PrimaryStickPressedButtonName))
                    primaryStickPressed = InputSystem.GetDevice(vrpnInput.PrimaryStickPressedButtonName) as VrpnButton;
                return primaryStickPressed;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton SecondaryStickPressedButton
        {
            get
            {
                if (secondaryStickPressed == null && !string.IsNullOrEmpty(vrpnInput.SecondaryStickPressedButton))
                    secondaryStickPressed = InputSystem.GetDevice(vrpnInput.SecondaryStickPressedButton) as VrpnButton;
                return secondaryStickPressed;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton PrimaryButtonTouchedButton
        {
            get
            {
                if (primaryButtonTouched == null && !string.IsNullOrEmpty(vrpnInput.PrimaryButtonTouchedButtonName))
                    primaryButtonTouched = InputSystem.GetDevice(vrpnInput.PrimaryButtonTouchedButtonName) as VrpnButton;
                return primaryButtonTouched;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton SecondaryButtonTouchedButton
        {
            get
            {
                if (secondaryButtonTouched == null && !string.IsNullOrEmpty(vrpnInput.SecondaryButtonTouchedButtonName))
                    secondaryButtonTouched = InputSystem.GetDevice(vrpnInput.SecondaryButtonTouchedButtonName) as VrpnButton;
                return secondaryButtonTouched;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton PrimaryStickTouchedButton
        {
            get
            {
                if (primaryStickTouched == null && !string.IsNullOrEmpty(vrpnInput.PrimaryStickTouchedButtonName))
                    primaryStickTouched = InputSystem.GetDevice(vrpnInput.PrimaryStickTouchedButtonName) as VrpnButton;
                return primaryStickTouched;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton SecondaryStickTouchedButton
        {
            get
            {
                if (secondaryStickTouched == null && !string.IsNullOrEmpty(vrpnInput.SecondaryStickTouchedButtonName))
                    secondaryStickTouched = InputSystem.GetDevice(vrpnInput.SecondaryStickTouchedButtonName) as VrpnButton;
                return secondaryStickTouched;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton TriggerTouchedButton
        {
            get
            {
                if (triggerTouched == null && !string.IsNullOrEmpty(vrpnInput.TriggerTouchedButtonName))
                    triggerTouched = InputSystem.GetDevice(vrpnInput.TriggerTouchedButtonName) as VrpnButton;
                return triggerTouched;
            }
        }

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnButton GripTouchedButton
        {
            get
            {
                if (gripTouched == null && !string.IsNullOrEmpty(vrpnInput.GripTouchedButtonName))
                    gripTouched = InputSystem.GetDevice(vrpnInput.GripTouchedButtonName) as VrpnButton;
                return gripTouched;
            }
        }

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

        /// <summary>
        /// Cache the device
        /// </summary>
        public VrpnAxis BatteryLevelAxis
        {
            get
            {
                if (batteryLevel == null && !string.IsNullOrEmpty(vrpnInput.BatteryLevelAxisName))
                    batteryLevel = InputSystem.GetDevice(vrpnInput.BatteryLevelAxisName) as VrpnAxis;
                return batteryLevel;
            }
        }

        public ButtonControl primaryButtonControl { get; private set; }
        public ButtonControl secondaryButtonControl { get; private set; }
        public ButtonControl startButtonControl { get; private set; }
        public ButtonControl primaryStickButtonControl { get; private set; }
        public ButtonControl secondaryStickButtonControl { get; private set; }
        public ButtonControl gripPressedControl { get; private set; }
        public ButtonControl primaryStickPressedControl { get; private set; }
        public ButtonControl secondaryStickPressedControl { get; private set; }
        public ButtonControl primaryButtonTouchedControl { get; private set; }
        public ButtonControl secondaryButtonTouchedControl { get; private set; }
        public ButtonControl primaryStickTouchedControl { get; private set; }
        public ButtonControl secondaryStickTouchedControl { get; private set; }
        public ButtonControl triggerTouchedControl { get; private set; }
        public ButtonControl gripTouchedControl { get; private set; }
        public Vector2Control primaryStickControl { get; private set; }
        public Vector2Control secondaryStickControl { get; private set; }
        public Vector3Control velocityControl { get; private set; }
        public Vector3Control angularVelocityControl { get; private set; }
        public AxisControl triggerControl { get; private set; }
        public AxisControl gripControl { get; private set; }
        public AxisControl batteryLevelControl { get; private set; }

        #endregion

        #region Constructors

        static VrpnXRController()
        {
            // RegisterLayout() adds a "control layout" to the system.
            // These can be layouts for individual controls (like sticks)
            // or layouts for entire devices (which are themselves
            // control) like in our case.
            // In this case, the layout is described through the InputControlLayout attribute of the class
            InputSystem.RegisterLayout<VrpnXRController>();
        }

        #endregion

        #region Methods

        public void OnUpdate()
        {
            // Get buttons states
            bool[] values = new bool[14]
            {
                PrimaryButton != null && PrimaryButton.IsActuated() ,
                SecondaryButton != null && SecondaryButton.IsActuated() ,
                StartButton != null && StartButton.IsActuated() ,
                PrimaryStickButton != null && PrimaryStickButton.IsActuated(),
                SecondaryStickButton != null && SecondaryStickButton.IsActuated(),
                GripPressedButton != null && GripPressedButton.IsActuated(),
                PrimaryStickPressedButton != null && PrimaryStickPressedButton.IsActuated(),
                SecondaryStickPressedButton != null && SecondaryStickPressedButton.IsActuated(),
                PrimaryButtonTouchedButton != null && PrimaryButtonTouchedButton.IsActuated(),
                SecondaryButtonTouchedButton != null && SecondaryButtonTouchedButton.IsActuated(),
                PrimaryStickTouchedButton != null && PrimaryStickTouchedButton.IsActuated(),
                SecondaryStickTouchedButton != null && SecondaryStickTouchedButton.IsActuated(),
                TriggerTouchedButton != null && TriggerTouchedButton.IsActuated(),
                GripTouchedButton != null && GripTouchedButton.IsActuated()
            };
            // Convert from bool array to ushort
            ushort shortvalue = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                    shortvalue += (ushort)(1 << i);
            }
            VrpnXRControllerState state = new()
            {
                buttons = shortvalue,
                primaryStick = PrimaryStick != null ? PrimaryStick.stick.ReadUnprocessedValue() : Vector2.zero,
                secondaryStick = SecondaryStick != null ? SecondaryStick.stick.ReadUnprocessedValue() : Vector2.zero,
                isTracked = true, // seems like this is important !
                trackingState = (int)(TrackingState.Position | TrackingState.Rotation),
                position = DevicePoseTracker != null ? DevicePoseTracker.devicePosition.ReadUnprocessedValue() : Vector3.zero,
                rotation = DevicePoseTracker != null ? DevicePoseTracker.deviceRotation.ReadUnprocessedValue() : Quaternion.identity,
                trigger = TriggerAxis != null ? TriggerAxis.axis.ReadUnprocessedValue() : 0f,
                grip = GripAxis != null ? GripAxis.axis.ReadUnprocessedValue() : 0f,
                batteryLevel = BatteryLevelAxis != null ? BatteryLevelAxis.axis.ReadUnprocessedValue() : 0f,
            };
            InputSystem.QueueStateEvent(this, state);
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();
            primaryButtonControl = GetChildControl<ButtonControl>("primaryButton");
            secondaryButtonControl = GetChildControl<ButtonControl>("secondaryButton");
            startButtonControl = GetChildControl<ButtonControl>("startButton");
            primaryStickButtonControl = GetChildControl<ButtonControl>("primaryStickButton");
            secondaryStickButtonControl = GetChildControl<ButtonControl>("secondaryStickButton");
            gripPressedControl = GetChildControl<ButtonControl>("gripPressed");
            primaryStickPressedControl = GetChildControl<ButtonControl>("primaryStickPressed");
            secondaryStickPressedControl = GetChildControl<ButtonControl>("secondaryStickPressed");
            primaryButtonTouchedControl = GetChildControl<ButtonControl>("primaryButtonTouched");
            secondaryButtonTouchedControl = GetChildControl<ButtonControl>("secondaryButtonTouched");
            primaryStickTouchedControl = GetChildControl<ButtonControl>("primaryStickTouched");
            secondaryStickTouchedControl = GetChildControl<ButtonControl>("secondaryStickTouched");
            triggerTouchedControl = GetChildControl<ButtonControl>("triggerTouched");
            gripTouchedControl = GetChildControl<ButtonControl>("gripTouched");
            primaryStickControl = GetChildControl<Vector2Control>("primaryStick");
            secondaryStickControl = GetChildControl<Vector2Control>("secondaryStick");
            triggerControl = GetChildControl<AxisControl>("trigger");
            gripControl = GetChildControl<AxisControl>("grip");
            batteryLevelControl = GetChildControl<AxisControl>("batteryLevel");
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
            foreach (VrpnMetaXRController inputSettings in VrpnInputSettings.Settings.XRControllers)
            {
                VrpnXRController vrpnDevice = InputSystem.GetDevice(inputSettings.InputName) as VrpnXRController;
                vrpnDevice ??= InputSystem.AddDevice<VrpnXRController>(inputSettings.InputName);
                if (vrpnDevice != null)
                {
                    vrpnDevice.vrpnInput = inputSettings;
                    if (inputSettings.Hand == VrpnMetaXRController.HandType.LeftHand)
                        InputSystem.SetDeviceUsage(vrpnDevice, CommonUsages.LeftHand);
                    else if (inputSettings.Hand == VrpnMetaXRController.HandType.RightHand)
                        InputSystem.SetDeviceUsage(vrpnDevice, CommonUsages.RightHand);
                    Debug.Log("Added XR controller device " + vrpnDevice.displayName);
                }
            }
        }

        #endregion
    }
}
