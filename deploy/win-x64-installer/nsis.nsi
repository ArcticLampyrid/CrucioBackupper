!pragma warning error all
!include "MUI2.nsh"
!include "x64.nsh"
!define NAME "CrucioBackupper"
!define REGPATH_UNINSTSUBKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"
!define WIN64
Unicode true
Name "${NAME}"
OutFile "${NAME}Installer.exe"
!ifdef WIN64
    InstallDir "$PROGRAMFILES64\$(^Name)"
!else
    InstallDir "$PROGRAMFILES\$(^Name)"
!endif
InstallDirRegKey HKLM "Software\${NAME}" "InstallFolder"
RequestExecutionLevel Admin

!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_RUN "$INSTDIR\CrucioBackupper.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Launch ${NAME} instantly"
!define MUI_ICON "..\icon.ico"

!define MUI_LANGDLL_REGISTRY_ROOT "HKLM" 
!define MUI_LANGDLL_REGISTRY_KEY "Software\${NAME}" 
!define MUI_LANGDLL_REGISTRY_VALUENAME "InstallerLanguage"

;--------------------------------
;Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages
!insertmacro MUI_LANGUAGE "English" ; The first language is the default language
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "German"
!insertmacro MUI_LANGUAGE "Spanish"
!insertmacro MUI_LANGUAGE "SpanishInternational"
!insertmacro MUI_LANGUAGE "SimpChinese"
!insertmacro MUI_LANGUAGE "TradChinese"
!insertmacro MUI_LANGUAGE "Japanese"
!insertmacro MUI_LANGUAGE "Korean"
!insertmacro MUI_LANGUAGE "Italian"
!insertmacro MUI_LANGUAGE "Dutch"
!insertmacro MUI_LANGUAGE "Danish"
!insertmacro MUI_LANGUAGE "Swedish"
!insertmacro MUI_LANGUAGE "Norwegian"
!insertmacro MUI_LANGUAGE "NorwegianNynorsk"
!insertmacro MUI_LANGUAGE "Finnish"
!insertmacro MUI_LANGUAGE "Greek"
!insertmacro MUI_LANGUAGE "Russian"
!insertmacro MUI_LANGUAGE "Portuguese"
!insertmacro MUI_LANGUAGE "PortugueseBR"
!insertmacro MUI_LANGUAGE "Polish"
!insertmacro MUI_LANGUAGE "Ukrainian"
!insertmacro MUI_LANGUAGE "Czech"
!insertmacro MUI_LANGUAGE "Slovak"
!insertmacro MUI_LANGUAGE "Croatian"
!insertmacro MUI_LANGUAGE "Bulgarian"
!insertmacro MUI_LANGUAGE "Hungarian"
!insertmacro MUI_LANGUAGE "Thai"
!insertmacro MUI_LANGUAGE "Romanian"
!insertmacro MUI_LANGUAGE "Latvian"
!insertmacro MUI_LANGUAGE "Macedonian"
!insertmacro MUI_LANGUAGE "Estonian"
!insertmacro MUI_LANGUAGE "Turkish"
!insertmacro MUI_LANGUAGE "Lithuanian"
!insertmacro MUI_LANGUAGE "Slovenian"
!insertmacro MUI_LANGUAGE "Serbian"
!insertmacro MUI_LANGUAGE "SerbianLatin"
!insertmacro MUI_LANGUAGE "Arabic"
!insertmacro MUI_LANGUAGE "Farsi"
!insertmacro MUI_LANGUAGE "Hebrew"
!insertmacro MUI_LANGUAGE "Indonesian"
!insertmacro MUI_LANGUAGE "Mongolian"
!insertmacro MUI_LANGUAGE "Luxembourgish"
!insertmacro MUI_LANGUAGE "Albanian"
!insertmacro MUI_LANGUAGE "Breton"
!insertmacro MUI_LANGUAGE "Belarusian"
!insertmacro MUI_LANGUAGE "Icelandic"
!insertmacro MUI_LANGUAGE "Malay"
!insertmacro MUI_LANGUAGE "Bosnian"
!insertmacro MUI_LANGUAGE "Kurdish"
!insertmacro MUI_LANGUAGE "Irish"
!insertmacro MUI_LANGUAGE "Uzbek"
!insertmacro MUI_LANGUAGE "Galician"
!insertmacro MUI_LANGUAGE "Afrikaans"
!insertmacro MUI_LANGUAGE "Catalan"
!insertmacro MUI_LANGUAGE "Esperanto"
!insertmacro MUI_LANGUAGE "Asturian"
!insertmacro MUI_LANGUAGE "Basque"
!insertmacro MUI_LANGUAGE "Pashto"
!insertmacro MUI_LANGUAGE "ScotsGaelic"
!insertmacro MUI_LANGUAGE "Georgian"
!insertmacro MUI_LANGUAGE "Vietnamese"
!insertmacro MUI_LANGUAGE "Welsh"
!insertmacro MUI_LANGUAGE "Armenian"
!insertmacro MUI_LANGUAGE "Corsican"
!insertmacro MUI_LANGUAGE "Tatar"
!insertmacro MUI_LANGUAGE "Hindi"

