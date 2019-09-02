; Extension infomation

#define ExtensionName "SOSIEL CEMMA"
#define AppVersion "1.0"
#define AppPublisher "The SOSIEL Foundation"
#define AppURL "https://www.sosiel.org"

; Build directory
#define BuildDir "C:\workspace\SOSIEL\CEMMA-Game-Repo\Demo\bin\Release\netcoreapp2.0"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{FBE5B57A-8D29-455C-ABFD-E1CE21B97F89}
AppName={#ExtensionName}
AppVersion={#AppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName=C:\{#ExtensionName}
DefaultGroupName={#ExtensionName}
AllowNoIcons=yes
DisableProgramGroupPage=yes
LicenseFile=C:\workspace\SOSIEL\CEMMA-Game-Repo\deploy\installer\THE SOSIEL PLATFORM LICENSE AGREEMENT.rtf
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir={#SourcePath}
OutputBaseFilename={#ExtensionName} {#AppVersion}-setup
SetupIconFile=C:\workspace\SOSIEL\sosiel_icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: users-modify

[Files]
Source: {#BuildDir}\*; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\workspace\SOSIEL\SOSIEL-w-CEMMA\SOSIEL CEMMA\Demo\bin\Release\netcoreapp2.0\configuration.json"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\workspace\SOSIEL\SOSIEL-w-CEMMA\SOSIEL CEMMA\Demo\bin\Release\netcoreapp2.0\Demo.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\workspace\SOSIEL\SOSIEL-w-CEMMA\SOSIEL CEMMA\Demo\bin\Release\netcoreapp2.0\Run.bat"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\workspace\SOSIEL\SOSIEL-w-CEMMA\SOSIEL CEMMA\Demo\bin\Release\netcoreapp2.0\SOSIEL.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\workspace\SOSIEL\SOSIEL-w-CEMMA\SOSIEL CEMMA\Demo\bin\Release\netcoreapp2.0\SOSIEL_CEMMA.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\SOSIEL CEMMA - old\Demo\CsvHelper.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\workspace\SOSIEL\SOSIEL-w-CEMMA\SOSIEL CEMMA\Demo\bin\Release\netcoreapp2.0\Demo.deps.json"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\SOSIEL CEMMA - old\Demo\DocumentFormat.OpenXml.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\SOSIEL PLATFORM\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\SOSIEL PLATFORM\Demo.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\workspace\SOSIEL\CEMMA-Game-Repo\Demo\bin\Release\netcoreapp2.0\birth_probability.csv" ; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\workspace\SOSIEL\CEMMA-Game-Repo\Demo\bin\Release\netcoreapp2.0\death_probability.csv" ; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\workspace\SOSIEL\CEMMA-Game-Repo\Demo\bin\Release\netcoreapp2.0\general_probability.csv" ; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

