# Desktop release procedure

1. Update the desktop and runtime versions, package default, changelog and versioned artwork URL together.
2. Run scripts/verify.ps1 and tests/desktop-updates/UpdateTests.csproj.
3. Build the Windows x64 runtime with scripts/build-release.ps1 -Architecture x64, then build the desktop with scripts/build-desktop.ps1 and an absolute output path.
4. Run verify-web.ps1 and verify-desktop.ps1 -CrossVolume against the candidate. Inspect narrow and wide screenshots. Use synthetic data in public screenshots.
5. Commit the reviewed source; rebuild the desktop to stamp the commit. Package with scripts/package-desktop.ps1. Never include Data, session fixtures, logs, personal config or credentials in releases.
6. Publish an annotated desktop-vX.Y.Z tag and a stable GitHub release in w-rakeeb/Codex-Discord-Rich-Presence. Upload the full portable ZIP and SHA256SUMS.txt. Do not replace published assets.
7. Download anonymously and verify the ZIP hash and every payload hash. Run the browser verifier on the downloaded package. Verify the versioned artwork URL before migrating saved image keys.
8. Back up the installed app, exit it gracefully and copy only package payloads. Preserve Data, manual settings, startup registration and historical installations. Installation does not run automatically through the update checker.

Current packaging targets Windows x64. ARM64/Linux/macOS engine source may be buildable, but this desktop release makes no support claim for untested platforms. No publishing workflow runs automatically.
