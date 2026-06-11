# Hangul Cursor Indicator

Windows에서 현재 활성 창의 한/영 입력 상태를 감지해 마우스 포인터 오른쪽 아래에 작은 배지로 `한` 또는 `A`를 표시하는 WPF 프로그램입니다.

## 기술 스택

- C# / .NET 8 이상
- WPF: 클릭 통과 가능한 최상위 반투명 배지 창
- Windows Forms `NotifyIcon`: 시스템 트레이 메뉴
- Windows API P/Invoke: 활성 창, IME 변환 상태, 커서 위치, 창 확장 스타일 제어

외부 NuGet 패키지는 사용하지 않습니다.

## 주요 기능

- 백그라운드 실행
- 활성 창의 키보드 레이아웃과 Korean IME 변환 모드 감지
- 한글 모드: `한`, 영문 모드: `A`
- 마우스 포인터를 따라다니는 클릭 통과 배지
- Alt+Tab 및 작업표시줄에서 배지 숨김
- 시스템 트레이 메뉴
  - 표시 켜기/끄기
  - 프로그램 시작 시 자동 실행 켜기/끄기
  - 종료
- 다중 모니터 및 Per-Monitor DPI 인식

## 빌드

```powershell
dotnet build -c Release
```

## 실행

```powershell
dotnet run -c Release
```

또는 빌드 결과물을 직접 실행합니다.

```text
bin\Release\net8.0-windows\HangulCursorIndicator.exe
```

## 단일 EXE 배포

x64 self-contained single-file EXE:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

생성 위치:

```text
bin\Release\net8.0-windows\win-x64\publish\HangulCursorIndicator.exe
```

x86이 필요하면 런타임 식별자만 바꿉니다.

```powershell
dotnet publish -c Release -r win-x86 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

생성 위치:

```text
bin\Release\net8.0-windows\win-x86\publish\HangulCursorIndicator.exe
```

## 자동 실행 방식

트레이 메뉴의 `프로그램 시작 시 자동 실행 켜기/끄기`는 현재 사용자 레지스트리 Run 키를 사용합니다.

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

값 이름:

```text
HangulCursorIndicator
```

관리자 권한은 필요하지 않습니다.

## Windows API 사용 이유

- `GetForegroundWindow`: 현재 입력 대상이 되는 활성 창 확인
- `GetWindowThreadProcessId`: 활성 창의 UI 스레드 ID 확인
- `GetKeyboardLayout`: 활성 창 스레드 기준 입력 언어 확인
- `ImmGetDefaultIMEWnd`: 활성 창에 연결된 기본 IME 창 확인
- `SendMessage` + `WM_IME_CONTROL` + `IMC_GETCONVERSIONMODE`: Korean IME가 실제 한글 변환 모드인지 확인
- `GetCursorPos`: 물리 화면 좌표 기준 마우스 위치 확인
- `SetWindowLongPtr`: 배지 창에 `WS_EX_TOOLWINDOW`, `WS_EX_TRANSPARENT`, `WS_EX_LAYERED`, `WS_EX_NOACTIVATE` 적용
- `SetWindowPos`: DPI 배율과 모니터가 다른 환경에서도 HWND를 화면 좌표로 직접 이동

## 알려진 한계

- 일부 UWP, Electron, 게임, 관리자 권한으로 실행된 앱은 IME 상태 조회가 제한되거나 지연될 수 있습니다.
- Korean IME가 아닌 다른 한글 입력기 또는 특수 IME는 `IMC_GETCONVERSIONMODE` 결과가 다를 수 있습니다.
- 보안 데스크톱, 전체 화면 독점 모드, 원격 데스크톱 환경에서는 최상위 배지 표시가 제한될 수 있습니다.
- 현재 구현은 50ms 폴링 방식입니다. 일반 사용에서는 즉시 반응에 가깝지만 Windows 메시지 훅 방식의 완전한 이벤트 기반 구현은 아닙니다.
