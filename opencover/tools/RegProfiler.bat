@rem run as recommended in usage.rtf
@echo off
pushd %~dp0 &:: @http://stackoverflow.com/questions/672693/windows-batch-file-starting-directory-when-run-as-admin
goto check_Permissions
:process
regsvr32 x86\OpenCover.Profiler.dll
regsvr32 x64\OpenCover.Profiler.dll
@pause
goto end
:check_Permissions
    @rem  https://stackoverflow.com/questions/4051883/batch-script-how-to-check-for-admin-rights#11995662
    net session >nul 2>&1
    if %errorLevel% == 0 (
        @rem echo Success: Administrative permissions confirmed.
          goto process
    ) else (
        echo Administrative permissions required.
    )
    pause >nul
:end

pause