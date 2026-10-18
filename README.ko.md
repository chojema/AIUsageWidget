# AIUsageWidget

[English](README.md) | 한국어

> 이 문서는 [README.md](README.md)의 한국어 번역본입니다. 두 문서의 내용이 다르면 영어본을 기준으로 합니다.

AIUsageWidget은 Windows와 macOS용 작은 데스크톱 앱입니다. 터미널 창 바로 아래에 사용량 보조창을 띄워, 터미널을 벗어나지 않고도 AI 코딩 에이전트의 남은 한도를 확인할 수 있게 합니다.

> **버전 0.0.1 — 초기 릴리스**
> 주 플랫폼인 Windows에서 시험했습니다. macOS 빌드는 아직 시험하지 않았습니다.

## 표시 항목

지원하는 에이전트마다 다음을 보여 줍니다.

| 항목 | 설명 |
|---|---|
| 5시간 | 5시간 사용 한도의 남은 비율과 초기화 시각 |
| 주간 | 주간 사용 한도의 남은 비율과 초기화 시각 |

초기화 시각은 시계 시각(하루 이상 남았으면 요일 포함)과 남은 시간으로 함께 표시합니다. 값을 얻을 수 없으면 꾸민 숫자 대신 이유(설치되지 않음, 로그인 필요, 연결 안 됨, 데이터 대기 중)를 표시합니다.

## 보조창 동작

- 최전면 터미널 창 바로 아래에 나타나고, 창의 위치와 가로 폭을 따라갑니다.
- 터미널 창이 여러 개 열려 있어도 최전면 창에만 표시합니다.
- 다른 앱이 최전면으로 오면 숨어서 그 앱을 가리지 않습니다.
- 터미널을 최소화하거나 닫으면 숨습니다.
- 터미널 아래에 공간이 없으면(예: 최대화) 터미널의 아래쪽 가장자리 위에 겹쳐 표시합니다.
- 보조창은 터미널의 키보드 입력 포커스를 가져가지 않습니다.

인식하는 터미널: Windows에서는 Windows Terminal, Windows 콘솔, WezTerm, Alacritty, mintty(Git Bash), Tabby, Hyper, Wave, Warp, ConEmu 등, macOS에서는 Terminal, iTerm2, WezTerm, Alacritty, kitty, Ghostty, Warp, Tabby, Hyper, Wave, Rio입니다. 다른 터미널은 설정 파일에 추가할 수 있습니다(아래 참고).

## 사용량 수집 방식

AIUsageWidget은 각 CLI의 공식 인터페이스만 사용합니다. 인증 정보를 읽거나 문서에 없는 서비스를 호출하지 않습니다.

| 에이전트 | 출처 | 설정 |
|---|---|---|
| Codex CLI | `codex app-server` (`account/rateLimits/read`, `account/rateLimits/updated`) | 없음. CLI에 로그인되어 있으면 됩니다. |
| Claude Code | 상태 표시줄 데이터 (`rate_limits.five_hour`, `rate_limits.seven_day`) | 트레이 메뉴에서 **Claude Code 연결**을 한 번 누릅니다. |
| Antigravity CLI | 제공 안 됨 | CLI가 대화형 `/usage` 패널에서만 한도를 보여 주고, 다른 프로그램용 공식 인터페이스가 아직 없습니다. 보조창에 이 상태를 표시합니다. |

### Claude Code 연결

Claude Code는 한도 정보를 상태 표시줄 명령에만 전달합니다. **Claude Code 연결**은 `~/.claude/settings.json`의 `statusLine` 항목이 `AIUsageWidget claude-statusline`을 실행하도록 설정합니다.

- `statusLine` 항목만 바꿉니다. 원래 파일의 백업을 `settings.json.aiusagewidget-backup`으로 한 번 저장합니다.
- 이미 쓰던 상태 표시줄이 있으면 그대로 동작합니다. AIUsageWidget이 같은 입력으로 이전 명령을 실행하고 그 출력을 보여 줍니다.
- 두 한도 값만 앱 데이터 폴더에 저장합니다. 세션의 다른 정보는 저장하지 않습니다.
- **Claude Code 연결 해제**를 누르면 이전 상태 표시줄로 되돌립니다.
- Claude Code는 세션이 실행 중일 때만 한도를 알려 주므로, 보조창에 마지막 갱신 시각을 표시합니다.

Claude Code가 그 경로에서 앱을 실행하므로, 연결하기 전에 앱을 계속 둘 폴더에 놓으세요. 나중에 옮기면 새 위치에서 한 번 실행하면 경로가 갱신됩니다.

## 설치 (Windows)

1. [Releases](https://github.com/chojema/AIUsageWidget/releases)에서 `AIUsageWidget-0.0.1-win-x64.zip`을 받아 `AIUsageWidget.exe`를 계속 둘 폴더(예: `%LOCALAPPDATA%\Programs\AIUsageWidget`)에 풉니다.
2. `AIUsageWidget.exe`를 실행합니다. 알림 영역(트레이)에 아이콘이 나타납니다.
3. 필요하면 트레이 메뉴에서 **Claude Code 연결**과 **로그인 시 자동 실행**을 선택합니다.

실행 파일에 아직 코드 서명이 없어 처음 실행할 때 Windows SmartScreen 경고가 나올 수 있습니다.

## 트레이 메뉴

- **언어**: 시스템 기본값, English, 한국어
- **Claude Code 연결 / 연결 해제**
- **로그인 시 자동 실행** (Windows)
- **지금 새로 고침**
- **프로젝트 페이지**, **종료**

## 설정

설정은 `%APPDATA%\AIUsageWidget\settings.json`(Windows) 또는 `~/Library/Application Support/AIUsageWidget/settings.json`(macOS)에 저장됩니다. 다른 터미널을 인식시키려면 Windows 프로세스 이름(`.exe` 제외)이나 macOS 번들 식별자를 추가합니다.

```json
{
  "extraTerminals": ["MyTerminal", "com.example.terminal"]
}
```

## 소스에서 빌드

[.NET 10 SDK](https://dotnet.microsoft.com/download)가 필요합니다.

```sh
dotnet test --solution AIUsageWidget.slnx -c Release
```

Windows 실행 파일(`dist/win-x64/AIUsageWidget.exe`와 릴리스용 zip):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/publish-windows.ps1
```

macOS 앱 번들(`dist/osx-arm64/AIUsageWidget.app`와 릴리스용 zip)은 Mac에서 실행합니다.

```sh
sh scripts/publish-macos.sh arm64   # Intel Mac은 x64
```

macOS 번들에는 로컬 실행용 임시(ad-hoc) 서명만 들어갑니다. 다른 Mac에 배포하려면 Developer ID 서명과 공증이 필요합니다.

## 개인정보와 보안

- 이 앱은 인증 정보를 읽거나 저장하거나 전송하지 않습니다.
- 네트워크 접속은 공식 CLI 자신이 하는 것뿐입니다.
- 저장하는 데이터는 설정 파일과 최근 Claude Code 한도 값뿐입니다.

이 저장소가 개인 데이터를 소스에서 빼는 방식은 [SECURITY.md](SECURITY.md)를 참고하세요.

## 알려진 제한

- 공식 인터페이스가 생기기 전까지 Antigravity 한도는 표시할 수 없습니다.
- Claude Code 값은 Claude Code 세션이 실행 중일 때만 갱신됩니다.
- macOS: 0.0.1에서 시험하지 않았고, 로그인 시 자동 실행은 아직 없습니다.
- 보조창은 터미널 창을 따라가며, 창 안의 개별 탭은 구분하지 않습니다.

## 라이선스

[MIT](LICENSE)
