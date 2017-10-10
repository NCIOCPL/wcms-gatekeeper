@echo off
setlocal

@set PATH=c:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\BIN;c:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\Tools;c:\Windows\Microsoft.NET\Framework\v3.5;c:\Windows\Microsoft.NET\Framework\v2.0.50727;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\VCPackages;%PATH%
@set INCLUDE=c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\ATLMFC\INCLUDE;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\INCLUDE;%INCLUDE%
@set LIB=c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\ATLMFC\LIB;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\LIB;%LIB%
@set LIBPATH=c:\Windows\Microsoft.NET\Framework\v3.5;c:\Windows\Microsoft.NET\Framework\v2.0.50727;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\ATLMFC\LIB;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\LIB;%LIBPATH%

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
	ECHO You must pass the branch and build environment names and to this script.
	ECHO USAGE:
	ECHO 	c:\Build.bat ^<branch^> ^<environment^>
	ECHO Additonally, the following environment variables must be set:
	ECHO 	WORKSPACE - directory containing the source code.
	ECHO	TEMP - Location for temporary files
	ECHO	BUILD_NUMBER - Build number ^(automatically generated/set by Jenkins^)
	ECHO	GITHUB_TOKEN - GitHub access token for the build user.
	GOTO :EOF
)

REM Determine the current Git commit hash.
FOR /f %%a IN ('git rev-parse --verify HEAD') DO SET COMMIT_ID=%%a

REM Insert placeholder configuration files.
call "%WORKSPACE%\tools\build\insert-placeholders.bat"

msbuild /fileLogger /t:ALL "/p:TargetEnvironment=%my_environ%;Branch=%my_branch%" "%WORKSPACE%\tools\build\build-gatekeeper.xml"