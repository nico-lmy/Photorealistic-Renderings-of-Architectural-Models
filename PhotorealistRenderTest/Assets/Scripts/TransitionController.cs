using UnityEngine;
using System.Collections; 
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class TransitionController : MonoBehaviour
{
    public Volume volume;
    public Camera mainCamera;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Exposure exposureComponent;

    [Header("Settings")]
    public float blackExposure = -10f; 
    public float fadeDuration = 0.15f;
    public float blackHoldDuration = 0.1f;

    [Header("State")]
    public bool isMoving = false; 

    private float defaultExposure;
    private Coroutine activeTransition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (volume.profile.TryGet<Exposure>(out var exposure)) 
        {
            exposureComponent = exposure;
            defaultExposure = exposureComponent.compensation.value; 
            lastPosition = mainCamera.transform.position; 
            lastRotation = mainCamera.transform.rotation; 
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(lastPosition, mainCamera.transform.position);
        float angle = Quaternion.Angle(lastRotation, mainCamera.transform.rotation);
        if (dist > 0.01f || angle > 0.05f) 
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
                activeTransition = null;
                exposureComponent.compensation.value = defaultExposure; 
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
        lastPosition = mainCamera.transform.position;  
        lastRotation = mainCamera.transform.rotation;    
    }

    private IEnumerator TransitionCoroutine()
    {
        float originalExposure = exposureComponent.compensation.value;
        float elapsed = 0f;

        while (elapsed < fadeDuration) 
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            exposureComponent.compensation.value = Mathf.Lerp(originalExposure, blackExposure, t);
            yield return null;
        }
        exposureComponent.compensation.value = blackExposure;
        yield return new WaitForSeconds(blackHoldDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration) 
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t); 
            exposureComponent.compensation.value = Mathf.Lerp(blackExposure, originalExposure, smoothT);
            yield return null;
        }
        exposureComponent.compensation.value = defaultExposure;    
        activeTransition = null;
    }
}
