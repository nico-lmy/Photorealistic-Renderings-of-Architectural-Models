using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace Vrpn.Input
{
    public struct VrpnXRControllerState : IInputStateTypeInfo
    {
        #region Fields

        [Preserve, InputControl(name = "primaryButton", displayName = "Primary Button", layout = "Button", usages = new[] { "PrimaryAction", "Submit" }, bit = 0)]
        [InputControl(name = "secondaryButton", displayName = "Secondary Button", layout = "Button", usages = new[] { "SecondaryAction", "Back" }, bit = 1)]
        [InputControl(name = "startButton", displayName = "Start Button", layout = "Button", usages = new[] { "Menu" }, bit = 2)]
        [InputControl(name = "primaryStickButton", displayName = "Primary Stick Button", layout = "Button", usages = new[] { "HatSwitch" }, bit = 3)]
        [InputControl(name = "secondaryStickButton", displayName = "Secondary Stick Button", layout = "Button", bit = 4)]
        [InputControl(name = "gripPressed", displayName = "Grip Pressed", layout = "Button", bit = 5)]
        [InputControl(name = "primaryStickPressed", displayName = "Primary Stick Pressed", layout = "Button", usages = new[] { "Primary2DAxisClick", "JoystickClicked" }, bit = 6)]
        [InputControl(name = "secondaryStickPressed", displayName = "Secondary Stick Pressed", layout = "Button", usages = new[] { "Secondary2DAxisClick", "TouchpadClicked" }, bit = 7)]
        [InputControl(name = "primaryButtonTouched", displayName = "Primary Button Touched", layout = "Button", bit = 8)]
        [InputControl(name = "secondaryButtonTouched", displayName = "Secondary Button Touched", layout = "Button", bit = 9)]
        [InputControl(name = "primaryStickTouched", displayName = "Primary Stick Touched", layout = "Button", usages = new[] { "Primary2DAxisTouch", "JoystickTouched" }, bit = 10)]
        [InputControl(name = "secondaryStickTouched", displayName = "Secondary Stick Touched", layout = "Button", usages = new[] { "Secondary2DAxisTouch", "TouchpadTouched" }, bit = 11)]
        [InputControl(name = "triggerTouched", displayName = "Trigger Touched", layout = "Button", bit = 12)]
        [InputControl(name = "gripTouched", displayName = "Grip Touched", layout = "Button", bit = 13)]
        public uint buttons;

        [Preserve, InputControl(name = "primaryStick", displayName = "Primary Stick", layout = "Stick", aliases = new[] { "Primary2DAxis", "Joystick" })]
        // Order is Horizontal, Vertical
        public Vector2 primaryStick;

        [Preserve, InputControl(name = "secondaryStick", displayName = "Secondary Stick", layout = "Stick", aliases = new[] { "Secondary2DAxis", "Touchpad" })]
        // Order is Horizontal, Vertical
        public Vector2 secondaryStick;

        /// <summary>
        /// name attribute is important so that TrackedDevice class can find this input
        /// </summary>
        /// <see cref="UnityEngine.InputSystem.TrackedDevice"/>
        [Preserve, InputControl(name = "isTracked", displayName = "Tracked", layout = "Button")]
        public bool isTracked;

        [Preserve, InputControl(name = "trackingState", displayName = "TrackingState", layout = "Integer")]
        public int trackingState;

        /// <summary>
        /// name attribute is important so that TrackedDevice class can find this input
        /// </summary>
        /// <see cref="UnityEngine.InputSystem.TrackedDevice"/>
        [Preserve, InputControl(name = "devicePosition", displayName = "Position", layout = "Vector3", usages = new[] { "Position" })]
        public Vector3 position;

        /// <summary>
        /// name attribute is important so that TrackedDevice class can find this input
        /// </summary>
        /// <see cref="UnityEngine.InputSystem.TrackedDevice"/>
        [Preserve, InputControl(name = "deviceRotation", displayName = "Rotation", layout = "Quaternion", usages = new[] { "Orientation" })]
        public Quaternion rotation;

        [Preserve, InputControl(name = "trigger", displayName = "Trigger", layout = "Axis", usages = new[] { "PrimaryTrigger" })]
        public float trigger;

        [Preserve, InputControl(name = "grip", displayName = "Grip", layout = "Axis", usages = new[] { "SecondaryTrigger" })]
        public float grip;

        [Preserve, InputControl(name = "batteryLevel", displayName = "Battery Level", layout = "Axis", usages = new[] { "BatteryStrength" })]
        public float batteryLevel;

        #endregion

        #region Properties

        // Every state format is tagged with a FourCC code that is used for type
        // checking. The characters can be anything. Choose something that allows
        // you do easily recognize memory belonging to your own device.
        public FourCC format => new('V', 'R', 'P', 'C');

        #endregion
    }
}
