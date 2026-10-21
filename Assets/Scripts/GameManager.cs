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
    Texture2D dim;
    GUIStyle hud, prompt, big, mid, btn;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        state = State.Playing;
        timeLeft = timeLimit;

        if (SceneManager.GetActiveScene().name == firstScene)
            objectsFoundBefore = 0;

        total = FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;
        if (total == 0)
            Debug.LogWarning("GameManager: no Collectible objects found in this scene.");

        dim = new Texture2D(1, 1);
        dim.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
        dim.Apply();
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

    timeLeft = Mathf.Max(0f, timeLeft - seconds);

    if (timeLeft <= 0f)
    {
        EndLevel(State.GameOver);
    }
}

    void EndLevel(State s)
    {
        state = s;
        if (s != State.GameOver) objectsFoundBefore += found;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnButton()
    {
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

    // ---------- on-screen text (no Canvas needed) ----------

    void BuildStyles()
    {
        int s = Mathf.Max(14, Screen.height / 30);

        hud = new GUIStyle(GUI.skin.label)
        { fontSize = s, alignment = TextAnchor.UpperRight, fontStyle = FontStyle.Bold };
        prompt = new GUIStyle(GUI.skin.label)
        { fontSize = s, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        big = new GUIStyle(GUI.skin.label)
        { fontSize = s * 3, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        mid = new GUIStyle(GUI.skin.label)
        { fontSize = Mathf.RoundToInt(s * 1.3f), alignment = TextAnchor.MiddleCenter };
        btn = new GUIStyle(GUI.skin.button)
        { fontSize = Mathf.RoundToInt(s * 1.3f), fontStyle = FontStyle.Bold };

        hud.normal.textColor = Color.white;
        prompt.normal.textColor = Color.white;
        big.normal.textColor = Color.white;
        mid.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        if (hud == null) BuildStyles();

        float w = Screen.width;
        float h = Screen.height;

        if (state == State.Playing)
        {
            hud.normal.textColor = timeLeft <= 10f ? Color.red : Color.white;
            GUI.Label(new Rect(w - 430f, 10f, 420f, h * 0.2f),
                $"{levelTitle}\nTIME: {Mathf.CeilToInt(timeLeft)}\nOBJECTS: {found} / {total}", hud);

            if (PlayerInteract.Looking != null)
                GUI.Label(new Rect(0f, h * 0.62f, w, h * 0.08f),
                    $"Press {PlayerInteract.PickupKey} to pick up", prompt);
            return;
        }

        GUI.DrawTexture(new Rect(0f, 0f, w, h), dim);

        string title, sub, label;
        switch (state)
        {
            case State.LevelComplete:
                title = $"{levelTitle} COMPLETE";
                sub = $"Objects found: {found} / {total}";
                label = "CONTINUE";
                break;
            case State.GameOver:
                title = "TIME'S UP";
                sub = $"GAME OVER   ({found} / {total} found)";
                label = "RETRY";
                break;
            default:
                title = "YOU ESCAPED";
                sub = $"OBJECTS FOUND: {objectsFoundBefore}";
                label = "PLAY AGAIN";
                break;
        }

        GUI.Label(new Rect(0f, h * 0.22f, w, h * 0.15f), title, big);
        GUI.Label(new Rect(0f, h * 0.38f, w, h * 0.08f), sub, mid);

        if (GUI.Button(new Rect(w * 0.5f - w * 0.12f, h * 0.55f, w * 0.24f, h * 0.09f), label, btn))
            OnButton();
    }
}
