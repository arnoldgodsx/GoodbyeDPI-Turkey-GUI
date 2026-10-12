; GoodbyeDPI-Turkey arayüzü kurulum betiği (Inno Setup 6).
; Derleme: önce gui\dist'e publish edin, sonra  iscc /DAppVersion=1.0 installer\setup.iss
; Çıktı: gui\installer\Output\GoodbyeDPI-Turkey-GUI-v<sürüm>-Setup.exe

#ifndef AppVersion
  #define AppVersion "1.0"
#endif
#define AppName "GoodbyeDPI-Turkey"
#define AppExe "Hub.exe"
#define SvcName "GoodbyeDPI"

[Setup]
AppId={{6B0F3C1E-6A57-4E0B-9C3E-2D6F1B7A9E41}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher=arnoldgodsx
AppPublisherURL=https://github.com/arnoldgodsx/hub
AppSupportURL=https://github.com/cagritaskn/GoodbyeDPI-Turkey
VersionInfoVersion={#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir=Output
OutputBaseFilename=GoodbyeDPI-Turkey-GUI-v{#AppVersion}-Setup
SetupIconFile=..\Assets\hub.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName} v{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=no

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\dist\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"
Name: "{autoprograms}\{#AppName} kaldır"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
; Kurulum yönetici olarak çalıştığı için ilk açılış UAC sormadan görevleri kurar.
Filename: "{app}\{#AppExe}"; Parameters: "--show"; WorkingDir: "{app}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im {#AppExe}"; Flags: runhidden; RunOnceId: "KillHub"
Filename: "{sys}\schtasks.exe"; Parameters: "/delete /tn ""Hub Autostart"" /f"; Flags: runhidden; RunOnceId: "DelAutostartTask"
Filename: "{sys}\schtasks.exe"; Parameters: "/delete /tn ""Hub"" /f"; Flags: runhidden; RunOnceId: "DelShowTask"
Filename: "{sys}\sc.exe"; Parameters: "stop {#SvcName}"; Flags: runhidden; RunOnceId: "StopSvc"
Filename: "{sys}\sc.exe"; Parameters: "delete {#SvcName}"; Flags: runhidden; RunOnceId: "DelSvc"
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im goodbyedpi.exe"; Flags: runhidden; RunOnceId: "KillEngine"
Filename: "{sys}\sc.exe"; Parameters: "stop WinDivert"; Flags: runhidden; RunOnceId: "StopWinDivert"
Filename: "{sys}\sc.exe"; Parameters: "delete WinDivert"; Flags: runhidden; RunOnceId: "DelWinDivert"

[Code]
var
  SvcWasRunning: Boolean;

function Run(const Exe, Params: String): Integer;
begin
  if not Exec(ExpandConstant('{sys}\') + Exe, Params, '', SW_HIDE, ewWaitUntilTerminated, Result) then
    Result := -1;
end;

// Güncellemede dosyalar kilitli olmasın: arayüzü ve GoodbyeDPI'ı durdur, sonra geri aç.
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  SvcWasRunning := Run('cmd.exe', '/c sc query {#SvcName} | find "RUNNING"') = 0;
  Run('taskkill.exe', '/f /im {#AppExe}');
  if SvcWasRunning then
  begin
    Run('sc.exe', 'stop {#SvcName}');
    Sleep(2000);
  end;
  Run('taskkill.exe', '/f /im goodbyedpi.exe');
  Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and SvcWasRunning then
    Run('sc.exe', 'start {#SvcName}');
end;
