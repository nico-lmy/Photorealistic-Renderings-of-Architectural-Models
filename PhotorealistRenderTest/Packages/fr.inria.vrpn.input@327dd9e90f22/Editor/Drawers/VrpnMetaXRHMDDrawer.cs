using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace Vrpn.Input.Editor
{
    // This class is somehow necessary to prevent the list editor to mess up when adding/removing elements
    [CustomPropertyDrawer(typeof(VrpnMetaXRHMD))]
    public class VrpnMetaXRHMDDrawer : PropertyDrawer
    {
        #region Methods

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            VisualElement container = new VisualElement();

            // Create and add property fields.
            container.Add(new PropertyField(property.FindPropertyRelative("InputName")));
            container.Add(new PropertyField(property.FindPropertyRelative("DevicePoseTrackerName")));

            return container;
        }

        #endregion
    }
}
