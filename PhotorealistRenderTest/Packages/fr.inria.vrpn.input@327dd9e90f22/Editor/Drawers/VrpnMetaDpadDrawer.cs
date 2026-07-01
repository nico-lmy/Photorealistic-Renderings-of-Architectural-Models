using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace Vrpn.Input.Editor
{
    // This class is somehow necessary to prevent the list editor to mess up when adding/removing elements
    [CustomPropertyDrawer(typeof(VrpnMetaDpad))]
    public class VrpnMetaDpadDrawer : PropertyDrawer
    {
        #region Methods

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            VisualElement container = new VisualElement();

            // Create and add property fields.
            container.Add(new PropertyField(property.FindPropertyRelative("InputName")));
            container.Add(new PropertyField(property.FindPropertyRelative("UpButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("DownButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("LeftButtonName")));
            container.Add(new PropertyField(property.FindPropertyRelative("RightButtonName")));

            return container;
        }

        #endregion
    }
}
