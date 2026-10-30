# Project Rivet — First Flight (Sprint 1)

**Prototype scope:** 3 temporary physics courses, swipe launch, flap/glide, springs, bumper, energy coins and charging goal. These are in-house scripts, not source copied from the open-source references. No Tesla logos/names/replica assets and no AdMob or IAP code at this stage.

## Build the scene

1. Open `~/Documents/project-rivet` in Unity 6.3 LTS.
2. Select **Project Rivet → Build First Flight Scene** (or run the editor headless via the supplied Mac script).
3. Open `Assets/_Rivet/Scenes/FirstFlight.unity` and press **Play**.
4. On Mac, drag up and right with the mouse and release; click/hold in flight to flap. **Space** starts a test launch and provides flight lift when held. **R** retries, **N** loads the next course.
5. On touch devices, swipe upward/right, release, then hold to flap. The game automatically adjusts its buttons to screen height, but the layout is still prototype-only.

## Proof-of-concept architecture

- `FirstFlightSceneBuilder`: editor-only scene creation and Build Settings.
- `FirstFlightGame`: level manager and runtime greybox layout for exactly three development courses.
- `WingoPhysics`: 3D Rigidbody constrained to XY, swipe impulse, battery-based wing lift and hinged visual doors.
- `ChargerTrigger`, `CoinTrigger`, `SpringPad`: reusable interactions.

## Next sprint — do not treat this as production ready

- Playtest swipe force, flight balance, and gap solvability on iOS and Android.
- Replace primitive Wingo with original approved 3D character model; improve wing motion and collision silhouette.
- Split runtime courses into stage prefabs + ScriptableObject level catalog as described in the local master plan.
- Build UGUI safe-area HUD, sound, camera polish and editor validation.
- Add EditMode/PlayMode tests and persistent progression before ads or purchases.

## Open-source references reviewed

- `villesepp/UnityGameJamTemplate` — MIT code/CC0 placeholders; 2D and different Unity version; do not import as a project.
- `SinanErmis/HyperTemplate` — MIT core, separate third-party dependency licenses, older Unity; level system patterns only.
- `Unity-Technologies/PhysicsExamples2D` — educational reference, different physics dimensionality and license.
- `googleads/googleads-mobile-unity` — official SDK; **not integrated until privacy policy and ad pacing are ready**.

Do not list third-party code as incorporated until actual source is imported and license notices are added.
