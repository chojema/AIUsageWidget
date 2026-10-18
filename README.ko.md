# AIUsageWidget

[English](README.md) | 한국어

터미널 아래 또는 독립 데스크톱 위젯에서 Codex, Claude Code, Antigravity의 사용량 한도를 확인합니다. Windows와 macOS, 영어와 한국어를 지원합니다.

## 다운로드 및 설치

[**0.0.9 다운로드**](https://github.com/chojema/AIUsageWidget/releases/tag/v0.0.9)

| 플랫폼 | 파일 |
|---|---|
| Windows x64 | `AIUsageWidget-0.0.9-win-x64.zip` |
| macOS Apple Silicon(M 시리즈) | `AIUsageWidget-0.0.9-osx-arm64.zip` |
| macOS Intel | `AIUsageWidget-0.0.9-osx-x64.zip` |

- **Windows:** 계속 사용할 폴더에 압축을 풀고 `AIUsageWidget.exe`를 실행합니다. 시스템 트레이 아이콘에서 설정을 엽니다.
- **macOS 14 이상:** 압축을 풀고 `AIUsageWidget.app`을 `/Applications`로 옮겨 실행합니다. 메뉴 막대 아이콘에서 설정을 엽니다. Dock 아이콘은 없습니다.
- **업데이트:** 앱을 종료하고 같은 위치에서 교체합니다. 기존 설정은 유지됩니다.

.NET을 별도로 설치할 필요는 없습니다. Windows 실행 파일은 미서명이므로 SmartScreen 경고가 나타날 수 있습니다. macOS 앱은 미공증 상태입니다. 실행이 차단되면 [Apple 안내](https://support.apple.com/102445)에 따라 다운로드를 신뢰하는 경우에만 **개인정보 보호 및 보안 → 확인 없이 열기**를 사용하세요.

## 주요 기능

- 5시간·주간 사용량 한도, 초기화 시각과 남은 시간 표시
- 남은 비율 또는 사용한 비율과 막대 표시
- 날짜, 요일, 시각(초 선택), EN / 한 등 키보드 입력 상태 표시
- 설정에서 섹션 표시·숨김 및 순서 변경
- 자동 갱신(기본 5분), 사용자 지정 주기 또는 수동 갱신

| 표시 방식 | 동작 |
|---|---|
| **터미널 창 아래에 붙이기** | 최전면 터미널을 따라가며 다른 앱이 앞에 오면 숨습니다. |
| **독립 위젯** | 원하는 곳으로 옮길 수 있고, 항상 위에 표시하도록 설정할 수 있습니다. |

Windows Terminal, Terminal, iTerm2, WezTerm, Alacritty, Ghostty, Warp 등을 지원합니다. 보조창은 개별 탭이 아닌 창을 따라갑니다.

**↻**로 새로 고치고 **✕**로 보조창을 숨기거나 앱을 종료합니다. 숨긴 보조창은 트레이·메뉴 막대 메뉴의 **보조창 다시 표시**로 복원합니다. 표시 항목, 언어, 갱신 및 닫기 동작은 **설정**에서 바꿉니다. 로그인 시 자동 실행은 Windows에서만 지원합니다.

## CLI 연결

| CLI | 설정 |
|---|---|
| Codex | Codex CLI에 로그인합니다. |
| Claude Code | 트레이·메뉴 막대 메뉴에서 **Claude Code 연결**을 선택합니다. |
| Antigravity | Antigravity CLI(1.1.12 이상)에 로그인하고 설정에서 모델 그룹을 고릅니다. |

Claude Code를 연결하기 전에 앱을 계속 사용할 위치에 두세요. 연결 시 `~/.claude/settings.json`의 `statusLine`을 변경하고 파일을 백업하며, 기존 상태 표시줄은 유지합니다. 연결을 해제하면 이전 설정으로 복원합니다. Claude Code 값은 세션이 실행 중일 때만 갱신됩니다.

사용량은 각 CLI의 공식 인터페이스로 읽으며, 앱은 인증 정보를 읽거나 저장하지 않습니다. [보안 안내](SECURITY.ko.md)를 참고하세요.

## 고급 설정

설정 파일 위치:

- Windows: `%APPDATA%\AIUsageWidget\settings.json`
- macOS: `~/Library/Application Support/AIUsageWidget/settings.json`

다른 터미널을 인식시키려면 `extraTerminals`에 Windows 프로세스 이름(`.exe` 제외) 또는 macOS 번들 식별자를 추가합니다.

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

Windows:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/publish-windows.ps1
```

macOS(Mac에서 실행):

```sh
sh scripts/publish-macos.sh arm64   # Intel은 x64
```

앱 번들과 릴리스 zip은 `dist/`에 생성됩니다.

[변경 내역](CHANGELOG.md) · [MIT 라이선스](LICENSE)
