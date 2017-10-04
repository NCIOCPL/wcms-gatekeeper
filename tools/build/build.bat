@set PATH=c:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\BIN;c:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\Tools;c:\Windows\Microsoft.NET\Framework\v3.5;c:\Windows\Microsoft.NET\Framework\v2.0.50727;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\VCPackages;%PATH%
@set INCLUDE=c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\ATLMFC\INCLUDE;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\INCLUDE;%INCLUDE%
@set LIB=c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\ATLMFC\LIB;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\LIB;%LIB%
@set LIBPATH=c:\Windows\Microsoft.NET\Framework\v3.5;c:\Windows\Microsoft.NET\Framework\v2.0.50727;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\ATLMFC\LIB;c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\LIB;%LIBPATH%

msbuild build-gatekeeper.xml /t:ALL
pause