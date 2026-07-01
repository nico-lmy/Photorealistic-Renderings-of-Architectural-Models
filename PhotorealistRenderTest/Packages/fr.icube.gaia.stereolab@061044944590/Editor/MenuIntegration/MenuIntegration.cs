using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Stereolab;

namespace Stereolab.Editor
{
    /// <summary>
    /// Create menu items to speed up the integration of interactions referenced here into a project.
    /// </summary>
    /// <remarks>
    /// The prefabs referenced can be found in <see cref="PrefabManagerSO"/>. This is where you have to add any new prefab newly created for the package.
    /// </remarks>
    public static class MenuIntegration
    {
        private const int MenuPriority = 100;
        private const string PrefabManagerPath = "Packages/fr.icube.gaia.stereolab/ScriptableObjects/EditorExtensions/PrefabManager.asset";

        private static PrefabManagerSO LocatePrefabManager() => AssetDatabase.LoadAssetAtPath<PrefabManagerSO>(PrefabManagerPath);

        [MenuItem("GameObject/Stereolab/Stereolab Instance")]
        public static void CreateStereolabInstance()
        {
            SafeInstantiate(prefabManager => prefabManager.StereolabInstance);
        }

        [MenuItem("GameObject/Stereolab/Stereo Projection/Projection Plane")]
        public static void CreateProjectionPlane()
        {
            SafeInstantiate(prefabManager => prefabManager.StereoProjection.ProjectionPlane);
        }

        [MenuItem("GameObject/Stereolab/Stereo Projection/Projection Plane Camera")]
        public static void CreateProjectionPlaneCamera()
        {
            SafeInstantiate(prefabManager => prefabManager.StereoProjection.ProjectionPlaneCamera);
        }

        /// <summary>
        ///  Instantiate a Prefab reference in the PrefabManager ScriptableObject
        /// </summary>
        /// <param name="itemSelector"></param>
        private static void SafeInstantiate(Func<PrefabManagerSO, GameObject> itemSelector)
        {
            var prefabManager = LocatePrefabManager();

            if (!prefabManager)
            {
            Debug.LogWarning($"PrefabManager not found at path {PrefabManagerPath}");
            return;
            }

            var item = itemSelector(prefabManager);
            var instance = PrefabUtility.InstantiatePrefab(item, Selection.activeTransform);

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }  
    }
}
