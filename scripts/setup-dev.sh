#!/usr/bin/env bash
# SPDX-License-Identifier: MIT
#
# Developer environment setup script.
# Run this after cloning the repository to install tools and pre-commit hooks.
#
# Usage: ./scripts/setup-dev.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

cd "$REPO_ROOT"

echo -e "${BLUE}Setting up development environment...${NC}"

# Step 1: Restore the CI tool
echo -e "\n${GREEN}[1/2]${NC} Restoring CI tool..."
dotnet tool restore

# Step 2: Install pre-commit hook
echo -e "\n${GREEN}[2/2]${NC} Installing pre-commit hook..."
dotnet ci --setup-hooks

echo -e "\n${GREEN}======================================================================${NC}"
echo -e "${GREEN}Development environment setup complete!${NC}"
echo -e "${GREEN}======================================================================${NC}"
echo ""
echo "Pre-commit hook installed. It runs 'dotnet ci --check' before each commit."
echo "You can also run 'dotnet ci' manually to check formatting, tests, and coverage."
