# DarkHunt room and HUD update

Both scenes now use a 28 x 24 metre enclosed room with a 4.8 metre ceiling. The existing player, camera, flashlight, UDP receiver, collectibles, managers, and obstacle objects were retained. New architectural objects are grouped under `EscapeRoom_Architecture`.

The rooms retain the enclosed shell, simple shelving, columns, and longer reinforced crates. Decorative floor slabs, wall seams, labels, vents, service lines, and glowing aisle guides have been removed. Matte charcoal, dark steel, and muted timber materials keep the rooms simple. The exit door is scenery; collecting the three objects still completes the level.

The flashlight is now the only active scene light; emergency point lights are disabled. Ambient and environment reflections are suppressed, and collectible/guide materials no longer emit light. The continuous floor is now the visible walking surface. Every collider, CharacterController, and Rigidbody definition was preserved during the simplification, along with obstacle movement paths and collectible positions.

## Files

- `Assets/Scenes/Level1.unity`: room geometry, materials, spawn, object positions, lighting, and one HUD component. Timer remains 60 seconds; obstacles remain stationary.
- `Assets/Scenes/Level2.unity`: matching architecture, subtle lighting, longer obstacles, adjusted motion paths, and trigger margins of 0.14 metres on each side. Timer remains 45 seconds; exactly three moving obstacles and three penalty triggers remain.
- `Assets/Scripts/GameManager.cs`: old HUD removed; exposes time, run count, and penalty feedback to the new HUD. Provides Continue/Retry/Play Again and Restart Run actions. Preserves scene transitions and cumulative object count.
- `Assets/Scripts/DarkHuntHUD.cs` and `.meta`: one IMGUI HUD per scene, designed at 1920 x 1080. Shows level at top-left, timer at top-centre, and collectibles plus the current level best time at top-right without a background strip. Saved scores and reset behaviour are documented in LEVEL_SELECTION.md. Text uses a small shadow for readability. Positions anchor directly to the Game view edges and centre without UI scaling or animations. Result screens are centred, and the prompt has only a small translucent background. No Canvas, EventSystem, or UI package is needed.
- `Assets/Scripts/MovingObstacle.cs`: retains the existing physics movement changes and corrects the invalid `Time.fixedTimeSinceLevelLoad` API to `Time.timeSinceLevelLoad` inside `FixedUpdate`.
- `Assets/Scripts/ObstaclePenalty.cs`: earlier uncommitted changes retained: entry-only penalty, two-second per-obstacle cooldown, and gameplay-state check. Staying inside does not repeatedly deduct time. Different obstacles have independent cooldowns.
- New materials and their `.meta` files in `Assets/Materials`: `Concrete`, `FloorSlate`, `FloorSlateAlt`, `Ceiling`, `Steel`, `Timber`, `Hazard`, `GuideMint`, and `GuideAmber`.
- `Assets/Materials/ConcreteGrain.tga` and `.meta`: earlier locally generated grain asset retained on disk; the simplified matte materials no longer use it. No third-party assets or packages added.

UDP packet debug text is disabled on the scene receivers so it does not overlap the new HUD. The UDP code, port 5005, webcam Python, head tracking, WASD movement, flashlight control, and head-controlled flashlight remain unchanged. Interaction now runs in LateUpdate using an explicit camera reference, a 12-degree aiming tolerance with a small retention margin, a three-metre collider-surface range, and a line-of-sight raycast that ignores the player and triggers. Pressing F uses the displayed target, rechecking range and visibility; collection is guarded against duplicate counts.

## Verification performed

All gameplay C# scripts compiled together with the installed Unity 6000.6.0f1 compiler and reference assemblies, with no errors or warnings. Both scenes and the materials parsed as YAML. Checks covered unique object IDs and asset GUIDs, local references, component ownership, hierarchy, scene roots, referenced scripts, timers, collectible/mover/trigger counts, and kinematic Rigidbody configuration. A conservative grid clearance check found routes from spawn to all three collectibles in each room, treating the entire moving-obstacle sweep as blocked. HUD rectangles also remain in bounds without overlapping at 640 x 480, 960 x 540, 1280 x 720, and 1920 x 1080. These checks are not a gameplay or visual test.

## Test in Unity

