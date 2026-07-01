using UnityEngine;
using UnityEngine.Rendering;

namespace Stereolab.StereoProjection
{
    /// <summary>
    /// Plane limited in space that will bind a camera's projection (through <see cref="ProjectionPlaneCamera"/>). It basicaly emulates a screen in space.
    /// </summary>
    [ExecuteInEditMode]
    public class ProjectionPlane : MonoBehaviour
    {
        /// <summary>
        /// Index of the display on which render this screen
        /// </summary>
        [SerializeField]
        [Tooltip("Index of the display on which render this screen.")]
        public uint displayIndex = 0;

        [Header("Size")]

        /// <summary>
        /// Size of the plane in meters (should match the size of the simulated monitor).
        /// </summary>
        [SerializeField]
        [Tooltip("Size of the screen in meters (width x height).")]
        public Vector2 size = new Vector2(1,1);
        private Vector2 previousSize = new Vector2(1,1);

        /// <summary>
        /// Aspect ratio of the display.
        /// </summary>
        /// <remarks>
        /// The off-axis projection will only look correct if the matches the one of the monitor.
        /// </remarks>
        [SerializeField]
        [Tooltip("Aspect ratio of the simulated monitor.")]
        public Vector2 aspectRatio = new Vector2(16, 9);
        private Vector2 previousAspectRatio = new Vector2(16, 9);

        /// <summary>
        /// Forceful change the size to match with the aspect ratio.
        /// </summary>
        [SerializeField]
        [Tooltip("Forceful change the size to match with the aspect ratio.")]
        public bool lockAspectRatio = true;

        /// <summary>
        /// Explicit, should match the resolution of the screen we're simulating.
        /// </summary>
        [SerializeField]
        [Tooltip("Resolution of the monitor that the display will render to.")]
        private Vector2Int resolution = new Vector2Int(1920, 1080);
        
        private Vector2Int previousResolution = new Vector2Int(1920, 1080);

        [Header("Helpers")]
        /// <summary>
        /// To draw the gizmos or not to.
        /// </summary>
        [SerializeField]
        [Tooltip("Activate the debug gizmos for the screen.")]
        private bool drawGizmos = false;

        /// <summary>
        /// Color of the gizmos of the plane. Controls also the colors of the camera's gizmos projection to this plane.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of the debug gizmo. Controls also the color of the gizmos of the cameras aiming to this screen.")]
        private Color _screenGizmoColor = Color.grey;

        public Color screenGizmoColor { get => _screenGizmoColor; }

        /// <summary>
        /// Define if we create the debug quad/texture to map a camera output on it.
        /// </summary>
        [SerializeField]
        [Tooltip("Define if we create the debug quad/texture to map a camera output on it.")]
        private bool _drawDebugTexture = false;

        /// <summary>
        /// Define if we create the debug quad and texture or not. Public read-only accessor for <see cref="_drawDebugTexture"/>.
        /// </summary>
        public bool drawDebugTexture { get => _drawDebugTexture; }

        private RenderTexture _debugTexture;
        /// <summary>
        /// Texture allowing to render the projection and emulating a real-life monitor in the scene.
        /// </summary>
        /// <remarks>
        /// Returns null if <see cref="drawDebugTexture"/> to avoid any destruction of used stuff that would create a nasty error message.
        /// </remarks>   
        public RenderTexture debugTexture { 
            get
            {
                return drawDebugTexture ? _debugTexture : null;
            } 
            private set
            {
                _debugTexture = value;
            } 
        }
        
        /// <summary>
        /// Holds the mesh to display <see cref="debugTexture"/>. Destroyed when not used anymore.
        /// </summary>
        private MeshFilter debugFilter = null;

        /// <summary>
        /// Renderer created to display <see cref="debugTexture"/>. Destroyed when not used anymore.
        /// </summary>
        private MeshRenderer debugRenderer = null;

        // private static bool is


        # region Screen corners
        /// <summary>
        /// Bottom-left corner of the screen in world space.
        /// </summary>
        public Vector3 bottomLeft { get; private set; }
        
        /// <summary>
        /// Bottom-right corner of the screen in world space.
        /// </summary>
        public Vector3 bottomRight { get; private set; }
        
