#!/usr/bin/env bash
set -euo pipefail
ROOT="$HOME/Documents/project-rivet"
EDITOR="/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity"
if [[ ! -x "$EDITOR" ]]; then EDITOR="$HOME/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity"; fi
cd "$ROOT"
test -x "$EDITOR" || { echo 'ERROR: Unity 6.3 Editor missing'; exit 1; }
test -s docs/internal/MASTER_PLAN.md || { echo 'ERROR: local master plan missing'; exit 1; }
test -f Assets/_Rivet/Scenes/FirstFlight.unity || { echo 'ERROR: FirstFlight scene missing'; exit 1; }
test -f Assets/_Rivet/Scripts/Gameplay/FirstFlightGame.cs || { echo 'ERROR: sprint code missing'; exit 1; }
test -f Assets/_Rivet/Scripts/Editor/RivetArtImporter.cs || { echo 'ERROR: art importer missing'; exit 1; }
[[ $(find Assets/_Rivet/Resources/RivetArt -maxdepth 1 -type f -name '*.png' | wc -l | tr -d ' ') -eq 10 ]] || {
 echo 'ERROR: expected 10 original PNG art assets'; exit 1;
}
git remote get-url origin | grep -Eq 'github.com[:/]sumcoinlabs/project-rivet(\.git)?$' || {
 echo 'ERROR: unexpected Git remote'; exit 1;
}
if pgrep -fl 'Unity.app/Contents/MacOS/Unity' | grep -F -- "$ROOT" >/dev/null 2>&1; then
 echo 'ERROR: Quit Unity completely before importing. Then run: bash ~/Documents/project-rivet/scripts/install_sprint3.sh'; exit 1
fi

echo '========== UNITY COMPILE + 10 SPRITES + SCENE =========='
mkdir -p Library
LOG="$ROOT/Library/rivet-sprint3-verify.log"
if ! "$EDITOR" -batchmode -nographics -quit -accept-apiupdate -projectPath "$ROOT" \
    -executeMethod ProjectRivet.Editor.RivetArtImporter.ReimportAll -logFile "$LOG"; then
 echo 'Unity compilation or asset validation failed. Last 130 log lines:'
 tail -n 130 "$LOG" || true
 exit 1
fi
if ! grep -Fq 'RIVET_SPRINT3_VALIDATED' "$LOG"; then
 echo 'Verification marker missing. Unity editor log:'
 tail -n 130 "$LOG" || true
 exit 1
fi

echo '========== COMMIT AND PUSH =========='
git add Assets/_Rivet scripts/install_sprint3.sh DesignTools
for optional in Assets/_Rivet.meta Packages/packages-lock.json ProjectSettings/SceneTemplateSettings.json; do
 if [[ -e "$optional" ]]; then git add "$optional"; fi
done
if ! git diff --cached --quiet; then
 git commit -m 'Sprint 3: single-screen robot EV physics puzzles and arrow-key controls'
fi
git push origin main

echo '========== STATUS =========='
git status -sb
APP="${EDITOR%/Contents/MacOS/Unity}"
open -a "$APP" --args -projectPath "$ROOT" || true
echo 'SUCCESS: open Assets/_Rivet/Scenes/FirstFlight.unity, Game tab, Play'
echo 'Arrows move/steer and aim; Space hops and glides; R retries; N next level.'
