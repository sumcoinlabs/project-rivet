#!/usr/bin/env bash
set -euo pipefail

ROOT="$HOME/Documents/project-rivet"
REPO="sumcoinlabs/project-rivet"

cd "$ROOT"

command -v git >/dev/null || {
    echo "Git is not installed."
    exit 1
}

command -v gh >/dev/null || {
    echo "GitHub CLI is not installed."
    exit 1
}

gh auth status >/dev/null

if [ ! -d .git ]; then
    git init -b main
fi

if ! git check-ignore -q docs/internal/MASTER_PLAN.md; then
    echo "ERROR: Master plan is not excluded from public Git."
    exit 1
fi

git add -A

if git ls-files --cached | grep -Eq \
    '^(docs/internal/|docs/(MASTER_PLAN|DECISIONS|THIRD_PARTY)\.md)'; then
    echo "ERROR: Internal documents are staged."
    exit 1
fi

if ! git diff --cached --quiet; then
    git commit -m "Initialize Project Rivet workspace"
fi

git branch -M main

echo
echo "========== GITHUB =========="

if gh repo view "$REPO" >/dev/null 2>&1; then
    echo "Repository already exists."

    PRIVATE=$(gh repo view "$REPO" \
        --json isPrivate --jq '.isPrivate')

    if [ "$PRIVATE" != "false" ]; then
        echo "ERROR: Existing repository is not public."
        exit 1
    fi

    if git remote get-url origin >/dev/null 2>&1; then
        case "$(git remote get-url origin)" in
            "https://github.com/$REPO"|"https://github.com/$REPO.git"|"git@github.com:$REPO.git")
                ;;
            *)
                echo "ERROR: Origin points to another repository."
                exit 1
                ;;
        esac
    else
        git remote add origin "https://github.com/$REPO.git"
    fi

    git push -u origin main

else
    echo "Creating PUBLIC repository: $REPO"

    if git remote get-url origin >/dev/null 2>&1; then
        echo "ERROR: Origin already exists but target repository was not found."
        exit 1
    fi

    gh repo create "$REPO" \
        --public \
        --source=. \
        --remote=origin \
        --push
fi

echo
echo "========== VERIFY =========="

gh repo view "$REPO" \
    --json nameWithOwner,isPrivate,url \
    --jq '{repository:.nameWithOwner,private:.isPrivate,url:.url}'

echo
git status -sb

echo
echo "SUCCESS: Project Rivet synchronized with GitHub."
