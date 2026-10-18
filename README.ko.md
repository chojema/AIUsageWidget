# AIUsageWidget

[English](README.md) | 한국어

> 이 문서는 [README.md](README.md)의 한국어 번역본입니다. 두 문서의 내용이 다르면 영어본을 기준으로 합니다.

AIUsageWidget은 Windows와 macOS용 작은 데스크톱 앱입니다. 터미널 창 바로 아래에 사용량 보조창을 띄워, 터미널을 벗어나지 않고도 AI 코딩 에이전트의 남은 한도를 확인할 수 있게 합니다.

> **버전 0.0.9 — 초기 릴리스**
> Windows x64와 macOS(Apple Silicon·Intel) 다운로드를 제공합니다. macOS Apple Silicon에서 실행과 설정·위젯 기본 동작을 확인했으며, Intel Mac에서는 실행하지 않았습니다.

## 표시 항목

보조창은 여러 섹션으로 이루어집니다. 섹션마다 **설정 → 보조창 섹션**에서 표시 여부와 순서를 바꿀 수 있습니다.

| 섹션 | 내용 |
|---|---|
| Codex, Claude Code, Antigravity | 사용량 한도(아래 참고) |
| 날짜 | 월과 일, 예: "10월 18일" |
| 요일 | 예: "일". 주말은 강조색으로 표시 |
| 현재 시각 | "18:04", **시각에 초 표시**를 켜면 "18:04:15" |
| 키보드 입력 상태 | EN / 한(아래 참고) |

기본 순서는 사용량 섹션 다음에 날짜, 요일, 시각, 키보드 입력 상태입니다.

지원하는 에이전트마다 다음을 보여 줍니다.

| 항목 | 설명 |
|---|---|
| 5시간 | 5시간 사용 한도의 남은 비율과 초기화 시각 |
| 주간 | 주간 사용 한도의 남은 비율과 초기화 시각 |

사용량은 남은 비율("62% 남음", 기본값)이나 사용한 비율("38% 사용")로 표시할 수 있으며 **설정 → 사용량 표시**에서 고릅니다. 막대는 글자와 같은 양을 보여 주고, 색은 늘 남은 양을 기준으로 합니다.

초기화 시각은 시계 시각(하루 이상 남았으면 요일 포함)과 남은 시간으로 함께 표시합니다. 값을 얻을 수 없으면 꾸민 숫자 대신 이유(설치되지 않음, 로그인 필요, 연결 안 됨, 데이터 대기 중)를 표시합니다.

## 표시 방식

**설정**(트레이 메뉴 → 설정…, 또는 트레이 아이콘 클릭)에서 고릅니다.

| 방식 | 동작 |
|---|---|
| **터미널 창 아래에 붙이기** (기본값) | 아래 설명처럼 터미널이 앞에 있을 때만 표시합니다. |
| **독립 위젯** | 항상 표시합니다. 원하는 곳으로 끌어 옮기면 위치를 기억합니다. 다른 창보다 항상 위에 둘 수도 있습니다. |

## 갱신 주기

**설정 → 갱신**에서 Codex와 Antigravity를 조회하는 주기를 고릅니다. 수동, 1분마다, 5분마다(기본값), 30분마다, 1시간마다, 또는 사용자 설정(1~1440분) 중에서 선택합니다.

- **수동**은 시작할 때와, 보조창의 **↻** 또는 트레이 메뉴의 **지금 새로 고침**을 누를 때만 조회합니다.
- Claude Code 값은 따로 조회하지 않고 Claude Code가 알려 주는 즉시 반영합니다. 수동일 때는 ↻를 눌러야 읽습니다.

## 보조창 버튼

보조창 오른쪽 끝에 작은 버튼 두 개가 있습니다.

- **↻**는 모든 값을 지금 새로 고칩니다.
- **✕**는 보조창을 닫습니다. 기본값은 보조창 안에서 **창만 닫기**와 **프로그램 종료** 중 무엇을 할지 묻는 것입니다. **설정 → 닫기 버튼**에서 동작을 고정할 수 있습니다. 숨긴 보조창은 트레이 메뉴의 **보조창 다시 표시**나 표시 방식 변경으로 다시 나타납니다.

## 키보드 입력 상태 표시

보조창 맨 오른쪽에 앞에 있는 창의 키보드 입력 상태를 **EN** 또는 **한**으로 표시하고, 다른 입력기는 언어 코드(예: JA, ZH)로 표시합니다. 설정 → 보조창 섹션에서 끌 수 있습니다.

- Windows: 포커스된 창의 키보드 배열과 IME 변환 모드를 읽습니다.
- macOS: 현재 입력 소스를 읽습니다.

## 터미널에 붙인 보조창의 동작

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
| Antigravity CLI | `agy -p /usage --output-format json` (읽기 전용 print 모드 명령, agy 1.1.12 이상) | 없음. CLI에 로그인되어 있으면 됩니다. 모델 그룹은 설정에서 고릅니다. |

### Antigravity 모델 그룹

Antigravity는 모델 그룹(예: *Gemini Models*, *Claude and GPT models*)마다 5시간 한도와 주간 한도를 따로 알려 줍니다. 보조창에는 한 번에 한 그룹을 보여 주며, 제목에 그룹 이름을 함께 표시합니다(예: "Antigravity · Gemini").

- **설정 → Antigravity → 모델 그룹**에서 고릅니다. 목록은 CLI에서 받아 오므로 새 그룹도 자동으로 나타납니다.
- 고르기 전에는 *Gemini Models*를 보여 줍니다(그 그룹이 없으면 첫 그룹).
- 고른 그룹이 요금제 변경 등으로 사라지면 같은 방식으로 대체합니다.

