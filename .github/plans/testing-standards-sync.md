<!--
Transient plan document. Delete when this repo's rollout is complete.
-->

# Testing standards sync — LobCorp.ConfigurationManager

Scoped slice of the org-wide initiative. Master plan: https://github.com/open-lobotomy/.github/blob/main/.github/plans/testing-standards-sync.md

## Context

This repo is the smallest of the 5 consumer rollouts — test projects are already `.Tests` (plural) and the source-generator package already uses `InternalsVisibleTo` correctly under the new analyzer classification. Work here is the smoke test for the whole rollout.

## This repo's work

### TESTING.md drop-in
- [ ] Merge the incoming `automated-sync` PR that adds `TESTING.md` at repo root (opened automatically after master TESTING.md PR merges in `open-lobotomy-github`).

### IVT audit (three-way classification)
- [ ] `ConfigurationManager.csproj` (runtime mod DLL) — application under the new rule (Unity mod, `Harmony_Patch` entry point). IVT remains allowed. No change.
- [ ] `LobotomyCorporation.Mods.ConfigurationManager.Integration.csproj` (source generator) — analyzer classification under the new rule. IVT remains allowed. The 18 internal-targeting unit tests continue to work as-is. No change.

### CLAUDE.md shortening
- [ ] Reduce the `## Testing` coverage in `.claude/CLAUDE.md` to a one-line pointer to `TESTING.md`.
- [ ] Preserve as repo-specific notes: net35 target-framework constraint, `ConfigurationManagerAttributes` simple-type-name reflection matching, `RootNamespace`/`AssemblyName` collision-prevention contract.
- [ ] Remove the `[ExcludeFromCodeCoverage]` bullet (now covered by the Category 1/2 pattern in TESTING.md).

### Tooling pin bump
- [ ] After `open-lobotomy-tooling` ships the `check-testing-doc` subcommand, bump the pin here so CI picks up the failsafe.

## Verification

- [ ] `dotnet build ConfigurationManager.slnx` passes.
- [ ] `dotnet test ConfigurationManager.slnx` — all tests pass; the Integration unit tests still cover internal types via `InternalsVisibleTo`.
- [ ] `dotnet ci --check` passes locally.
- [ ] Opening this repo on github.com shows `TESTING.md` at root with correct version header.
- [ ] Fresh `dotnet ci --check` after a divergent local edit to `TESTING.md` fails with the expected drift message (once `check-testing-doc` feature flag is on).

## Notes

- **Rollout order:** this repo goes first in step 6 of the master plan (smallest change, good smoke test). Once this PR merges cleanly, proceed with the other 4 in any order.
- **No renames needed.** The test projects here are already `LobCorp.ConfigurationManager.Test` (singular, to be renamed) and `LobotomyCorporation.Mods.ConfigurationManager.Integration.Tests` (already plural). Wait — double-check at execution time: if `LobCorp.ConfigurationManager.Test` is singular, add it to the rename list. Otherwise, skip.
- **Integration package is not a sync target.** It's a source generator consumed via NuGet. The test classification for IVT is analyzer/generator = allowed.
