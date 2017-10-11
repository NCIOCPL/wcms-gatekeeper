@echo off
rem Do a backup first.
powershell -ExecutionPolicy RemoteSigned ./gkBackup.ps1
setLocal
rem Build up the source location from the current execution path and
rem an assumed DATA subdirectory.
set filepath=%~dp0
powershell -ExecutionPolicy RemoteSigned ./gkDeploy.ps1 %filepath%
pause