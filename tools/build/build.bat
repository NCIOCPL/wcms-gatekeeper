@echo off
setlocal

@set PATH=C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow;C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\;C:\Program Files (x86)\Microsoft SDKs\TypeScript\1.0;C:\Program Files (x86)\MSBuild\12.0\bin;C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\;C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\BIN;C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools;C:\Windows\Microsoft.NET\Framework\v4.0.30319;C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\VCPackages;C:\Program Files (x86)\HTML Help Workshop;C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Performance Tools;C:\Program Files (x86)\Windows Kits\8.1\bin\x86;C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\;%PATH%
@set INCLUDE=C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\INCLUDE;C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\ATLMFC\INCLUDE;C:\Program Files (x86)\Windows Kits\8.1\include\shared;C:\Program Files (x86)\Windows Kits\8.1\include\um;C:\Program Files (x86)\Windows Kits\8.1\include\winrt;
@set LIB=C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\LIB;C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\ATLMFC\LIB;C:\Program Files (x86)\Windows Kits\8.1\lib\winv6.3\um\x86;
@set LIBPATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319;C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\LIB;C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\ATLMFC\LIB;C:\Program Files (x86)\Windows Kits\8.1\References\CommonConfiguration\Neutral;C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1\ExtensionSDKs\Microsoft.VCLibs\12.0\References\CommonConfiguration\neutral;

REM Get the branch from the command line
set my_branch=%1

REM Determine the Build Environment Name.  This is used for tagging and proper config deployment

set my_environ=%2

REM Validate Branch and Target
set FAIL=
IF "%my_branch%"=="" set FAIL=True
IF "%my_environ%"=="" set FAIL=True
IF "%WORKSPACE%"=="" set FAIL=True
IF "%TEMP%"=="" set FAIL=True
IF "%BUILD_NUMBER%"=="" set FAIL=True
IF "%GITHUB_TOKEN%"=="" set FAIL=True

IF "%FAIL%" NEQ "" (
	ECHO.
	ECHO You must pass the branch and build environment names to this script.
	ECHO.
	ECHO USAGE:
	ECHO 	c:\Build.bat ^<branch^> ^<environment^>
	ECHO.
	ECHO Additonally, the following environment variables must be set:
	ECHO.
	ECHO 	WORKSPACE - directory containing the source code.
	ECHO 	TEMP - Location for temporary files
	ECHO 	BUILD_NUMBER - Build number ^(automatically generated/set by Jenkins^)
	ECHO 	GITHUB_TOKEN - GitHub access token for the build user.
	GOTO :EOF
)

REM Determine the current Git commit hash.
FOR /f %%a IN ('git rev-parse --verify HEAD') DO SET COMMIT_ID=%%a

REM Insert placeholder configuration files.
call "%WORKSPACE%\tools\build\insert-placeholders.bat"

.nuget\NuGet.exe restore
msbuild /fileLogger /t:ALL "/p:TargetEnvironment=%my_environ%;Branch=%my_branch%" "%WORKSPACE%\tools\build\build-gatekeeper.xml"