# Verification map

- `cargo test --workspace --all-features`: engine and core behavior, configuration, Discord identity/payloads, session discovery, manual mode and startup gates.
- `cargo fmt --all --check` and strict Clippy: formatting and static analysis.
- `dotnet run --project tests/desktop-updates/UpdateTests.csproj`: update metadata, stable-version comparison, trusted links, bounded responses, rate limits, offline failures and timeouts.
- `scripts/verify-web.ps1 -AppRoot <absolute path>`: real local WebView2 interactions, responsive geometry, active/hidden footer, settings, manual controls and crash recovery.
- `scripts/verify-desktop.ps1 -AppRoot <absolute path> -CrossVolume`: native Windows layout, config backups, background behavior, startup gates and shortcut resources.

The previous engine-only promotion pipeline is retired in this independent desktop repository. Engine behavior tests remain intact. Desktop release steps are documented in [releasing](../docs/releasing.md).
