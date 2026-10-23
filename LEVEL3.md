# Level 3: implementation and Editor checks

Level 3 reuses Level 2's player, camera, flashlight, UDP receiver, interaction, HUD, materials, and moving-obstacle penalty components. Its enclosed 32 x 28 metre room has a ceiling, three long storage crates, two extra steel dividers, four moving hazards, and four collectibles hidden around furniture. It starts at 40 seconds. Each hazard has a solid collider, one kinematic Rigidbody, and one child trigger configured for a 5-second penalty and 2-second cooldown. Standing in a trigger does not repeatedly deduct time; the existing entry-only logic is unchanged.

The shared Build Profiles scene list now contains LevelSelection, Level1, Level2, Level3, in that order. Level 2's only scene change is its Continue destination, Level3. Level 1 is unchanged. Level 3 has no next scene, so the existing GameManager shows the final victory screen with Play Again and Level Selection. A complete sequential run collects ten objects (3 + 3 + 4).

LevelProgress remains the sole saved-progress owner. Completing an unlocked level unlocks the next one. Level 3's best uses `DarkHunt.BestTime.Level3`; existing unlock and score keys retain their names. Completion time is the starting timer minus remaining time, so collision penalties count toward the score. Losses and unfinished runs do not save a score or unlock levels. The existing HUD reads the Level 3 best automatically.

Older saves keep Level 2 unlocked and retain their scores. Complete Level 2 again to unlock Level 3, even if the new run is slower than the saved best. Opening a locked scene directly in the Editor does not unlock a later level by skipping its prerequisite.

## Test in Unity

1. Let Unity finish importing and compiling. Open LevelSelection and check the three buttons and best labels. Check Build Profiles includes all four enabled scenes, with LevelSelection first.
2. Use RESET PROGRESS, then CANCEL: saved data should remain. Confirm RESET: only Level 1 should be unlocked, all three best labels should read BEST: --, and the UI should update immediately. This deletes only the DarkHunt unlock key and its three best-time keys.
3. Lose or quit Level 1: Level 2 and Level 3 must stay locked. Complete Level 1: Level 2 unlocks, Level 3 stays locked. Check the original 60 seconds, three objects, and stationary obstacles.
4. Complete Level 2: verify its 45 seconds, three objects, moving penalties, and Level Complete screen. Continue must load Level 3. Returning to the menu instead must show Level 3 unlocked and replayable.
5. In Level 3, verify 40 seconds, four objects, dark lighting, head-controlled flashlight, WASD, C recenter, and reliable F prompts/collection. Walk around each crate/divider; all four objects should be accessible. Inspect the four hazard lanes for clear movement.
6. Touch each moving hazard: verify red -5 SECONDS feedback and exactly five seconds deducted. Staying in contact should not repeatedly penalize. Exit and re-enter within two seconds: no extra penalty; re-enter after the cooldown: another penalty. Check the player cannot walk through solid obstacles.
7. Complete Level 3 after a penalty. Its saved time should equal 40 minus the remaining timer, including penalties. Verify YOU ESCAPED, Play Again, and Level Selection. A continuous run from Level 1 should show ten objects; launching Level 3 from the menu should start a fresh four-object run.
8. Replay Level 3 more slowly and then faster: only the faster time should replace its best. Level 1/2 bests must remain unchanged. Lose and quit Level 3: its saved best must remain unchanged.
9. Restart Play mode or the built game: all unlocks and scores should persist. Editor and standalone builds use separate PlayerPrefs stores.
10. Reset again: Level 2/3 must lock and every best must clear. Verify unrelated saved settings remain intact.

Automated validation compiles gameplay scripts against the installed Unity 6000.6 assemblies and exercises the actual progress/GameManager classes with in-memory PlayerPrefs. Scene/reference checks and conservative geometry clearance checks supplement those tests. They do not replace Play mode testing of rendering, trigger callbacks, interaction, or difficulty. No real saved preferences are reset during automated checks.

No Python, head tracking, movement, interaction, shared HUD, penalty scripts, or materials were changed for Level 3. Existing uncommitted visual changes were retained. Nothing was committed or pushed.