;--------------------------------
;Reserve Files

;If you are using solid compression, files that are required before
;the actual installation should be stored first in the data block,
;because this will make your installer start faster.
!insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------------
;Installer Sections
Section "Program" SecProgram
    SetOutPath "$INSTDIR"
    File /r "..\..\artifacts\win-self-contained\*"

    WriteRegStr HKLM "Software\${NAME}" "InstallFolder" $INSTDIR

    WriteUninstaller "$InstDir\Uninstall.exe"
    WriteRegStr HKLM "${REGPATH_UNINSTSUBKEY}" "DisplayName" "${NAME}"
    WriteRegStr HKLM "${REGPATH_UNINSTSUBKEY}" "DisplayIcon" "$InstDir\CrucioBackupper.exe,0"
    WriteRegStr HKLM "${REGPATH_UNINSTSUBKEY}" "UninstallString" '"$InstDir\Uninstall.exe"'
    WriteRegStr HKLM "${REGPATH_UNINSTSUBKEY}" "QuietUninstallString" '"$InstDir\Uninstall.exe" /S'
    WriteRegDWORD HKLM "${REGPATH_UNINSTSUBKEY}" "NoModify" 1
    WriteRegDWORD HKLM "${REGPATH_UNINSTSUBKEY}" "NoRepair" 1

    WriteRegStr HKLM "Software\Classes\.dign" "" "CrucioBackupper.dign"
    WriteRegStr HKLM "Software\Classes\CrucioBackupper.dign" "" "Dialogue Novel File"
    WriteRegStr HKLM "Software\Classes\CrucioBackupper.dign\DefaultIcon" "" "$InstDir\CrucioBackupper.exe,0"
    WriteRegStr HKLM "Software\Classes\CrucioBackupper.dign\shell\open\command" "" '"$InstDir\CrucioBackupper.exe" "%1"'

    SetShellVarContext all
    CreateDirectory "$SMPROGRAMS\${NAME}"
    CreateShortCut "$SMPROGRAMS\${NAME}\CrucioBackupper.lnk" "$INSTDIR\CrucioBackupper.exe" "" ""
    CreateShortCut "$SMPROGRAMS\${NAME}\Uninstall CrucioBackupper.lnk" "$INSTDIR\Uninstall.exe" "" ""
SectionEnd

;--------------------------------
;Installer Functions
Function .onInit
    !insertmacro MUI_LANGDLL_DISPLAY
    ${If} ${RunningX64}
    !ifdef WIN64
        SetRegView 64
    !else
        MessageBox MB_OK|MB_ICONSTOP 'This is the 64 bit installer.$\r$\nYou are running the 32 bit system, for which this installer is not suitable.$\r$\nClick Ok to quit Setup.'
        Quit
    !endif
    ${EndIf}
FunctionEnd

;--------------------------------
;Uninstaller Section
Section "Uninstall"
    SetShellVarContext all
    RMDir /r "$SMPROGRAMS\${NAME}"
    RMDir /r "$INSTDIR"
    DeleteRegKey HKLM "Software\Classes\CrucioBackupper.dign"
    DeleteRegKey HKLM "Software\Classes\.dign"
    DeleteRegKey HKLM "${REGPATH_UNINSTSUBKEY}"
    DeleteRegKey HKLM "Software\${NAME}"
SectionEnd

;--------------------------------
;Uninstaller Functions
Function un.onInit
    !insertmacro MUI_UNGETLANGUAGE
FunctionEnd