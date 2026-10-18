using System.Globalization;
using AIUsageWidget.Core;

namespace AIUsageWidget.Localization;

/// <summary>User-visible text in English (default) and Korean.</summary>
public sealed class Strings
{
    public static Strings English { get; } = new(
        code: "en",
        fiveHour: "5h",
        weekly: "Week",
        left: "{0}% left",
        resets: "resets {0}",
        resetsIn: "in {0}",
        hours: "{0}h {1}m",
        minutes: "{0}m",
        updatedAgo: "updated {0} ago",
        justNow: "updated just now",
        notInstalled: "Not installed",
        notSignedIn: "Sign in with the CLI",
        notConnected: "Not connected · use the tray menu",
        waiting: "Waiting for data",
        waitingClaude: "Waiting for a Claude Code session",
        unsupported: "No official usage interface yet",
        error: "Could not read usage",
        starting: "Loading…",
        menuLanguage: "Language",
        menuSystemLanguage: "System default",
        menuConnectClaude: "Connect Claude Code (edits ~/.claude/settings.json)",
        menuDisconnectClaude: "Disconnect Claude Code",
        menuStartAtLogin: "Start at login",
        menuRefresh: "Refresh now",
        menuOpenProject: "Project page",
        menuQuit: "Quit",
        claudeConnectFailed: "Could not change Claude Code settings: {0}",
        days: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]);

    public static Strings Korean { get; } = new(
        code: "ko",
        fiveHour: "5시간",
        weekly: "주간",
        left: "{0}% 남음",
        resets: "{0} 초기화",
        resetsIn: "{0} 후",
        hours: "{0}시간 {1}분",
        minutes: "{0}분",
        updatedAgo: "{0} 전 갱신",
        justNow: "방금 갱신",
        notInstalled: "설치되지 않음",
        notSignedIn: "CLI에서 로그인하세요",
        notConnected: "연결 안 됨 · 트레이 메뉴에서 연결",
        waiting: "데이터 대기 중",
        waitingClaude: "Claude Code 세션 대기 중",
        unsupported: "공식 사용량 인터페이스 없음",
        error: "사용량을 읽지 못함",
        starting: "불러오는 중…",
        menuLanguage: "언어",
        menuSystemLanguage: "시스템 기본값",
        menuConnectClaude: "Claude Code 연결 (~/.claude/settings.json 수정)",
        menuDisconnectClaude: "Claude Code 연결 해제",
        menuStartAtLogin: "로그인 시 자동 실행",
        menuRefresh: "지금 새로 고침",
        menuOpenProject: "프로젝트 페이지",
        menuQuit: "종료",
        claudeConnectFailed: "Claude Code 설정을 바꾸지 못했습니다: {0}",
        days: ["일", "월", "화", "수", "목", "금", "토"]);

    private Strings(string code, string fiveHour, string weekly, string left, string resets, string resetsIn,
        string hours, string minutes, string updatedAgo, string justNow, string notInstalled, string notSignedIn,
        string notConnected, string waiting, string waitingClaude, string unsupported, string error, string starting,
        string menuLanguage, string menuSystemLanguage, string menuConnectClaude, string menuDisconnectClaude,
        string menuStartAtLogin, string menuRefresh, string menuOpenProject, string menuQuit,
        string claudeConnectFailed, string[] days)
    {
        Code = code;
        FiveHour = fiveHour;
        Weekly = weekly;
        Left = left;
        Resets = resets;
        ResetsIn = resetsIn;
        Hours = hours;
        Minutes = minutes;
        UpdatedAgo = updatedAgo;
        JustNow = justNow;
        NotInstalled = notInstalled;
        NotSignedIn = notSignedIn;
        NotConnected = notConnected;
        Waiting = waiting;
        WaitingClaude = waitingClaude;
        Unsupported = unsupported;
        Error = error;
        Starting = starting;
        MenuLanguage = menuLanguage;
        MenuSystemLanguage = menuSystemLanguage;
        MenuConnectClaude = menuConnectClaude;
        MenuDisconnectClaude = menuDisconnectClaude;
        MenuStartAtLogin = menuStartAtLogin;
        MenuRefresh = menuRefresh;
        MenuOpenProject = menuOpenProject;
        MenuQuit = menuQuit;
        ClaudeConnectFailed = claudeConnectFailed;
        Days = days;
    }

    public string Code { get; }
    public string FiveHour { get; }
    public string Weekly { get; }
    public string Left { get; }
    public string Resets { get; }
    public string ResetsIn { get; }
    public string Hours { get; }
    public string Minutes { get; }
    public string UpdatedAgo { get; }
    public string JustNow { get; }
    public string NotInstalled { get; }
    public string NotSignedIn { get; }
    public string NotConnected { get; }
    public string Waiting { get; }
    public string WaitingClaude { get; }
    public string Unsupported { get; }
    public string Error { get; }
    public string Starting { get; }
    public string MenuLanguage { get; }
    public string MenuSystemLanguage { get; }
    public string MenuConnectClaude { get; }
    public string MenuDisconnectClaude { get; }
    public string MenuStartAtLogin { get; }
    public string MenuRefresh { get; }
    public string MenuOpenProject { get; }
    public string MenuQuit { get; }
    public string ClaudeConnectFailed { get; }
    public string[] Days { get; }

    /// <summary>Resolves a setting value ("system", "en", "ko") to a string table.</summary>
    public static Strings For(string setting) => setting switch
    {
        "ko" => Korean,
        "en" => English,
        _ => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ko" ? Korean : English,
    };

    public static string ProviderName(ProviderId id) => id switch
    {
        ProviderId.Codex => "Codex",
        ProviderId.ClaudeCode => "Claude Code",
        ProviderId.Antigravity => "Antigravity",
        _ => id.ToString(),
    };

    public string StateText(ProviderSnapshot snapshot) => snapshot.State switch
    {
        ProviderState.NotInstalled => NotInstalled,
        ProviderState.NotSignedIn => NotSignedIn,
        ProviderState.NotConnected => NotConnected,
        ProviderState.WaitingForData => snapshot.Provider == ProviderId.ClaudeCode ? WaitingClaude : Waiting,
        ProviderState.Unsupported => Unsupported,
        ProviderState.Error => Error,
        ProviderState.Starting => Starting,
        _ => string.Empty,
    };
}
