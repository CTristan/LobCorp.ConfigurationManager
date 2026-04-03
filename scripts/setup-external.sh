#!/bin/bash
# Sets up external game DLLs for building.
# Usage: ./scripts/setup-external.sh [source_path]

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DEST="$REPO_ROOT/external"

DEFAULT_SOURCE="$REPO_ROOT/../lobotomy-corporation-mods/external"
SOURCE="${1:-$DEFAULT_SOURCE}"

if [ ! -d "$SOURCE/LobotomyCorp_Data/Managed" ]; then
    echo "Error: Game DLLs not found at $SOURCE/LobotomyCorp_Data/Managed"
    echo "Usage: $0 [path_to_external_dir]"
    exit 1
fi

if [ -L "$DEST" ] || [ -d "$DEST" ]; then
    echo "External directory already exists at $DEST"
else
    ln -s "$SOURCE" "$DEST"
    echo "Created symlink: $DEST -> $SOURCE"
fi
