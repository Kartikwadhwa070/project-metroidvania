using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // Default offset for perspective view
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private float fieldOfView = 60f;  // Standard perspective FOV
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform target;
    private Camera cam;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = false;  // Ensure perspective mode
            cam.fieldOfView = fieldOfView;
        }

        // Set initial position
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    void Start()
    {
        // Double check perspective settings
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;  // Standard perspective camera setting
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    // Optional: Method to adjust FOV during runtime if needed
    public void SetFieldOfView(float newFOV)
    {
        fieldOfView = newFOV;
        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }
    }
}