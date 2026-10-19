# Changelog

## 0.0.12 — 2026-10-19

- On macOS, find Codex and Antigravity CLIs installed in common folders such as `~/.local/bin`, even when that folder is added to `PATH` only in `~/.zshrc`. Previously they could show as not installed when the app was opened from Finder.
- macOS Apple Silicon / Intel downloads only. Windows is unchanged; keep using 0.0.11.

## 0.0.11 — 2026-10-19

- On Windows, attach only after the same terminal stays in front for one second. Brief background terminals no longer immediately show the panel; switching terminals starts a fresh delay.
- Apply non-activating window styles before showing the panel, and strengthen hidden startup for chained status line commands.
- Windows x64 and macOS Apple Silicon / Intel downloads.

## 0.0.10 — 2026-10-19

- Right-click the panel for Settings, Refresh now, Hide panel, and Quit program. The panel stays visible while its menu is open.
- Skip screen updates and unrelated keyboard input checks while the attached panel is hidden, then show the latest usage when returning to the terminal.
- Strengthen hidden startup for Windows CLI queries.
- Windows x64 and macOS Apple Silicon / Intel downloads.

## 0.0.9 — 2026-10-18

Versions 0.0.3 to 0.0.8 were not released; their changes are included here.

- Date, day of the week and current time sections (seconds optional), placed left of the keyboard input section by default.
- Panel sections: show or hide each of the seven sections and change their order in Settings. The earlier keyboard input on/off setting carries over.
- Usage display: share left ("62% left") or share used ("38% used").
- The standalone widget width follows the visible sections.
- The settings window scrolls and can be resized.
- Antigravity CLI limits through the read-only `agy -p /usage --output-format json` command, with the 5-hour and weekly windows and reset times.
- Antigravity model group selection in Settings; the panel title shows the group in use.
- Safety check: the Antigravity answer is accepted only when no agent turn ran; otherwise collection stops for the session.
- Arguments for CLIs started through the macOS login shell are quoted.
- Refresh interval in Settings: manual, 1, 5 (default), 30 or 60 minutes, or a custom number of minutes.
- Panel buttons: ↻ refreshes now; ✕ hides the panel or quits, asking inside the panel by default or using the action chosen in Settings.
- Tray menu item to show a hidden panel again.
- A copy started with its own data folder (AIUSAGEWIDGET_DATA_DIR) runs as a separate instance.

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