        /// <summary>
        /// Top-left corner of the screen in world space.
        /// </summary>
        public Vector3 topLeft { get; private set; }
        
        /// <summary>
        /// Top-right corner of the screen in world space.
        /// </summary>
        public Vector3 topRight { get; private set; }
        # endregion

        # region Component vectors of the plane
        /// <summary>
        /// Up-vector component of the plane, normalized, in world space.
        /// </summary>
        public Vector3 directionUp { get; private set; }
        
        /// <summary>
        /// Normal-vector component of the plane, normalized, in world space.
        /// </summary>
        public Vector3 directionNormal { get; private set; }
        
        /// <summary>
        /// Right-vector component of the plane, normalized, in world space.
        /// </summary>
        public Vector3 directionRight { get; private set; }

        private Matrix4x4 _worldToPlane;
        public Matrix4x4 worldToPlane { get => _worldToPlane; }
        # endregion

        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                // Draw the borders of the plane
                Gizmos.color = screenGizmoColor;
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomLeft, topLeft);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(topLeft, topRight);

                // Draw the direction towards eye
                Vector3 planeCenter = bottomLeft + ((topRight - bottomLeft) * 0.5f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(planeCenter, planeCenter + directionNormal* 0.2f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(planeCenter, planeCenter + directionUp * 0.2f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(planeCenter, planeCenter + directionRight * 0.2f);
            }
        }

        private void Awake()
        {
            # if !UNITY_EDITOR
            Display.displays[displayIndex].Activate(resolution.x, resolution.y, 120);
            # endif
        }

        private void Start()
        {
            // Don't do the debug rendering when the application is playing
            // Thus, we preemptively destroy those to avoid overcrowing the scene/costing perfs 
            if (Application.isPlaying)
            {
                if (TryGetComponent<MeshFilter>(out debugFilter))
                {
                    DestroyImmediate(debugFilter);
                    debugFilter = null;

                }
                if (TryGetComponent<MeshRenderer>(out debugRenderer))
                {
                    DestroyImmediate(debugRenderer);
                    debugRenderer = null;
                }
            }
        }

        private void Update()
        {
            AdaptSizeAndAspectRatio();

            ComputePlaneComponents();

            if (!Application.isPlaying)
            {
                DrawDebugTexture();
            }
        }

        /// <summary>
        /// Takes care of the mesh, filter, render texture and mesh for the screen.
        /// </summary>
        private void DrawDebugTexture()
        {
            if (drawDebugTexture)
            {
                // get the MeshFilter or create a new one
                if (debugFilter == null && !TryGetComponent<MeshFilter>(out debugFilter))
                {
                    debugFilter = gameObject.AddComponent<MeshFilter>();
                }

                // Create a simple quad mesh mapped to the geometry of the screen
                Mesh debugMesh = new Mesh();
                debugFilter.sharedMesh = debugMesh;
                debugFilter.sharedMesh.Clear();

                Vector3 normalOffset = directionNormal * 0.001f;
                debugMesh.vertices = new Vector3[]
                {
                    new Vector3(-size.x, -size.y) * 0.5f - normalOffset,
                    new Vector3(-size.x, size.y) * 0.5f - normalOffset,
                    new Vector3(size.x, size.y) * 0.5f - normalOffset,
                    new Vector3(size.x, -size.y) * 0.5f - normalOffset
                };
                debugMesh.normals = new Vector3[]
                {
                    directionNormal,
                    directionNormal,
                    directionNormal,
                    directionNormal
                };
                debugMesh.uv = new Vector2[]
                {
                    Vector2.zero,
                    Vector2.up,
                    Vector2.one,
                    Vector2.right
                };
                debugMesh.triangles = new int[] {0, 1, 2, 0, 2, 3};

                // Get the MeshRenderer component or create a new one
                if (debugRenderer == null && !TryGetComponent<MeshRenderer>(out debugRenderer))
                {
                    debugRenderer = gameObject.AddComponent<MeshRenderer>();
                    debugRenderer.shadowCastingMode = ShadowCastingMode.Off;
                    debugRenderer.receiveShadows = false;
                    debugRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
                }

                // Prevent any resolution lesser or equal to 0
                if (resolution.x <= 0)
                {
                    resolution.x = 1;
                }
                if (resolution.y <= 0)
                {
                    resolution.y = 1;
                }

                // Create a new RenderTexture if the last one doesn't exist anymore or if the resolution has changed
                if ((debugTexture == null) || (resolution != previousResolution))
                {
                    debugTexture = new RenderTexture(resolution.x, resolution.y, 16);
                    previousResolution = resolution;
                }
                debugRenderer.sharedMaterial.mainTexture = debugTexture;
            }
            else 
            {
                // Get rid of the debugTexture related stuff to avoid overcrowing the scene/wild edits of temporary stuff by the unaware user
                if (debugTexture != null)
                {
                    DestroyImmediate(debugTexture);
                    debugTexture = null;
                }
                if (debugFilter != null)
                {
                    DestroyImmediate(debugFilter);
                    debugFilter = null;

                }
                if (debugRenderer != null)
                {
                    DestroyImmediate(debugRenderer);
                    debugRenderer = null;
                }
            }
        }

        /// <summary>
        /// Manage the aspect ratio locking and clamp the size to void matrices hiccups.
        /// </summary>
        private void AdaptSizeAndAspectRatio()
        {
            // Adapt the size of the plane to correspond to the aspect ratio
            if(lockAspectRatio)
            {
                if (aspectRatio.x != previousAspectRatio.x) 
                {
                    size.y = size.x / aspectRatio.x * aspectRatio.y;

                    // If edited on X, do not do the Y
                    previousAspectRatio.y = aspectRatio.y;
                }

                if (aspectRatio.y != previousAspectRatio.y)
                {
                    size.x = size.y / aspectRatio.y * aspectRatio.x;
                }

                if (size.x != previousSize.x)
                {
                    size.y = size.x / aspectRatio.x * aspectRatio.y;
                    previousSize.y = size.y;
                }

                if (size.y != previousSize.y)
                {
                    size.x = size.y / aspectRatio.y * aspectRatio.x;
                }
            }

            // Lock the size and aspect ratio values to avoid matrices failures
            // We consider a minimal screen size of 5cm by 5cm, which would be a very small phone
            size.x = Mathf.Max(0.05f, size.x);
            size.y = Mathf.Max(0.05f, size.y);
            aspectRatio.x = Mathf.Max(1, aspectRatio.x);
            aspectRatio.y = Mathf.Max(1, aspectRatio.y);

            previousSize = size;
            previousAspectRatio = aspectRatio;
        }

        /// <summary>
        /// Compute the corners of the screen and component vectors of its plane.
        /// </summary>
        private void ComputePlaneComponents()
        {
            // Recompute the corners and components vectors of the plane (since we changed the size)
            bottomLeft = transform.TransformPoint(
                new Vector3(-size.x, -size.y) * 0.5f
            );
            bottomRight = transform.TransformPoint(
                new Vector3(size.x, -size.y) * 0.5f
            );
            topLeft = transform.TransformPoint(
                new Vector3(-size.x, size.y) * 0.5f
            );
            topRight = transform.TransformPoint(
                new Vector3(size.x, size.y) * 0.5f
            );

            directionRight = (topRight + bottomRight - 2*transform.position).normalized;
            directionUp = (topRight + topLeft - 2*transform.position).normalized;
            directionNormal = -Vector3.Cross(directionRight, directionUp).normalized;

            // Transformation matrix
            _worldToPlane = Matrix4x4.zero;
            _worldToPlane[0, 0] = directionRight.x;
            _worldToPlane[0, 1] = directionRight.y;
            _worldToPlane[0, 2] = directionRight.z;

            _worldToPlane[1, 0] = directionUp.x;
            _worldToPlane[1, 1] = directionUp.y;
            _worldToPlane[1, 2] = directionUp.z;

            _worldToPlane[2, 0] = directionNormal.x;
            _worldToPlane[2, 1] = directionNormal.y;
            _worldToPlane[2, 2] = directionNormal.z;

            _worldToPlane[3, 3] = 1.0f;
        }
    }
}
