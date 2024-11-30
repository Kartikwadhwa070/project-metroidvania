using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    private static PersistentPlayer instance;

    void Awake()
    {
        // If an instance already exists, destroy this one
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Make this the singleton instance
        instance = this;

        // Prevent this object from being destroyed on scene load
        DontDestroyOnLoad(gameObject);
    }
}

// Camera follow script that persists between scenes
public class PersistentCameraFollow : MonoBehaviour
{
    private static PersistentCameraFollow instance;
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform target;

    void Awake()
    {
        // If an instance already exists, destroy this one
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Make this the singleton instance
        instance = this;

        // Prevent the camera from being destroyed
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // Method to set new target if needed after scene load
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}