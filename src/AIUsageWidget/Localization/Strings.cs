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
        menuSettings: "Settings…",
        settingsTitle: "AIUsageWidget Settings",
        displayMode: "Display mode",
        modeTerminal: "Attach below the terminal window",
        modeTerminalHint: "Shown only while a terminal is in front.",
        modeWidget: "Standalone widget",
        modeWidgetHint: "Always shown. Drag it to any place on the screen.",
        widgetTopmost: "Keep the widget above other windows",
        showIme: "Show keyboard input state (EN / 한) at the right end",
        claudeSection: "Claude Code",
        claudeConnected: "Connected through the status line.",
        claudeDisconnected: "Not connected. Connecting edits ~/.claude/settings.json and keeps your current status line.",
        connect: "Connect",
        disconnect: "Disconnect",
        general: "General",
        close: "Close",
        imeTitle: "Input",
        antigravityHalted: "Stopped: unexpected CLI output",
        antigravitySection: "Antigravity",
        antigravityGroup: "Model group",
        antigravityGroupHint: "Each group has its own 5-hour and weekly limits. The list comes from the CLI.",
        antigravityGroupsPending: "Groups appear after the first successful read.",
        refreshSection: "Refresh",
        refreshInterval: "Update interval",
        refreshManual: "Manual",
        refreshOneMinute: "Every minute",
        refreshFiveMinutes: "Every 5 minutes",
        refreshThirtyMinutes: "Every 30 minutes",
        refreshOneHour: "Every hour",
        refreshCustom: "Custom",
        minutesUnit: "minutes",
        refreshHint: "Codex and Antigravity are queried at this interval. Claude Code values appear as soon as Claude Code reports them.",
        refreshManualHint: "Update with the ↻ button on the panel or Refresh now in the tray menu.",
        closeButtonAction: "Close button (✕) on the panel",
        closeAsk: "Ask each time",
        closeHide: "Hide the panel",
        closeQuit: "Quit the program",
        closePrompt: "Close the panel or quit?",
        closeHideButton: "Hide panel",
        closeQuitButton: "Quit program",
        cancel: "Cancel",
        menuShowPanel: "Show panel",
        closeTooltip: "Close",
        used: "{0}% used",
        sectionsHeading: "Panel sections",
        sectionsHint: "Choose which sections to show and change their order with the arrows.",
        moveUp: "Move up",
        moveDown: "Move down",
        sectionDate: "Date",
        sectionWeekday: "Day of the week",
        sectionTime: "Current time",
        sectionIme: "Keyboard input (EN / 한)",
        showSeconds: "Show seconds in the time",
        usageDisplay: "Usage display",
        usageRemaining: "Remaining (% left)",
        usageUsed: "Used (% used)",
        dateTitle: "Date",
        weekdayTitle: "Day",
        timeTitle: "Time",
        monthDayFormat: "MMM d",
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
        menuSettings: "설정…",
        settingsTitle: "AIUsageWidget 설정",
        displayMode: "표시 방식",
        modeTerminal: "터미널 창 아래에 붙이기",
        modeTerminalHint: "터미널이 앞에 있을 때만 표시합니다.",
        modeWidget: "독립 위젯",
        modeWidgetHint: "항상 표시합니다. 화면 어디로든 끌어서 옮길 수 있습니다.",
        widgetTopmost: "위젯을 다른 창보다 항상 위에 표시",
        showIme: "맨 오른쪽에 키보드 입력 상태(EN / 한) 표시",
        claudeSection: "Claude Code",
        claudeConnected: "상태 표시줄을 통해 연결되어 있습니다.",
        claudeDisconnected: "연결되어 있지 않습니다. 연결하면 ~/.claude/settings.json을 고치며, 지금 쓰는 상태 표시줄은 그대로 유지됩니다.",
        connect: "연결",
        disconnect: "연결 해제",
        general: "일반",
        close: "닫기",
        imeTitle: "입력",
        antigravityHalted: "중지됨: CLI 출력이 예상과 다름",
        antigravitySection: "Antigravity",
        antigravityGroup: "모델 그룹",
        antigravityGroupHint: "그룹마다 5시간 한도와 주간 한도가 따로 있습니다. 목록은 CLI에서 받아 옵니다.",
        antigravityGroupsPending: "처음 한도를 읽은 뒤에 그룹 목록이 나타납니다.",
        refreshSection: "갱신",
        refreshInterval: "갱신 주기",
        refreshManual: "수동",
        refreshOneMinute: "1분마다",
        refreshFiveMinutes: "5분마다",
        refreshThirtyMinutes: "30분마다",
        refreshOneHour: "1시간마다",
        refreshCustom: "사용자 설정",
        minutesUnit: "분",
        refreshHint: "Codex와 Antigravity는 이 주기로 조회합니다. Claude Code 값은 Claude Code가 알려 주는 즉시 반영합니다.",
        refreshManualHint: "보조창의 ↻ 버튼이나 트레이 메뉴의 \"지금 새로 고침\"으로 갱신합니다.",
        closeButtonAction: "보조창의 닫기 버튼(✕)",
        closeAsk: "매번 묻기",
        closeHide: "보조창만 닫기",
        closeQuit: "프로그램 종료",
        closePrompt: "창만 닫을까요, 프로그램을 종료할까요?",
        closeHideButton: "창만 닫기",
        closeQuitButton: "프로그램 종료",
        cancel: "취소",
        menuShowPanel: "보조창 다시 표시",
        closeTooltip: "닫기",
        used: "{0}% 사용",
        sectionsHeading: "보조창 섹션",
        sectionsHint: "표시할 섹션을 고르고 화살표로 순서를 바꿉니다.",
        moveUp: "위로",
        moveDown: "아래로",
        sectionDate: "날짜",
        sectionWeekday: "요일",
        sectionTime: "현재 시각",
        sectionIme: "키보드 입력 상태 (EN / 한)",
        showSeconds: "시각에 초 표시",
        usageDisplay: "사용량 표시",
        usageRemaining: "남은 양 (% 남음)",
        usageUsed: "사용한 양 (% 사용)",
        dateTitle: "날짜",
        weekdayTitle: "요일",
        timeTitle: "시각",
        monthDayFormat: "M월 d일",
        days: ["일", "월", "화", "수", "목", "금", "토"]);

    private Strings(string code, string fiveHour, string weekly, string left, string resets, string resetsIn,
        string hours, string minutes, string updatedAgo, string justNow, string notInstalled, string notSignedIn,
        string notConnected, string waiting, string waitingClaude, string unsupported, string error, string starting,
        string menuLanguage, string menuSystemLanguage, string menuConnectClaude, string menuDisconnectClaude,
        string menuStartAtLogin, string menuRefresh, string menuOpenProject, string menuQuit,
        string claudeConnectFailed, string menuSettings, string settingsTitle, string displayMode, string modeTerminal, string modeTerminalHint, string modeWidget, string modeWidgetHint, string widgetTopmost, string showIme, string claudeSection, string claudeConnected, string claudeDisconnected, string connect, string disconnect, string general, string close, string imeTitle, string antigravityHalted, string antigravitySection, string antigravityGroup, string antigravityGroupHint, string antigravityGroupsPending, string refreshSection, string refreshInterval, string refreshManual, string refreshOneMinute, string refreshFiveMinutes, string refreshThirtyMinutes, string refreshOneHour, string refreshCustom, string minutesUnit, string refreshHint, string refreshManualHint, string closeButtonAction, string closeAsk, string closeHide, string closeQuit, string closePrompt, string closeHideButton, string closeQuitButton, string cancel, string menuShowPanel, string closeTooltip, string used, string sectionsHeading, string sectionsHint, string moveUp, string moveDown, string sectionDate, string sectionWeekday, string sectionTime, string sectionIme, string showSeconds, string usageDisplay, string usageRemaining, string usageUsed, string dateTitle, string weekdayTitle, string timeTitle, string monthDayFormat, string[] days)
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
        MenuSettings = menuSettings;
        SettingsTitle = settingsTitle;
        DisplayMode = displayMode;
        ModeTerminal = modeTerminal;
        ModeTerminalHint = modeTerminalHint;
        ModeWidget = modeWidget;
        ModeWidgetHint = modeWidgetHint;
        WidgetTopmost = widgetTopmost;
        ShowIme = showIme;
        ClaudeSection = claudeSection;
        ClaudeConnected = claudeConnected;
        ClaudeDisconnected = claudeDisconnected;
        Connect = connect;
        Disconnect = disconnect;
        General = general;
        Close = close;
        ImeTitle = imeTitle;
        AntigravityHalted = antigravityHalted;
        AntigravitySection = antigravitySection;
        AntigravityGroup = antigravityGroup;
        AntigravityGroupHint = antigravityGroupHint;
        AntigravityGroupsPending = antigravityGroupsPending;
        RefreshSection = refreshSection;
        RefreshInterval = refreshInterval;
        RefreshManual = refreshManual;
        RefreshOneMinute = refreshOneMinute;
        RefreshFiveMinutes = refreshFiveMinutes;
        RefreshThirtyMinutes = refreshThirtyMinutes;
        RefreshOneHour = refreshOneHour;
        RefreshCustom = refreshCustom;
        MinutesUnit = minutesUnit;
        RefreshHint = refreshHint;
        RefreshManualHint = refreshManualHint;
        CloseButtonAction = closeButtonAction;
        CloseAsk = closeAsk;
        CloseHide = closeHide;
        CloseQuit = closeQuit;
        ClosePrompt = closePrompt;
        CloseHideButton = closeHideButton;
        CloseQuitButton = closeQuitButton;
        Cancel = cancel;
        MenuShowPanel = menuShowPanel;
        CloseTooltip = closeTooltip;
        Used = used;
        SectionsHeading = sectionsHeading;
        SectionsHint = sectionsHint;
        MoveUp = moveUp;
        MoveDown = moveDown;
        SectionDate = sectionDate;
        SectionWeekday = sectionWeekday;
        SectionTime = sectionTime;
        SectionIme = sectionIme;
        ShowSeconds = showSeconds;
        UsageDisplay = usageDisplay;
        UsageRemaining = usageRemaining;
        UsageUsed = usageUsed;
        DateTitle = dateTitle;
        WeekdayTitle = weekdayTitle;
        TimeTitle = timeTitle;
        MonthDayFormat = monthDayFormat;
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
    public string MenuSettings { get; }
    public string SettingsTitle { get; }
    public string DisplayMode { get; }
    public string ModeTerminal { get; }
    public string ModeTerminalHint { get; }
    public string ModeWidget { get; }
    public string ModeWidgetHint { get; }
    public string WidgetTopmost { get; }
    public string ShowIme { get; }
    public string ClaudeSection { get; }
    public string ClaudeConnected { get; }
    public string ClaudeDisconnected { get; }
    public string Connect { get; }
    public string Disconnect { get; }
    public string General { get; }
    public string Close { get; }
    public string ImeTitle { get; }
    public string AntigravityHalted { get; }
    public string AntigravitySection { get; }
    public string AntigravityGroup { get; }
    public string AntigravityGroupHint { get; }
    public string AntigravityGroupsPending { get; }
    public string RefreshSection { get; }
    public string RefreshInterval { get; }
    public string RefreshManual { get; }
    public string RefreshOneMinute { get; }
    public string RefreshFiveMinutes { get; }
    public string RefreshThirtyMinutes { get; }
    public string RefreshOneHour { get; }
    public string RefreshCustom { get; }
    public string MinutesUnit { get; }
    public string RefreshHint { get; }
    public string RefreshManualHint { get; }
    public string CloseButtonAction { get; }
    public string CloseAsk { get; }
    public string CloseHide { get; }
    public string CloseQuit { get; }
    public string ClosePrompt { get; }
    public string CloseHideButton { get; }
    public string CloseQuitButton { get; }
    public string Cancel { get; }
    public string MenuShowPanel { get; }
    public string CloseTooltip { get; }
    public string Used { get; }
    public string SectionsHeading { get; }
    public string SectionsHint { get; }
    public string MoveUp { get; }
    public string MoveDown { get; }
    public string SectionDate { get; }
    public string SectionWeekday { get; }
    public string SectionTime { get; }
    public string SectionIme { get; }
    public string ShowSeconds { get; }
    public string UsageDisplay { get; }
    public string UsageRemaining { get; }
    public string UsageUsed { get; }
    public string DateTitle { get; }
    public string WeekdayTitle { get; }
    public string TimeTitle { get; }
    public string MonthDayFormat { get; }
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

    public string SectionName(SectionId id) => id switch
    {
        SectionId.Codex => ProviderName(ProviderId.Codex),
        SectionId.ClaudeCode => ProviderName(ProviderId.ClaudeCode),
        SectionId.Antigravity => ProviderName(ProviderId.Antigravity),
        SectionId.Date => SectionDate,
        SectionId.Weekday => SectionWeekday,
        SectionId.Time => SectionTime,
        SectionId.Ime => SectionIme,
        _ => id.ToString(),
    };

    public string StateText(ProviderSnapshot snapshot) => snapshot.State switch
    {
        ProviderState.NotInstalled => NotInstalled,
        ProviderState.NotSignedIn => NotSignedIn,
        ProviderState.NotConnected => NotConnected,
        ProviderState.WaitingForData => snapshot.Provider == ProviderId.ClaudeCode ? WaitingClaude : Waiting,
        ProviderState.Unsupported => Unsupported,
        ProviderState.Error => snapshot.Detail == "unexpected-output" ? AntigravityHalted : Error,
        ProviderState.Starting => Starting,
        _ => string.Empty,
    };
}