1. Open the project with Unity 6000.6.0f1. Allow asset import and compilation to finish. If Unity has an older scene open, reload it from disk; discard only stale in-memory scene data, not newer work you intend to keep.
2. Open `Assets/Scenes/Level1.unity`. Set Game view to **1920 x 1080**. Check the Console for import, shader, or script errors.
3. Press Play. Confirm 60 seconds, `00 / 03`, one HUD, enclosed architecture, and a flashlight-lit room. The three main obstacles must stay still. Walk past shelving and pillars to check clearance.
4. Run your existing Python head tracker as usual. Click back into Game view, verify WASD and head aim, and use the existing calibration controls. Receiver `Show Debug` can be temporarily enabled if you need packet diagnostics.
5. Aim near an orb within three metres: the reticle changes colour and `PRESS F TO COLLECT` appears. Press F immediately when the prompt appears, including while making small head movements. Confirm a single count increment and immediate prompt clearing. Move outside three metres and look through a solid crate or wall: there should be no prompt or pickup. Recover all three and test **CONTINUE**.
6. In Level 2, confirm 45 seconds and three moving obstacles. Touch each obstacle: the timer should lose five seconds and red `-5 SECONDS` should appear. Staying in contact must not keep charging penalties. Exit/re-enter within two seconds to check cooldown; exit/re-enter after two seconds to check a new hit.
7. Check movement/colliders across the whole path and verify every collectible can be reached. Recover all three. **YOU ESCAPED** should show six recovered objects when starting the run from Level 1. **PLAY AGAIN** must reset Level 1 and its count.
8. Let the timer expire, and separately take a penalty with fewer than five seconds remaining. Confirm **GAME OVER**, frozen gameplay, an unlocked cursor, **RETRY ROOM**, and **RESTART FROM LEVEL 1**.
9. Stop Play mode before making permanent Inspector adjustments. Visual balance, actual collision callbacks, button interaction, performance, and webcam gameplay still need this Editor pass.

The simplification also preserved all physics component definitions exactly, and removed roughly 250 purely decorative GameObjects per scene. No gameplay or tracking scripts were changed in that pass. No changes were committed or pushed.


## Flashlight-focused lighting update

Both levels now use a realtime spotlight at intensity 120 (previously 5), range 30 metres, outer angle 68 degrees, and inner angle 34 degrees. The broad inner/outer transition gives the spot a soft edge; standard URP distance attenuation provides falloff. Neutral tone mapping remains enabled and bloom is reduced from 0.25 to 0.05. The existing exposure settings were neutral and were not boosted.

The first-person player capsule surrounded the flashlight while still casting shadows, which could obstruct its own beam. Player mesh shadow casting is now disabled; the CharacterController, all colliders, and obstacle shadows remain unchanged. Per-light shadow bias is 0.025 with normal bias 0.12 and a 0.05 near plane. URP is set to respect these per-light bias values. Both PC and Mobile quality pipelines support per-pixel additional lights and soft spotlight shadows; Mobile shadow support was enabled to prevent light leaking through solid room geometry.

The beam follows the existing camera-child transform and head-aim script. Camera references, head input, smoothing, yaw/pitch limits, flicker, and Python are unchanged. No new tracking or lighting scripts were added.

Lighting files changed: `Assets/Scenes/Level1.unity`, `Assets/Scenes/Level2.unity`, `Assets/Materials/OrbMat.mat`, `GuideMint.mat`, `GuideAmber.mat`, `Assets/Settings/SampleSceneProfile.asset`, and `Assets/Settings/Mobile_RPAsset.asset`.

To test, open each saved level, allow Unity to reload it from disk, and enter Play mode. Inspect surfaces at roughly 3, 6, and 10 metres, turn the beam across a crate edge, and look away from the collectibles. The illuminated centre should be clear, its edges gradual, shadows should block light behind solid crates, and unlit objects should no longer glow. Test head movement in both directions and up/down. In Play mode only, temporarily disable the Flashlight Light component to verify that no room-filling light remains, then re-enable it. Check the Game view rather than the Scene view's editor lighting. If testing Mobile quality, verify obstacle shadows there as well.

Static scene/reference checks and compilation passed. Physics and gameplay blocks were compared against pre-lighting snapshots and match exactly. All gameplay, UI, saved-progress, and head-tracking scripts, Python files, the selection scene, and build list are unchanged. Actual brightness, shadow quality, and visibility on your monitor still require the Unity Play mode pass. No commits or pushes were made.
