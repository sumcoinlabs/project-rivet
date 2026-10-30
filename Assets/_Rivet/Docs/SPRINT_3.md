# Project Rivet — Sprint 3: Relay Run

Original code and vector art. This revision rebuilds the Sprint 2 launch demo around a contained, step-by-step physics puzzle.

## Gameplay

* Three levels: Relay Run, Spring Circuit, Skyline Switchback.
* Each level has five visible platforms, three **ordered** relay checkpoints, collectible optional energy coins, a locked energy barrier, and an actual charger cabinet.
* Touch checkpoints 1 → 2 → 3 to open the gate. Enter the charging area to complete the course.
* Bounded camera and arena. Failures trigger immediate retry, not a long free-flight sequence.
* Hops can be repeated after landing. Wings provide only a small gravity counterforce so traversing platforms remains central.

## Mac controls

* **Left / Right**: drive along a platform and steer gently in the air. Also sets facing direction for the next hop.
* **Up / Down**: raise / lower the angle of the next hop. Observe the cyan trajectory guide.
* **Space**: hop when on a platform. Hold it **while airborne** to deploy the wing doors and extend the hop slightly.
* **R**: restart. **N**: next prototype level.
* **Mouse/trackpad**: drag to choose a hop direction, release to hop; hold to glide when airborne.
* In Unity's Game view, **Shift + Space** maximizes the panel. Click inside the Game view to focus keyboard controls.

## Build / smoke test

Quit Unity; extract the Sprint 3 ZIP at the project root, overwriting the **single** `FirstFlightGame.cs` file. Run `bash scripts/install_sprint3.sh`. The script compiles/imports the scene and ten PNG sprites, then commits/pushes the changes to GitHub. If compilation fails it stops before committing.

The existing `Assets/_Rivet/Scenes/FirstFlight.unity` remains unchanged, including its manager component reference. Reopen the project and select that scene.

## Architecture

* `FirstFlightGame`: route logic, checkpoint sequence, UI, sprite-based arena, charger, progression prototype.
* `WingoController`: 3D rigidbody constrained to XY; keyboard and pointer inputs; launch guide; mild in-air adjustment.
* `StepTrigger`, `EnergyPickup`, `ChargerZone`, `SpringObstacle`, `BounceObstacle`: interactions.
* `RivetArtImporter`: configures bundled 10 original PNG illustrations as Sprites and verifies scene/art import.

**Art/IP**: All included SVG-to-PNG vector artwork was authored for this prototype, with independent original designs. No Tesla logos, third-party game screenshots, or licensed vehicle models. This is not a Tesla-endorsed product.

**Scope**: This is an editor-playable prototype, not a shipping game. It has no AdMob, purchases, server analytics, final vehicle model, proven portrait UX, or mobile builds. Physics balance and checkpoint reachability require playtesting in Unity on the Mac.
