using UnityEngine;
using UnityEngine.SceneManagement;

// One GameManager per level scene (put it on an empty object called GameManager).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum State { Playing, LevelComplete, GameOver, Won }

    [Header("This level")]
    public string levelTitle = "LEVEL 1";
    public float timeLimit = 60f;

    [Header("Flow (scene names must be in Build Profiles > Scene List)")]
    public string nextScene = "";          // leave EMPTY on the last level
    public string firstScene = "Level1";   // used by PLAY AGAIN

    public State state { get; private set; } = State.Playing;
    public int found { get; private set; }
    public int total { get; private set; }

    static int objectsFoundBefore;   // objects from levels already finished
    float timeLeft;
    float startingTime;
    public float RemainingTime => timeLeft;
    public float CompletionTime => Mathf.Max(0f, startingTime - timeLeft);
    public string BestTimeText => LevelProgress.BestTimeLabel(
        LevelProgress.LevelForScene(SceneManager.GetActiveScene().name));
    public int RunObjectsFound => objectsFoundBefore;
    public float LastPenaltySeconds { get; private set; }
    public float LastPenaltyAt { get; private set; } = float.NegativeInfinity;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        state = State.Playing;
        timeLeft = timeLimit;
        startingTime = timeLimit;

        if (SceneManager.GetActiveScene().name == firstScene)
            objectsFoundBefore = 0;

        total = FindObjectsByType<Collectible>().Length;
        if (total == 0)
            Debug.LogWarning("GameManager: no Collectible objects found in this scene.");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (state != State.Playing) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndLevel(State.GameOver);
        }
    }

    public void ItemCollected()
    {
        if (state != State.Playing) return;

        found++;
        if (found >= total)
            EndLevel(string.IsNullOrEmpty(nextScene) ? State.Won : State.LevelComplete);
    }

    public void ApplyTimePenalty(float seconds)
    {
        if (state != State.Playing) return;
        if (seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;

        LastPenaltySeconds = seconds;
        LastPenaltyAt = Time.unscaledTime;
        timeLeft = Mathf.Max(0f, timeLeft - seconds);

        if (timeLeft <= 0f)
        {
            EndLevel(State.GameOver);
        }
    }

    void EndLevel(State s)
    {
        if (state != State.Playing) return;
        state = s;
        if (s == State.LevelComplete || s == State.Won)
        {
            objectsFoundBefore += found;
            if (total > 0 && found >= total)
                LevelProgress.RecordCompletion(SceneManager.GetActiveScene().name, CompletionTime);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Advance()
    {
        if (state == State.Playing) return;
        Time.timeScale = 1f;
        switch (state)
        {
            case State.LevelComplete:
                SceneManager.LoadScene(nextScene);
                break;
            case State.GameOver:
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case State.Won:
                SceneManager.LoadScene(firstScene);
                break;
        }
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstScene);
    }

    public static void ResetRunProgress() => objectsFoundBefore = 0;

    public void OpenLevelSelection()
    {
        ResetRunProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene(LevelProgress.SelectionScene);
    }
}
