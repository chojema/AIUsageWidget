# AIUsageWidget

English | [한국어](README.ko.md)

AIUsageWidget is a small desktop app for Windows and macOS. It shows a usage panel right below your terminal window, so you can see how much of your AI coding agent limits remain without leaving the terminal.

> **Version 0.0.9 — early release.**
> Windows is the primary platform and has been tested. The macOS build has not been tested yet.

## What it shows

The panel is made of sections. Each one can be shown or hidden and moved in **Settings → Panel sections**:

| Section | Content |
|---|---|
| Codex, Claude Code, Antigravity | Usage limits (below) |
| Date | Month and day, for example "Oct 18" |
| Day of the week | For example "Sun"; weekends are highlighted |
| Current time | "18:04", or "18:04:15" with **Show seconds in the time** |
| Keyboard input | EN / 한 (see below) |

By default the usage sections come first, followed by the date, day, time and keyboard input.

For each supported agent:

| Item | Description |
|---|---|
| 5h | Remaining share of the 5-hour usage limit and when it resets |
| Week | Remaining share of the weekly usage limit and when it resets |

Usage can be shown as the share left ("62% left", default) or the share used ("38% used"); choose in **Settings → Usage display**. The bar shows the same quantity as the text, and its color always reflects what is left.

Reset times are shown as a clock time (with the weekday when more than a day away) and as time remaining. When a value is unavailable, the panel says why (not installed, not signed in, not connected, waiting for data) instead of showing a made-up number.

## Display modes

Choose the mode in **Settings** (tray menu → Settings…, or click the tray icon):

| Mode | Behavior |
|---|---|
| **Attach below the terminal window** (default) | Shown only while a terminal is in front, as described below. |
| **Standalone widget** | Always shown. Drag it anywhere; the position is remembered. Optionally kept above other windows. |

## Refresh interval

Choose how often Codex and Antigravity are queried in **Settings → Refresh**: Manual, every minute, every 5 minutes (default), every 30 minutes, every hour, or a custom number of minutes (1 to 1440).

- **Manual** queries only at startup and when you click **↻** on the panel or **Refresh now** in the tray menu.
- Claude Code values are not queried; they appear as soon as Claude Code reports them (except in manual mode, where ↻ reads them).

## Panel buttons

The right end of the panel has two small buttons:

- **↻** refreshes all values now.
- **✕** closes the panel. By default it asks, inside the panel, whether to **Hide panel** or **Quit program**. Choose a fixed action in **Settings → Close button**. A hidden panel comes back with **Show panel** in the tray menu, or when you change the display mode.

## Keyboard input indicator

The right end of the panel shows the keyboard input state of the window in front: **EN** or **한** (Korean), and the language code for other input methods (for example JA or ZH). It can be turned off in Settings → Panel sections.

- Windows: read from the keyboard layout and the IME conversion mode of the focused window.
- macOS: read from the current input source.

## How the attached panel behaves

- It appears directly below the frontmost terminal window and follows its position and width.
- With several terminal windows open, it is shown only for the one in front.
- When any other app comes to the front, the panel hides, so it never covers that app.
- When the terminal is minimized or closed, the panel hides.
- If there is no room below the terminal (for example when it is maximized), the panel sits over the bottom edge of the terminal.
- The panel never takes keyboard focus from the terminal.

Recognized terminals: Windows Terminal, Windows console, WezTerm, Alacritty, mintty (Git Bash), Tabby, Hyper, Wave, Warp, ConEmu and others on Windows; Terminal, iTerm2, WezTerm, Alacritty, kitty, Ghostty, Warp, Tabby, Hyper, Wave and Rio on macOS. Others can be added in the settings file (see below).

## How usage data is collected

AIUsageWidget uses only official interfaces of each CLI. It never reads your credentials or calls undocumented services.

| Agent | Source | Setup |
|---|---|---|
| Codex CLI | `codex app-server` (`account/rateLimits/read`, `account/rateLimits/updated`) | None. Being signed in to the CLI is enough. |
| Claude Code | Status line data (`rate_limits.five_hour`, `rate_limits.seven_day`) | One click: **Connect Claude Code** in the tray menu. |
| Antigravity CLI | `agy -p /usage --output-format json` (read-only print-mode command; agy 1.1.12 or later) | None. Being signed in to the CLI is enough. Choose the model group in Settings. |

