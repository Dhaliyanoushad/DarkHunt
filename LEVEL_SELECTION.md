# Level selection, best times, and reset progress

Open `Assets/Scenes/LevelSelection.unity` for Editor testing. Builds start there automatically: the shared Build Profiles list is **LevelSelection, Level1, Level2**, all enabled.

Level 1 is always available. Completing Level 1 unlocks Level 2. Unlocked levels can be replayed, and losing, retrying, or quitting does not remove unlocks or save a new score. Existing saved unlocks remain compatible with this update; a previously unlocked level with no recorded score displays `BEST: --`.

## Saved data and scoring

`Assets/Scripts/LevelProgress.cs` is the single owner of these PlayerPrefs keys:

- `DarkHunt.HighestUnlockedLevel`
- `DarkHunt.BestTime.Level1`
- `DarkHunt.BestTime.Level2`

A level's score is its captured starting timer minus its remaining timer at successful completion. Lower is better. For example, Level 2 starts at 45 seconds: seven seconds of play plus a five-second collision penalty leaves 33 seconds, so the recorded completion time is **12 seconds** (`45 - 33`).

Only the successful completion path records scores. The first valid completion establishes a best; later completions replace it only if faster. Equal/slower wins do not overwrite the score. Each level has an independent floating-point best time, displayed in seconds to two decimal places. PlayerPrefs.Save is called immediately when an unlock or score changes. No external database, packages, or account are needed.

The selection menu displays each best below its level button. During gameplay, the current level's best appears under the collectible counter at top-right. An uncompleted level shows `BEST: --`.

Continue, Retry, Play Again, Restart From Level 1, and Level Selection still work as before. Continue retains the run's object total, so finishing both rooms in order shows six objects. Selecting a room from the menu starts a fresh run count without changing permanent progress.

## Test saving and high scores in Unity

1. Open LevelSelection. Press Play. Confirm each level has a BEST label, and only Level 1 is available on fresh progress. If you need a fresh state, use the confirmed reset described below.
2. Play Level 1. Confirm 60 seconds, three collectibles, stationary obstacles, WASD, head tracking, flashlight, and collection prompts still work. The HUD should initially show `BEST: --`.
3. Collect all three objects. On Level Complete, press LEVEL SELECTION: Level 2 should be unlocked and Level 1 should display its completion time.
4. Replay Level 1 and finish more slowly. The best must stay unchanged. Replay faster: the lower time must replace it. In the HUD, the existing best should appear before you finish the replay.
5. Finish Level 2 after taking a collision penalty. Check its recorded best against `45 - remaining time` on completion; each five-second collision penalty must count toward that time. Scores are rounded only for display, so a visual stopwatch comparison may differ by a few hundredths.
6. Check that improving Level 2 never changes Level 1's score. Equal/slower penalized runs must leave Level 2's best unchanged.
7. Lose a run with a saved best, and quit another unfinished run. Return to the menu: neither should overwrite any best. After a reset, losing/quitting Level 1 must neither create a best nor unlock Level 2.
8. Stop/restart Play mode or close/reopen the game. Confirm unlocks and both best times persist. Editor and standalone PlayerPrefs stores are separate: test each environment's persistence separately.
9. Test the existing CONTINUE route through both levels and verify the final count is still six. Test all result-screen navigation buttons.

## Test reset and cancellation

1. From Level Selection, click **RESET PROGRESS**. A confirmation panel must appear; level buttons must not be usable while it is open.
2. Choose **CANCEL**. All unlocks and best times must remain unchanged.
3. Open RESET PROGRESS again and choose **RESET**. Both best labels must immediately become `BEST: --`, Level 1 must remain playable, and Level 2 must read LOCKED. No scene reload is required.
4. Restart the game/Play session and verify the reset persisted. Complete Level 1 again to check unlocking and score creation after reset.

Only the three listed keys are deleted. Other PlayerPrefs keys, including unrelated DarkHunt settings, remain intact. The older Inspector-only testing reset has been replaced by this player-facing confirmation flow. No actual saved progress was reset during automated checks.

## Files changed for high scores and reset

- `Assets/Scripts/LevelProgress.cs`: per-level best-time keys, reading/formatting scores, success-only recording, and targeted reset of unlocks plus scores.
- `Assets/Scripts/GameManager.cs`: captures the starting timer and passes elapsed timer time to the progress manager only after successful collection of all objects.
- `Assets/Scripts/DarkHuntHUD.cs`: current-level BEST label beneath the top-right counter.
- `Assets/Scripts/LevelSelectionMenu.cs`: best labels, RESET PROGRESS button, modal confirmation, cancellation, and immediate refresh through live progress reads.

No scenes, physics, movement, head tracking, Python, or build settings were changed for this score/reset update. Earlier uncommitted work is retained.

All gameplay scripts compile against Unity 6000.6.0f1 without errors or warnings. Twenty-nine logic checks exercised the actual progress and GameManager classes with in-memory Unity/PlayerPrefs test doubles: first/faster/slower/equal scores, separate levels, penalties, losses, quits, Continue, unlock retention, reset isolation, invalid saved values, and display formatting. Scene YAML, references, component settings, and menu/HUD bounds were checked statically. Actual UI clicks, confirmation interaction, and on-disk persistence across a real Unity/game restart still require the Editor/build tests above.

No changes were committed or pushed.
