# Verification

Run the engine, update-check, WebView2 and native verification commands in the root README. Browser fixtures include narrow layouts, 100/125/150/200% zoom, light/dark, forced colors, reduced motion and renderer recovery. Update checks use fake HTTP responses for newer/current/older releases, malformed responses, hostile links, timeouts, rate limits and network failures.

Real Windows 11 runtime validation supplements build checks. Physical Windows 10, mixed-DPI monitor transitions and a full screen-reader session remain unverified. No bug-free or universal performance claim is made. Screenshots contain synthetic fixture data.

The update checker only reads public release metadata. It does not download or install executable code. Future automatic installation would require its own signed-artifact and rollback design.
## 3.3.0 local release evidence

- 319 Rust/core/integration tests passed, with formatting and strict Clippy.
- 23 deterministic update-check tests passed; no network or credentials are needed for these fixtures.
- 165 real WebView2 checks passed, including active/hidden publication status, fixed sidebar footer, narrow layouts and enlarged scaling.
- 599 native Windows checks passed across separate app/config drives, including cache-safe shortcut paths and historical-source documentation routing.
- Windows PE validation rejects an architecture mismatch.

One early loaded-machine browser run completed all 164 assertions after the wrapper's former 120-second deadline. The wrapper now accepts a bounded timeout (240 seconds by default); assertions and feature checks were not removed. A complete later 165-check run passed. Compile-load timings are not app performance guarantees. The final public package is independently verified before installation.
