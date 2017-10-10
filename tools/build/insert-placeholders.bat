@echo off
SETLOCAL

echo on

REM Insert placeholders for web.config / app.config
set placeholder=%WORKSPACE%\tools\build\resources\placeholder.config

REM Applications
copy "%placeholder%" "App\Admin\web.config"
copy "%placeholder%" "App\CDRPreviewWS\web.config"
copy "%placeholder%" "App\WebSvc\web.config"
copy "%placeholder%" "App\ProcMgr\app.config"

REM Test Harnesses
copy "%placeholder%" "Test Harnesses\PromotionTester\app.config"
copy "%placeholder%" "Test Harnesses\UnitTest\UnitTest.dll.config"

REM Fake the shared connectionStrings config files.
for %%a in (Admin ProcMgr WebSvc CdrPreviewWS) do (
	mkdir "%WORKSPACE%\app\%%a\sharedconfig"
	copy "%placeholder%" "%WORKSPACE%\app\%%a\sharedconfig\connectionStrings.config"
)