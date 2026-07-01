using System;

namespace Vrpn.Input
{
    [Serializable]
    public class VrpnMetaXRController
    {
        #region Enums

        public enum HandType
        {
            None = 0,
            RightHand = 1,
            LeftHand = 2
        }

        #endregion

        #region Fields

        public string InputName;
        public HandType Hand;
        public string PrimaryButtonName;
        public string SecondaryButtonName;
        public string PrimaryStickName;
        public string SecondaryStickName;
        public string TriggerAxisName;
        public string GripAxisName;
        public string StartButtonName;
        public string PrimaryStickButtonName;
        public string SecondaryStickButtonName;
        public string GripPressedButtonName;
        public string PrimaryStickPressedButtonName;
        public string SecondaryStickPressedButton;
        public string PrimaryButtonTouchedButtonName;
        public string SecondaryButtonTouchedButtonName;
        public string PrimaryStickTouchedButtonName;
        public string SecondaryStickTouchedButtonName;
        public string GripTouchedButtonName;
        public string TriggerTouchedButtonName;
        public string BatteryLevelAxisName;
        public string DevicePoseTrackerName;

        #endregion
    }
}
