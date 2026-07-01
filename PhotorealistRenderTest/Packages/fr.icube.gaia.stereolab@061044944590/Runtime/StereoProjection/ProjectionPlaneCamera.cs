using UnityEngine;

namespace Stereolab.StereoProjection
{
    /// <summary>
    /// Update the camera attached to the same GameObject so it's projection matrix and rotation are rendering an instance of <see cref="ProjectionPlane"/>.
    /// </summary>
    /// <remarks>
    /// The GameObject it is attached to also requires a Camera component.
    /// </remarks>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class ProjectionPlaneCamera : MonoBehaviour
    {
        [Header("Projection plane")]

        /// <summary>
        /// Camera component attached the same game-object that this script.
        /// </summary>
        [SerializeField]
        [Tooltip("Camera component attached to the same object than this script.")]
        private new Camera camera;

        /// <summary>
        /// Screen onto which we should map the camera's orientation and projection matrix.
        /// </summary>
        [SerializeField]
        [Tooltip("Screen onto which we should map the camera's orientation and projection matrix.")]
        public ProjectionPlane projectionScreen;

        /// <summary>
        /// Transform used to synchronize the viewpoint of the user onto the screens.
        /// </summary>
        [SerializeField]
        [Tooltip("Transform used to synchronize the viewpoint of the user onto the screens.")]
        public Transform viewPointController;

        /// <summary>
        /// Set the near clipping plan distance of the camera to match with the distance to the projection plane.
        /// </summary>
        /// <remarks>
        /// If true, overrides the near clipping plan property of the camera.
        /// </remarks>
        [SerializeField]
        [Tooltip("Set the near clipping plan distance of the camera to match with the distance to the projection plane.")]
        private bool alignNearClippingPlanOnScreen = false;

        
        [Header("Stereoscopy")]

        /// <summary>
        /// Enable the active stereoscopic rendering for this camera.
        /// </summary>
        [SerializeField]
        [Tooltip("Enable the active stereoscopic rendering for this camera.")]
        public bool stereoEnabled = false;

        /// <summary>
        /// Distance between the middle of the eyes of the user in meter. 
        /// </summary>
        /// <remarks>
        /// When <see cref="stereoEnabled"/> is true, will be used to offset the camera, generating the left/right eye representations.
        /// </remarks>
        [SerializeField]
        [Tooltip("Distance between the middle of the eyes of the user in meter.")]
        private float interPupillaryDistance = 0.064f;

        [SerializeField]
        [Tooltip("Inverse the eye rendering")]
        private bool inverseEye = false;

        [Header("Helpers")]

        /// <summary>
        /// Enable the gizmos for this camera.
        /// </summary>
        [SerializeField]
        [Tooltip("Enable the gizmos for this camera.")]
        private bool drawGizmos = false;

        /// <summary>
        /// Enable the output of the camera to a RenderTexture mapped on the <see cref="projectionScreen"/> selected.
        /// </summary>
        /// <remarks>
        /// The <see cref="projectionScreen"/> selected must have the same property enabled for it to work.
        /// </remarks>
        [SerializeField]
        [Tooltip("Enable the output of the camera to a RenderTexture mapped on the projection screen selected.")]
        private bool drawDebugTexture = false;

        # region Camera
        /// <summary>
        /// GameObject storing the camera rendering to the debug display of the ProjectionScreen (see <see cref="debugCamera"/>).
        /// </summary>
        /// <remarks>
        /// We use a child GameObject because a single one can't have two <see cref="Camera"/> components.
        /// </remarks>
        private GameObject debugCameraGO;

        /// <summary>
        /// A clone of <see cref="camera"/> that renders to a texture, emulating the monitor in real life.
        /// </summary>
        private Camera debugCamera;

        /// <summary>
        /// Normalized direction vector from the camera to the middle of the screen in world space. 
        /// </summary>
        private Vector3 viewDirection;
        # endregion

        # region Eye position and vectors to the screen corners
        /// <summary>
        /// Bottom-left corner of the screen in world space.
        /// </summary>
        private Vector3 screenBottomLeft => projectionScreen.bottomLeft;

        /// <summary>
        /// Bottom-right corner of the screen in world space.
        /// </summary>
        private Vector3 screenBottomRight => projectionScreen.bottomRight;
        
        /// <summary>
        /// Top-left corner of the screen in world space.
        /// </summary>
        private Vector3 screenTopLeft => projectionScreen.topLeft;
        
        /// <summary>
        /// Top-right corner of the screen in world space.
        /// </summary>
        private Vector3 screenTopRight => projectionScreen.topRight;
        # endregion

        private void Awake()
        {
            // TODO: Trigger only when going in play mode
            // Debug.Assert(projectionScreen != null);
            camera = GetComponent<Camera>();

            // Prevent remaining DebugCamera objects when we go in/out of play mode
            Transform debugCameraTransform = transform.Find("[DebugCamera]");
            if (debugCameraTransform != null)
            {
                DestroyImmediate(debugCameraTransform.gameObject);
                debugCameraGO = null;
                debugCamera = null;
            }

            if (viewPointController == null)
            {
                viewPointController = transform;
            }
        }

        private void OnDrawGizmos()
        {
            if (projectionScreen == null)
                return;

            if (drawGizmos)
            {
                Vector3 position = viewPointController.position;
                Gizmos.color = projectionScreen.screenGizmoColor;

                if (stereoEnabled)
                {
                    // Indicates the projection of the camera to the corners of the selected screen
                    Vector3 rightEyeOffset = viewPointController.right * interPupillaryDistance/2f;
                    Vector3 rightEyePosition = position + rightEyeOffset;

                    Gizmos.DrawLine(rightEyePosition, screenBottomLeft);
                    Gizmos.DrawLine(rightEyePosition, screenBottomRight);
                    Gizmos.DrawLine(rightEyePosition, screenTopLeft);
                    Gizmos.DrawLine(rightEyePosition, screenTopRight);

                    Vector3 leftEyePosition = position - rightEyeOffset;
                    Gizmos.DrawLine(leftEyePosition, screenBottomLeft);
                    Gizmos.DrawLine(leftEyePosition, screenBottomRight);
                    Gizmos.DrawLine(leftEyePosition, screenTopLeft);
                    Gizmos.DrawLine(leftEyePosition, screenTopRight);
                }
                else 
                {
                    // Indicates the projection of the camera to the corners of the selected screen
                    Gizmos.DrawLine(position, screenBottomLeft);
                    Gizmos.DrawLine(position, screenBottomRight);
                    Gizmos.DrawLine(position, screenTopLeft);
                    Gizmos.DrawLine(position, screenTopRight);
                }

                Vector3 lineEnd = position + viewDirection * 0.2f;
                Gizmos.DrawLine(position, lineEnd);
                Gizmos.DrawSphere(lineEnd, 0.008f);
            }
        }

        private void LateUpdate()
        {
            // Camera behaviours in late updateto keep track of the objects movement that might have happened in the Update call of the same frame
            
            GeneralizedPerspectiveProjection();

            // Render the debug texture only in the editor
            if (!Application.isPlaying)
            {
                RenderToDebugTexture();
            }
        }

        /// <summary>
        /// Compute the projection matrix of the camera based on the projection screen, allowing for an off-center and off-axis perspective projection.
        /// </summary>
        /// <remarks>
        /// Implementation of Generalized Perspective Projection by Robert Kooima (2008-2009).
        /// See: http://160592857366.free.fr/joe/ebooks/ShareData/Generalized%20Perspective%20Projection.pdf
        /// </remarks>
        private void GeneralizedPerspectiveProjection()
        {
            if (projectionScreen == null) 
                return; 

            // Quick accessors
            Vector3 screenUpVector = projectionScreen.directionUp;
            Vector3 screenRightVector = projectionScreen.directionRight;
            Vector3 screenNormalVector = projectionScreen.directionNormal;

            Vector3 eyePosition = viewPointController.position;

            // Update the eye-to-screen vector from healthcheck and gizmo purposes
            viewDirection = (projectionScreen.transform.position - eyePosition).normalized;

            // If the camera is at the back of the screen, don't go any further to avoid matrices problems
            if (Vector3.Dot(screenNormalVector, viewDirection) >= 0f)
            {
                if (Application.isPlaying)
                {   
                    // Error case not logged in editor view to avoid spamming messages
                    Debug.LogWarningFormat("The camera attached to {0} should face the screen", gameObject.name);
                }
                return;
            }

            // Offset the rendered eye position if we're in stereo mode
            if (stereoEnabled && Application.isPlaying)
            {
                // We use the view point of the user for the offset of the eyes, so the rotation to the screen doesn't mess with it
                Vector3 stereoEyeOffset = viewPointController.right * interPupillaryDistance/2f;
                int rightEyeOrientation = 0;
                if (!inverseEye)
                {
                    rightEyeOrientation = StereolabInstance.renderLeftEye ? -1 : 1;
                }
                else
                {
                    rightEyeOrientation = StereolabInstance.renderLeftEye ? 1 : -1;
                }
                eyePosition += rightEyeOrientation * stereoEyeOffset;
            }

            // Eye to projection screen corners
            Vector3 eyeToScreenBottomLeft = screenBottomLeft - eyePosition;
            Vector3 eyeToScreenBottomRight = screenBottomRight - eyePosition;
            Vector3 eyeToScreenTopLeft = screenTopLeft - eyePosition;
            Vector3 eyeToScreenTopRight = screenTopRight - eyePosition;

            // Quick explanation: distance to the plane = projection of any vector plane to point over the normal facing outward
            // Since our normal is normalized and facing inward, we replace by -1, hence the -Vector3.Dot(...)
            float eyeToScreenDistance = -Vector3.Dot(eyeToScreenBottomLeft, screenNormalVector);
            
            // Fix the near clipping plane to the screen if needed
            float nearClipPlane;
            if (alignNearClippingPlanOnScreen)
            {
                nearClipPlane = eyeToScreenDistance;
            }
            else 
            {
                nearClipPlane = camera.nearClipPlane;
            }

            // Projection matrix from frustum extents and near/far clipping planes
            float nearPlaneOverDistance = nearClipPlane / eyeToScreenDistance;
            float bottomFrustumExtent = Vector3.Dot(screenUpVector, eyeToScreenBottomLeft) * nearPlaneOverDistance;
            float topFrustumExtent = Vector3.Dot(screenUpVector, eyeToScreenTopLeft) * nearPlaneOverDistance;
            float leftFrustumExtent = Vector3.Dot(screenRightVector, eyeToScreenBottomLeft) * nearPlaneOverDistance;
            float rightFrustumExtent = Vector3.Dot(screenRightVector, eyeToScreenBottomRight) * nearPlaneOverDistance;
            Matrix4x4 projectionMatrix = Matrix4x4.Frustum(leftFrustumExtent, rightFrustumExtent, bottomFrustumExtent, topFrustumExtent, nearClipPlane, camera.farClipPlane);

            // Update the rotation and projection matrix of the camera
            camera.transform.rotation = Quaternion.LookRotation(-screenNormalVector, screenUpVector);

            camera.projectionMatrix = projectionMatrix;

            camera.targetDisplay = (int)projectionScreen.displayIndex;

            // Translation and rotation to eye position
            Matrix4x4 T = Matrix4x4.Translate(-eyePosition);
            Matrix4x4 R = Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation) * projectionScreen.transform.rotation);

