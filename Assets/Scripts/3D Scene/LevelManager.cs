using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Error & Loss Settings")]
    [SerializeField] private int maxErrors = 3;
    public int currentErrors { get; private set; } = 0;
    public double EndTime { get; private set; }
    
    private float startTime;
    private float finalTime;
    private bool isGameEnded = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private GameObject gameOverScreenPanel;
    
    [Header("UI Text Displays (Optional)")]
    [SerializeField] private TMP_Text winStatsText;
    [SerializeField] private TMP_Text gameOverStatsText;

    private void Awake() 
    { 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; 
    }

    private void Start()
    {
        startTime = Time.time;
        currentErrors = 0;
        isGameEnded = false;

        if (winScreenPanel != null) winScreenPanel.SetActive(false);
        if (gameOverScreenPanel != null) gameOverScreenPanel.SetActive(false);
    }


public void LogMistake(string errorDescription)
{
    if (isGameEnded) return;

    currentErrors++;
    Debug.LogWarning($"Safety Mistake Logged: {errorDescription}. Errors: {currentErrors}/{maxErrors}");

    if (UINotificationManager.Instance != null)
    {
        UINotificationManager.Instance.ShowMessage(errorDescription);
    }

    if (currentErrors >= maxErrors)
    {
        TriggerGameOver(errorDescription);
    }
}


    public void CompleteLevel()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        double finalTime = Time.time - startTime;
        EndTime = finalTime;
        Debug.Log($"Level Complete! Time: {finalTime:F2} seconds. Errors: {currentErrors}");

        if (winScreenPanel != null)
            winScreenPanel.SetActive(true);

        if (winStatsText != null)
        {
            winStatsText.text = $"Completion Time: {finalTime:F1}s\nErrors Made: {currentErrors} / {maxErrors}";
        }

        UIState.Push();
        Time.timeScale = 0f;
    }

    private void TriggerGameOver(string lastErrorReason)
    {
        if (isGameEnded) return;
        isGameEnded = true;

        finalTime = Time.time - startTime;
        Debug.Log("Game Over! Maximum safety errors reached.");

        if (gameOverScreenPanel != null)
            gameOverScreenPanel.SetActive(true);

        if (gameOverStatsText != null)
        {
            gameOverStatsText.text = $"Failure Reason: {lastErrorReason}\nTotal Errors: {currentErrors} / {maxErrors}\nTime Elapsed: {finalTime:F1}s";
        }

        UIState.Push();
        Time.timeScale = 0f; 
    }


    public void RestartCurrentScene()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}