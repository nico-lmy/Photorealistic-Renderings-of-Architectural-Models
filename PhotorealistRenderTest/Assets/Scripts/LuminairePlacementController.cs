using UnityEngine;

public class LuminairePlacementController : MonoBehaviour
{
    public LuminaireCatalog catalog;
    public LayerMask placementLayerMask = ~0;

    [Header("Camera references")]
    public Camera simpleCam;
    public Camera caveCenterCam;

    [Header("Preview marker")]
    public GameObject previewMarkerPrefab;
    private GameObject markerInstance;
    public float surfaceOffset = 0.05f;

    [Header("UI reference")]
    public RuntimeUIController uiController;

    [Header("Hierarchy organization")]
    public Transform lightsContainer;

    public bool IsPlacing => isPlacing;
    public LuminaireProfile CurrentProfile => selectedProfile;

    private Camera ActiveReferenceCamera
    {
        get
        {
            if (simpleCam != null && simpleCam.gameObject.activeInHierarchy) return simpleCam;
            if (caveCenterCam != null && caveCenterCam.gameObject.activeInHierarchy) return caveCenterCam;
            return null;
        }
    }

    private GameObject previewInstance;
    private LuminaireProfile selectedProfile;
    private bool isPlacing = false;

    void Update()
    {
        if (!isPlacing || selectedProfile == null) return;
        if (uiController != null && uiController.IsPanelOpen)
        {
            if (previewInstance != null) previewInstance.SetActive(false);
            return;
        }
        else if (previewInstance != null && !previewInstance.activeSelf) previewInstance.SetActive(true);

        Camera cam = ActiveReferenceCamera;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayerMask))
        {
            if (previewInstance == null)
            {
                Transform parent = lightsContainer != null ? lightsContainer : null;
                previewInstance = Instantiate(selectedProfile.lightPrefab, parent);
                previewInstance.name = selectedProfile.luminaireName + " (Preview)";
                if (previewMarkerPrefab != null)
                {
                    markerInstance = Instantiate(previewMarkerPrefab, previewInstance.transform);
                    markerInstance.transform.localPosition = Vector3.zero;
                }
            }
            previewInstance.transform.position = hit.point + hit.normal * surfaceOffset;
            previewInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)) ConfirmPlacement();
        else if (Input.GetKeyDown(KeyCode.Escape)) CancelPlacement();
    }

    public void StartPlacement(LuminaireProfile profile)
    {
        CancelPlacement();
        selectedProfile = profile;
        isPlacing = true;

        if (uiController != null) uiController.ClosePanel();
    }


    public void CancelPlacementPublic() => CancelPlacement();

    void ConfirmPlacement()
    {
        if (markerInstance != null) Destroy(markerInstance);
        previewInstance.name = selectedProfile.luminaireName;
        PlacedLuminaireRegistry.Instance.Register(previewInstance);
        previewInstance = null;
        markerInstance = null;
        isPlacing = false;
        selectedProfile = null;
    }

    void CancelPlacement()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;
        markerInstance = null;
        isPlacing = false;
        selectedProfile = null;
    }
}