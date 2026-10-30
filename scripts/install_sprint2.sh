#!/usr/bin/env bash
set -euo pipefail
ROOT="$HOME/Documents/project-rivet"
EDITOR="/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity"
if [[ ! -x "$EDITOR" ]]; then EDITOR="$HOME/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity"; fi
cd "$ROOT"
test -s docs/internal/MASTER_PLAN.md || { echo 'ERROR: master plan missing'; exit 1; }
test -f Assets/_Rivet/Scenes/FirstFlight.unity || { echo 'ERROR: First Flight scene missing'; exit 1; }
test -x "$EDITOR" || { echo 'ERROR: Unity 6.3 Editor not installed at expected location'; exit 1; }
git remote get-url origin | grep -Eq 'github.com[:/]sumcoinlabs/project-rivet(\.git)?$' || { echo 'ERROR: unexpected Git remote'; exit 1; }

# Do not start a second editor process for the same project.
if pgrep -fl 'Unity.app/Contents/MacOS/Unity' | grep -F -- "$ROOT" >/dev/null 2>&1; then
  echo 'Unity appears open on this project. Quit Unity completely and rerun this script.'
  exit 1
fi

echo '========== COMPILE + VERIFY THE FIRST FLIGHT SCENE =========='
mkdir -p Library
LOG="$ROOT/Library/rivet-sprint2-verify.log"
if ! "$EDITOR" -batchmode -nographics -quit -accept-apiupdate -projectPath "$ROOT" \
    -executeMethod ProjectRivet.Editor.Sprint2Verifier.Verify -logFile "$LOG"; then
    echo "Unity verification failed; last 100 lines: $LOG"
    tail -n 100 "$LOG" || true
    exit 1
fi
if ! grep -Fq 'PROJECT RIVET SPRINT 2 SMOKE TEST PASS' "$LOG"; then
  echo 'Verification marker missing; investigate editor log before committing.'
  tail -n 100 "$LOG" || true
  exit 1
fi
echo '========== COMMIT SOURCE =========='
git add Assets/_Rivet scripts/install_sprint2.sh
for optional in Assets/_Rivet.meta Packages/packages-lock.json ProjectSettings/SceneTemplateSettings.json; do
  if [[ -e "$optional" ]]; then git add "$optional"; fi
done
if ! git diff --cached --quiet; then
    git commit -m 'Sprint 2: controlled Wingo physics and interactive charging courses'
fi

echo '========== PUSH =========='
git push origin main
git status -sb
echo '========== OPEN UNITY =========='
APP="${EDITOR%/Contents/MacOS/Unity}"
open -a "$APP" --args -projectPath "$ROOT" || true
echo 'SUCCESS. Open Assets/_Rivet/Scenes/FirstFlight.unity and press Play.'
