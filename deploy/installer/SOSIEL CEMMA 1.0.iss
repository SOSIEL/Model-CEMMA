; Extension infomation

#define ExtensionName "SOSIEL CEMMA"
#define AppVersion "1.0"
#define AppPublisher "The SOSIEL Foundation"
#define AppURL "https://www.sosiel.org"

; Build directory
#define BuildDir "..\\..\\Demo\bin\Release\netcoreapp2.0"

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
DisableDirPage=no
DefaultGroupName={#ExtensionName}
AllowNoIcons=yes
DisableProgramGroupPage=yes
LicenseFile=THE SOSIEL PLATFORM LICENSE AGREEMENT.rtf
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir={#SourcePath}
OutputBaseFilename={#ExtensionName} {#AppVersion}-setup
;SetupIconFile=C:\workspace\SOSIEL\sosiel_icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: users-modify

[Files]
Source: {#BuildDir}\*; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