### Antigravity model groups

Antigravity reports separate 5-hour and weekly limits for each model group (for example *Gemini Models* and *Claude and GPT models*). The panel shows one group at a time, with its name in the title (for example "Antigravity · Gemini"):

- Pick the group in **Settings → Antigravity → Model group**. The list comes from the CLI, so new groups appear automatically.
- Until you choose, *Gemini Models* is shown (or the first group if that one does not exist).
- If the chosen group disappears, for example after a plan change, the panel falls back the same way.

The command runs at the refresh interval without a shell, and its answer is accepted only when it shows that no agent turn ran. If the CLI ever answers differently, collection stops for the session so that no quota can be spent in the background.

### Claude Code connection

Claude Code passes its rate limits only to a status line command. **Connect Claude Code** sets the `statusLine` entry in `~/.claude/settings.json` to run `AIUsageWidget claude-statusline`:

- Only the `statusLine` entry is changed. A backup of the original file is saved once as `settings.json.aiusagewidget-backup`.
- If you already had a status line, it keeps working: AIUsageWidget runs your previous command with the same input and shows its output.
- Only the two limit windows are saved (in the app's data folder). Nothing else from the session is stored.
- **Disconnect Claude Code** restores your previous status line.
- Claude Code reports limits only while a session is running, so the panel shows when the values were last updated.

Place the app in a permanent folder before connecting, because Claude Code runs it from that path. If you move it later, start it once from the new location and it updates the path.

## Install (Windows)

1. Download `AIUsageWidget-0.0.9-win-x64.zip` from [Releases](https://github.com/chojema/AIUsageWidget/releases) and extract `AIUsageWidget.exe` to a permanent folder, for example `%LOCALAPPDATA%\Programs\AIUsageWidget`.
2. Run `AIUsageWidget.exe`. An icon appears in the notification area (tray).
3. Optional: open **Settings** to choose the display mode, connect Claude Code, or start at login.

The executable is not code-signed yet, so Windows SmartScreen may show a warning on first run.

## Tray menu

- **Show panel** (only while the panel is hidden)
- **Settings…** (also opens with a click on the tray icon): display mode, widget on top, panel sections and their order, seconds, usage display, refresh interval, close button action, start at login, language, Antigravity model group, Claude Code connection
- **Language**: System default, English, or 한국어 (Korean)
- **Connect / Disconnect Claude Code**
- **Start at login** (Windows)
- **Refresh now**
- **Project page**, **Quit**

## Settings

Settings are stored in `%APPDATA%\AIUsageWidget\settings.json` (Windows) or `~/Library/Application Support/AIUsageWidget/settings.json` (macOS). To recognize another terminal, add its Windows process name (without `.exe`) or macOS bundle identifier:

```json
{
  "extraTerminals": ["MyTerminal", "com.example.terminal"]
}
```

## Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```sh
dotnet test --solution AIUsageWidget.slnx -c Release
```

Windows executable (`dist/win-x64/AIUsageWidget.exe` and a release zip):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/publish-windows.ps1
```

macOS app bundle (`dist/osx-arm64/AIUsageWidget.app` and a release zip), run on a Mac:

```sh
sh scripts/publish-macos.sh arm64   # or x64 for Intel Macs
```

The macOS bundle gets an ad-hoc signature for local use. Distributing it to other Macs requires a Developer ID signature and notarization.

## Privacy and security

- No credentials are read, stored or sent by this app.
- The only network access comes from the official CLIs themselves.
- Stored data is limited to the settings file and the latest Claude Code limit values.

See [SECURITY.md](SECURITY.md) for how this repository keeps private data out of the source.

## Known limitations

- The Antigravity output format is documented only in the CLI release notes and may change between versions.
- Claude Code values update only while a Claude Code session is running.
- macOS: untested so far; start at login is not available yet.
- The panel follows a terminal window, not individual tabs inside it.

## License

[MIT](LICENSE)
