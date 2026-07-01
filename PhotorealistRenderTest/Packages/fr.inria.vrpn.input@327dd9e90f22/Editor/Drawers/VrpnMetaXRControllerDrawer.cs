using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace Vrpn.Input.Editor
{
    // This class is somehow necessary to prevent the list editor to mess up when adding/removing elements
    [CustomPropertyDrawer(typeof(VrpnMetaXRController))]
    public class VrpnMetaXRControllerDrawer : PropertyDrawer
    {
        #region Methods

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            VisualElement container = new VisualElement();

            // Create and add property fields.
            container.Add(new PropertyField(property.FindPropertyRelative("InputName")));
            container.Add(new PropertyField(property.FindPropertyRelative("Hand")));
            container.Add(new PropertyField(property.FindPropertyRelative("PrimaryButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("SecondaryButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("PrimaryStickName")));
            container.Add(new PropertyField(property.FindPropertyRelative("SecondaryStickName")));
            container.Add(new PropertyField(property.FindPropertyRelative("TriggerAxisName")));
            container.Add(new PropertyField(property.FindPropertyRelative("GripAxisName")));
            container.Add(new PropertyField(property.FindPropertyRelative("StartButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("PrimaryStickButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("SecondaryStickButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("GripPressedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("PrimaryStickPressedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("SecondaryStickPressedButton")));
            container.Add(new PropertyField(property.FindPropertyRelative("PrimaryButtonTouchedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("SecondaryButtonTouchedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("PrimaryStickTouchedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("SecondaryStickTouchedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("GripTouchedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("TriggerTouchedButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("BatteryLevelAxisName")));
            container.Add(new PropertyField(property.FindPropertyRelative("DevicePoseTrackerName")));

            return container;
        }

        #endregion
    }
}
