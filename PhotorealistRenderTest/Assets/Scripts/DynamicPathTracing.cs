using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DynamicPathTracing : MonoBehaviour
{
    [Header("Configuration")]
    public Volume globalVolume;
    
    [Header("Detection Settings")]
    public Transform cameraTransform;
    public float stopDelay = 0.3f; 
    public float movementThreshold = 0.005f; 
    public float rotationThreshold = 0.05f; 

    private PathTracing pathTracing;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float stopTimer = 0f;
    private bool isMoving = false;

    void Start()
    {
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
                cameraTransform = transform;
        }

        if (globalVolume != null && globalVolume.profile.TryGet<PathTracing>(out pathTracing))
        {
            lastPosition = cameraTransform.position;
            lastRotation = cameraTransform.rotation;
        }
        else
        {
            Debug.LogError("Volume Global ou composant PathTracing introuvable");
        }
    }

    void Update()
    {
        if (pathTracing == null || cameraTransform == null) return;

        float posDelta = Vector3.Distance(cameraTransform.position, lastPosition);
        float rotDelta = Quaternion.Angle(cameraTransform.rotation, lastRotation);

        if (posDelta > movementThreshold || rotDelta > rotationThreshold)
        {
            isMoving = true;
            stopTimer = 0f;

            if (pathTracing.enable.value)
            {
                pathTracing.enable.value = false;
            }
        }
        else
        {
            if (isMoving)
            {
                stopTimer += Time.deltaTime;
                if (stopTimer >= stopDelay)
                {
                    isMoving = false;
                    pathTracing.enable.value = true;
                }
            }
        }

        lastPosition = cameraTransform.position;
        lastRotation = cameraTransform.rotation;
    }
}