명령은 셸을 거치지 않고 갱신 주기마다 실행하며, 에이전트 턴이 실행되지 않았음이 확인된 응답만 받아들입니다. CLI가 다르게 응답하면 그 세션 동안 수집을 멈춰, 백그라운드에서 한도가 소비되지 않게 합니다.

### Claude Code 연결

Claude Code는 한도 정보를 상태 표시줄 명령에만 전달합니다. **Claude Code 연결**은 `~/.claude/settings.json`의 `statusLine` 항목이 `AIUsageWidget claude-statusline`을 실행하도록 설정합니다.

- `statusLine` 항목만 바꿉니다. 원래 파일의 백업을 `settings.json.aiusagewidget-backup`으로 한 번 저장합니다.
- 이미 쓰던 상태 표시줄이 있으면 그대로 동작합니다. AIUsageWidget이 같은 입력으로 이전 명령을 실행하고 그 출력을 보여 줍니다.
- 두 한도 값만 앱 데이터 폴더에 저장합니다. 세션의 다른 정보는 저장하지 않습니다.
- **Claude Code 연결 해제**를 누르면 이전 상태 표시줄로 되돌립니다.
- Claude Code는 세션이 실행 중일 때만 한도를 알려 주므로, 보조창에 마지막 갱신 시각을 표시합니다.

Claude Code가 그 경로에서 앱을 실행하므로, 연결하기 전에 앱을 계속 둘 폴더에 놓으세요. 나중에 옮기면 새 위치에서 한 번 실행하면 경로가 갱신됩니다.

## 설치 (Windows)

1. [Releases](https://github.com/chojema/AIUsageWidget/releases)에서 `AIUsageWidget-0.0.9-win-x64.zip`을 받아 `AIUsageWidget.exe`를 계속 둘 폴더(예: `%LOCALAPPDATA%\Programs\AIUsageWidget`)에 풉니다.
2. `AIUsageWidget.exe`를 실행합니다. 알림 영역(트레이)에 아이콘이 나타납니다.
3. 필요하면 **설정**에서 표시 방식, Claude Code 연결, 로그인 시 자동 실행을 고릅니다.

실행 파일에 아직 코드 서명이 없어 처음 실행할 때 Windows SmartScreen 경고가 나올 수 있습니다.

## 설치 (macOS)

macOS 14 이상이 필요합니다. .NET 런타임이 포함되어 있어 별도로 설치할 필요가 없습니다.

1. [v0.0.9](https://github.com/chojema/AIUsageWidget/releases/tag/v0.0.9)에서 Mac에 맞는 zip을 받습니다. **Apple Silicon(M 시리즈)**: `AIUsageWidget-0.0.9-osx-arm64.zip`, **Intel**: `AIUsageWidget-0.0.9-osx-x64.zip`.
2. 압축을 풀고 Claude Code를 연결하기 전에 `AIUsageWidget.app`을 `/Applications`로 옮깁니다. 업데이트할 때는 기존 앱을 종료하고 같은 위치에서 교체하면 설정이 유지됩니다.
3. 앱을 열고 메뉴 막대 아이콘의 **설정…**에서 표시 방식을 고릅니다. 기본 터미널 부착 모드에서는 지원 터미널이 앞에 있을 때 보조창이 나타납니다. Dock에는 아이콘이 나타나지 않습니다.

이 빌드는 임시(ad-hoc) 서명만 적용되었으며 Developer ID 서명과 공증은 없습니다. macOS가 실행을 차단하면 [Apple 안내](https://support.apple.com/102445)에 따라 앱 실행을 시도한 뒤, 다운로드를 신뢰하는 경우에만 **시스템 설정 → 개인정보 보호 및 보안 → 확인 없이 열기**를 사용하세요.

## 트레이 메뉴

- **보조창 다시 표시**(보조창을 숨겼을 때만)
- **설정…**(트레이 아이콘을 클릭해도 열림): 표시 방식, 위젯 항상 위, 보조창 섹션과 순서, 초 표시, 사용량 표시, 갱신 주기, 닫기 버튼 동작, 로그인 시 자동 실행, 언어, Antigravity 모델 그룹, Claude Code 연결
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

macOS 번들에는 임시(ad-hoc) 서명만 들어갑니다. Developer ID 서명과 공증은 포함되지 않으며, 위 설치 안내를 참고하세요.

## 개인정보와 보안

- 이 앱은 인증 정보를 읽거나 저장하거나 전송하지 않습니다.
- 네트워크 접속은 공식 CLI 자신이 하는 것뿐입니다.
- 저장하는 데이터는 설정 파일과 최근 Claude Code 한도 값뿐입니다.

이 저장소가 개인 데이터를 소스에서 빼는 방식은 [SECURITY.md](SECURITY.md)를 참고하세요.

## 알려진 제한

- Antigravity 출력 형식은 CLI 릴리스 노트에만 적혀 있어 버전에 따라 바뀔 수 있습니다.
- Claude Code 값은 Claude Code 세션이 실행 중일 때만 갱신됩니다.
- macOS: 로그인 시 자동 실행은 아직 없습니다. Apple Silicon에서 실행, 설정, 위젯, 입력 상태와 Codex·Antigravity 실제 한도 표시를 확인했습니다. Intel 실행, 다중 화면 터미널 부착, macOS Claude Code 연결은 아직 검증하지 않았습니다.
- 보조창은 터미널 창을 따라가며, 창 안의 개별 탭은 구분하지 않습니다.

## 라이선스

[MIT](LICENSE)
