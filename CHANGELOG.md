# Changelog

## 0.0.3 — not yet released

- Antigravity CLI limits through the read-only `agy -p /usage --output-format json` command, with the 5-hour and weekly windows and reset times.
- Antigravity model group selection in Settings; the panel title shows the group in use.
- Safety check: the Antigravity answer is accepted only when no agent turn ran; otherwise collection stops for the session.
- Arguments for CLIs started through the macOS login shell are quoted.

## 0.0.2 — 2026-10-18

- Settings window (tray menu → Settings…, or click the tray icon).
- Display mode: attach below the terminal window, or a standalone widget that can be dragged anywhere, remembers its position and can stay on top.
- Keyboard input indicator (EN / 한) at the right end of the panel.
- A saved widget position is kept inside the screen.

## 0.0.1 — 2026-10-18

First release.

- Usage panel attached below the frontmost terminal window; hides when another app is in front.
- Codex CLI limits through the official `codex app-server` interface.
- Claude Code limits through an opt-in status line bridge that keeps an existing status line.
- Antigravity CLI shown as unavailable until an official interface exists.
- 5-hour and weekly remaining share with reset time and time remaining.
- English and Korean user interface.
- Tray menu with language, Claude Code connection, start at login (Windows) and refresh.
- Windows x64 single-file executable. macOS build script (untested in this release).
