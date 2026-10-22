using UnityEngine;

// The only owner of saved unlocks and best times. Navigation never writes progress.
public static class LevelProgress
{
    public const string SaveKey = "DarkHunt.HighestUnlockedLevel";
    public const string SelectionScene = "LevelSelection";
    public const int LevelCount = 2;

    public static int HighestUnlockedLevel =>
        Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 1), 1, LevelCount);

    public static bool IsUnlocked(int level) =>
        level >= 1 && level <= LevelCount && level <= HighestUnlockedLevel;

    public static string SceneForLevel(int level)
    {
        if (level < 1 || level > LevelCount)
            throw new System.ArgumentOutOfRangeException(nameof(level));
        return "Level" + level;
    }

    public static int LevelForScene(string sceneName)
    {
        for (int level = 1; level <= LevelCount; level++)
            if (sceneName == SceneForLevel(level)) return level;
        return 0;
    }

    public static string BestTimeKey(int level) => "DarkHunt.BestTime." + SceneForLevel(level);

    public static bool TryGetBestTime(int level, out float seconds)
    {
        seconds = 0f;
        if (level < 1 || level > LevelCount || !PlayerPrefs.HasKey(BestTimeKey(level))) return false;
        float saved = PlayerPrefs.GetFloat(BestTimeKey(level));
        if (saved < 0f || float.IsNaN(saved) || float.IsInfinity(saved)) return false;
        seconds = saved;
        return true;
    }

    public static string BestTimeLabel(int level)
    {
        return TryGetBestTime(level, out float seconds)
            ? "BEST: " + seconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s"
            : "BEST: --";
    }

    // Called exclusively from GameManager's successful completion path.
    public static void RecordCompletion(string sceneName, float completionSeconds)
    {
        int level = LevelForScene(sceneName);
        if (level == 0 || completionSeconds < 0f || float.IsNaN(completionSeconds) ||
            float.IsInfinity(completionSeconds)) return;

        bool changed = false;
        if (level == 1 && !IsUnlocked(2))
        {
            PlayerPrefs.SetInt(SaveKey, 2);
            changed = true;
        }
        if (!TryGetBestTime(level, out float best) || completionSeconds < best)
        {
            PlayerPrefs.SetFloat(BestTimeKey(level), completionSeconds);
            changed = true;
        }
        if (changed) PlayerPrefs.Save();
    }

    // Called after reset confirmation. Never delete unrelated preferences.
    public static void ResetSavedProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        for (int level = 1; level <= LevelCount; level++)
            PlayerPrefs.DeleteKey(BestTimeKey(level));
        PlayerPrefs.Save();
    }
}
