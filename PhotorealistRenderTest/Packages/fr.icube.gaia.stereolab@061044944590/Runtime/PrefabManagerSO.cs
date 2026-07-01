using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Stereolab
{
    //* This whole thing is there to allow creating prefabs of the package from the right-click menu in the Hierarchy.
    
    // Commented out because a single instance is needed (at the root of the package)
    [CreateAssetMenuAttribute(menuName = "Stereolab/PrefabManager")]
    public class PrefabManagerSO : ScriptableObject
    {
        #if UNITY_EDITOR
        public GameObject StereolabInstance;
        public StereoProjectionPrefabs StereoProjection;

        [Serializable]
        public class StereoProjectionPrefabs
        {
            public GameObject ProjectionPlane;
            public GameObject ProjectionPlaneCamera;
        }
        #endif
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(PrefabManagerSO))]
    public class PrefabManagerSOEditor : Editor
    {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.HelpBox("If you move this file somewhere else, also change the path in the MenuIntegration script.", MessageType.Info);
    }
    }
    #endif
}
