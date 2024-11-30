using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  
public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;
    [SerializeField] private CameraFollow cameraScript;
    [SerializeField] private Transform player;

 
    [SerializeField] private Canvas mainCanvas;
    private Image[] uiImages;  

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (mainCanvas != null)
        {
            uiImages = mainCanvas.GetComponentsInChildren<Image>(true);
            DontDestroyOnLoad(mainCanvas.gameObject);
        }

        if (player != null)
        {
            DontDestroyOnLoad(player.gameObject);
        }
        if (cameraScript != null)
        {
            DontDestroyOnLoad(cameraScript.gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    
        if (cameraScript != null && player != null)
        {
            cameraScript.SetTarget(player);
        }
        GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");
        if (spawnPoint != null && player != null)
        {
            player.position = spawnPoint.transform.position;
        }
        VerifyUIReferences();
    }

    private void VerifyUIReferences()
    {
        if (mainCanvas == null)
        {
            Debug.LogWarning("Main canvas reference lost during scene transition!");
            return;
        }

        if (uiImages == null || uiImages.Length == 0)
        {
            uiImages = mainCanvas.GetComponentsInChildren<Image>(true);
        }

        foreach (var image in uiImages)
        {
            if (image == null)
            {
                Debug.LogWarning("UI Image reference lost - attempting to reacquire UI references");
                uiImages = mainCanvas.GetComponentsInChildren<Image>(true);
                break;
            }
        }
    }

    public void PrepareForSceneChange()
    {

    }
}