using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingBarController : MonoBehaviour
{
    [SerializeField] private Scrollbar loadingBar;  
    [SerializeField] private float loadingSpeed = 0.5f; 
    [SerializeField] private string sceneToLoad = "Game"; 
    private float targetProgress = 0f;

    void Start()
    {
        if (loadingBar == null)
        {
            Debug.LogError("LoadingBar is not assigned in the Inspector!");
            return;
        }

        // Initialize the loading bar at 0
        loadingBar.value = 0f;
        targetProgress = 0f;

        // Start loading automatically
        StartLoading();
    }

    void Update()
    {
        // Smoothly fill the scrollbar towards the target progress
        if (loadingBar.value < targetProgress)
        {
            loadingBar.value += loadingSpeed * Time.deltaTime;
        }
        else if (loadingBar.value >= 1f)
        {
            // Load the scene when the loading is complete
            LoadGameScene();
        }
    }

    /// <summary>
    /// Start loading with a target progress
    /// </summary>
    public void StartLoading()
    {
        targetProgress = 1f; // Set to 100% target
    }

    /// <summary>
    /// Load the specified scene when loading is complete
    /// </summary>
    private void LoadGameScene()
    {
        Debug.Log("Loading Complete. Loading Scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
