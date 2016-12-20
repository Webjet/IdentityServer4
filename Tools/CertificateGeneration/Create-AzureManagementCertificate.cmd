@echo off
set CertName=marcel-dempers-development2

for /f "tokens=3*" %%i in ('reg query "HKLM\Software\Microsoft\Microsoft SDKs\Windows" /v CurrentInstallFolder') do set SdkPath=%%i %%j
if not [%SdkPath:~-1%]==[\] set SdkPath=%SdkPath%\
"C:\Program Files (x86)\Fiddler2\makecert.exe" -r -pe -a sha1 -n "CN=%CertName%" -sky exchange -m 240 -len 2048 -ss My "%~dp0%CertName%.cer"
if errorlevel 1 echo Makecert failed & exit /b %errorlevel%

powershell -command ^& "%~dp0Export-CertificatePrivateKey.ps1" -Path "%~dp0%CertName%.cer" -Destination "%~dp0%CertName%.p12"
if errorlevel 1 echo Export-CertificatePrivateKey failed & exit /b %errorlevel%

@pause