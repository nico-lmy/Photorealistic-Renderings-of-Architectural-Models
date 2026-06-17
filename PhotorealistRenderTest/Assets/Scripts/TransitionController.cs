using UnityEngine;
using System.Collections; 
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class TransitionController : MonoBehaviour
{
    public Volume volume;
    public Camera targetCamera;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Exposure exposureComponent;
    private DepthOfField dofComponent;

    [Header("Settings - Transition")]
    public float fadeDuration = 0.15f;
    public float holdDuration = 0.1f;

    [Header("Settings - Exposure")]
    public float darkExposure = -1.5f; 
    private float defaultExposure;

    [Header("Settings - Blur (Depth of Field)")]
    public float blurFocusDistance = 0.1f; 
    private float defaultFocusDistance = 10f; 
    
    [Header("State")]
    public bool isMoving = false; 
    private Coroutine activeTransition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastPosition = targetCamera.transform.position; 
        lastRotation = targetCamera.transform.rotation;

        if (volume.profile.TryGet<Exposure>(out var exposure)) 
        {
            exposureComponent = exposure;
            defaultExposure = exposureComponent.compensation.value;  
        }

        if (volume.profile.TryGet<DepthOfField>(out var dof))
        {
            dofComponent = dof;
            defaultFocusDistance = dofComponent.focusDistance.value; 
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (targetCamera == null) return; 

        float dist = Vector3.Distance(lastPosition, targetCamera.transform.position);
        float angle = Quaternion.Angle(lastRotation, targetCamera.transform.rotation);

        if (dist > 0.01f || angle > 0.05f) 
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
                activeTransition = null;
                ResetVisuals();  
            }
            isMoving = true;
        } 
        else 
        {
            if (isMoving)
            {
                activeTransition = StartCoroutine(TransitionCoroutine());
                isMoving = false;
            }
        } 
        lastPosition = targetCamera.transform.position;  
        lastRotation = targetCamera.transform.rotation;    
    }

    private void ResetVisuals()
    {
        if (exposureComponent != null) exposureComponent.compensation.value = defaultExposure;
        if (dofComponent != null) dofComponent.focusDistance.value = defaultFocusDistance;
    }

    private IEnumerator TransitionCoroutine()
    {
        float startExposure = exposureComponent != null ? exposureComponent.compensation.value : defaultExposure;
        float startFocus = dofComponent != null ? dofComponent.focusDistance.value : defaultFocusDistance;
        float elapsed = 0f;

        while (elapsed < fadeDuration) 
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            if (exposureComponent != null) 
                exposureComponent.compensation.value = Mathf.Lerp(startExposure, darkExposure, t);
            if (dofComponent != null)
                dofComponent.focusDistance.value = Mathf.Lerp(startFocus, blurFocusDistance, t); 
            yield return null;
        }
        if (exposureComponent != null) exposureComponent.compensation.value = darkExposure;
        if (dofComponent != null) dofComponent.focusDistance.value = blurFocusDistance;
        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration) 
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            if (exposureComponent != null) 
                exposureComponent.compensation.value = Mathf.Lerp(darkExposure, defaultExposure, t);
            if (dofComponent != null)
                dofComponent.focusDistance.value = Mathf.Lerp(blurFocusDistance, defaultFocusDistance, t); 
            yield return null;
        }
        ResetVisuals(); 
        activeTransition = null;
    }
}
