using UnityEngine;

namespace Stereolab.StereoProjection
{
    /// <summary>
    /// Rig of the stereoscopic multi-display tool, handling synchronization.
    /// </summary>
    /// <remarks>
    /// There must be only one active in the scene.
    /// </remarks>
    public class StereolabInstance : MonoBehaviour
    {
        [SerializeField]
        /// <summary>
        /// Key used to manually invert back the left/right frame in case of a frame drop that already inverted them.
        /// </summary>
        private KeyCode frameInversionKey = KeyCode.Backspace;

        /// <summary>
        /// Keeps track of the amount of instances in the scene, to keep it at one.
        /// </summary>
        private static int instancesCount = 0;

        /// <summary>
        /// Controls which eye is being rendered at the given frame for the stereoscopic cameras.
        /// </summary>
        public static bool renderLeftEye { get; private set; } = true;

        // Nicolas LAMY - July 2026 - Non-destructive addition (autoFlip=true by default -> behavior remains unchanged) to suspend stereo flipping while the path tracer is accumulating data
        
        /// <summary>
        /// Allows to pause automatic switching during image accumulation.
        /// </summary>
        public static bool autoFlip = true;

        // Explicitly forces the rendered eye (used during accumulation)
        public static void ForceEye(bool left) { renderLeftEye = left; }

        // Nicolas LAMY - July 2026 - End of the modifications

        private void Awake()
        {
            // StereolabInstance represents one stereoscopic 3D rig, so only one should exist in the scene
            instancesCount += 1;
            if (instancesCount > 1)
            {
                Debug.LogError("Only one StereolabInstance can exist in the scene. This one will be deleted.");
                Destroy(this);
                return;
            }

            // Lock framerate et vsync for the stereo rendering
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 120;

            // Hide the cursor
            Cursor.visible = false;
        }

        private void Update()
        {
            // Flip the eye rendered at each frame.
            // Nicolas LAMY - July 2026 - Non-destructive addition (autoFlip=true by default -> behavior remains unchanged) to suspend stereo flipping while the path tracer is accumulating data
            if (autoFlip) 
            // Nicolas LAMY - July 2026 - End of the modifications
                renderLeftEye = !renderLeftEye;

            // Allow to swap eye frames manually in case of a frame drop
            // TODO: Find a way to avoid this problem and apply the inversion automatically
            if (Input.GetKeyDown(frameInversionKey))
            {
                renderLeftEye = !renderLeftEye;
            }
        }

    }

}