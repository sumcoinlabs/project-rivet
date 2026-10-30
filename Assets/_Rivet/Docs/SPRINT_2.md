# Project Rivet — Sprint 2: The first playable loop

**Scope:** Original in-house C#/Unity 6.3 gameplay. Existing FirstFlight scene and original asset GUIDs are preserved. Not final art, no Tesla marks/models, no adverts and no purchases.

## What's materially different

- Wingo is no longer a **one-shot projectile**. Launch by dragging in the intended direction, and drag again to hop/recover; Space provides a standard launch and another hop once almost stationary.
- Clear in-world yellow **trajectory preview** during aiming; launch strength depends on how far you drag; no more initial `12.8` impulse that throws him across the whole map.
- **Glide** by holding Space (or holding/clicking on the playfield after launch); flight power depletes and a little recharges when collecting coins or resting.
- Damped rotation, velocity caps, adjustable trajectory, less punishing x bounds and more sensible mobile camera tracking.
- Three **different** test courses: simple charging boulevard, canyon crossing with spring and bumper, and wind-assisted raised charging platform with moving bridge.
- Recognizable original futuristic rounded **SUV-shaped** body with roof glass, four visible tires, headlights, and flapping roof doors (still primitives; proper original model remains a future art task).
- Better world staging: hills, clouds, platform trim, moving parts, battery bar, charging pad, success confetti, results/next panel.
- Level wins store best coin totals and unlocked progress in local PlayerPrefs. No account, Cloud or analytics.

## Run

Open `Assets/_Rivet/Scenes/FirstFlight.unity` in the Unity Editor and press Play. You must click the **Game** tab for keyboard input. `Shift + Space` maximizes that panel.

| Action | Mac | iPhone/Android |
|---|---|---|
| Aim | Drag up/right in playfield | Swipe up/right |
| Fire | Release mouse/trackpad | Lift finger |
| Quick launch | Space | Normal swipe |
| Air control | Hold Space or click | Hold after releasing, or swipe again |
| Another hop | Drag again | Swipe again |
| Retry | R or Retry button | Retry button |
| Next stage (debug) | N | Completion button |

A drag must exceed ~26 screen pixels. Starting a swipe in the top HUD or bottom instruction bar is ignored. The aim preview is a simple ballistic approximation; springs, bumpers, wind and glide affect the real flight.

## QA checklist for the Mac before further development

1. First course can be won by reaching the glowing charger. Confirm the completion overlay appears and button advances.
2. Changing drag length changes launch strength; holding Space in flight noticeably slows falling and consumes battery.
3. A second drag can recover the car; coins increment on contact; retry resets coins and position.
4. `N` cycles through all three courses. Fly through the canyon and find the wind tunnel on stage 3.
5. On narrower Game aspect (e.g. 9:16), verify the camera tracks and UI remains legible.
6. Check Unity Console for errors, red or yellow; make screen recording if course is unwinnable.

**Known:** Primitive procedural car and props; no hand-made art/models, in-editor level authoring, character unlocks, sound, mobile build testing, AdMob, purchase flow, or production difficulty balancing yet. This is now a gameplay loop prototype, not a release candidate. Tuning/solvability must be verified in the Unity Editor and on devices.

## Open source

References, not imported source: `villesepp/UnityGameJamTemplate` (MIT), `SinanErmis/HyperTemplate` (MIT core, dependency licensing differs), `Unity-Technologies/PhysicsExamples2D` (Unity Companion, 2D), and `googleads/googleads-mobile-unity` (official future SDK). No external gameplay source/assets incorporated into Sprint 2.