            camera.worldToCameraMatrix = projectionScreen.worldToPlane * R * T;
        }

        /// <summary>
        /// Render the view from this camera onto the selected projection screen.
        /// </summary>
        /// <remarks>
        /// If the RenderTexture flickers, it means that two cameras are trying to render on the same screen. Disable the <see cref="drawDebugTexture"/> 
        /// property of one of them to fix it.
        /// </remarks>
        private void RenderToDebugTexture()
        {
            if (drawDebugTexture && projectionScreen.drawDebugTexture)
            {
                // We have to create a clone camera because a single camera can't render to a display and a texture in the same time.
                if (debugCameraGO == null)
                {
                    debugCameraGO = new GameObject("[DebugCamera]", typeof(Camera));

                    // Child of the camera, set everything at zero to use the camera's transform
                    debugCameraGO.transform.parent = this.transform;
                    debugCameraGO.transform.localPosition = Vector3.zero;
                    debugCameraGO.transform.localRotation = Quaternion.identity;
                    debugCameraGO.transform.localScale = Vector3.zero;

                    debugCamera = debugCameraGO.GetComponent<Camera>();
                    debugCamera.stereoTargetEye = StereoTargetEyeMask.None;
                }

                // Update the camera properties and target texture at every frame to account for serialized fields modifications
                debugCamera.CopyFrom(camera);
                debugCamera.targetTexture = projectionScreen.debugTexture;
            }
            else 
            {
                if (projectionScreen != null && !projectionScreen.drawDebugTexture && drawDebugTexture)
                {
                    Debug.LogWarning("The selected ProjectionPlane has its property drawDebugTexture set to false. Turn it on to display the debug texture of this camera.");
                }

                // Destroy and forget everything about those useless debug components to avoid overcrowding the scene
                if (debugCameraGO != null)
                {
                    DestroyImmediate(debugCameraGO);
                    debugCameraGO = null;
                    debugCamera = null;
                }
            }
        }
    }

}