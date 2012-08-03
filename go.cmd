@echo off
if '%1a'=='a' goto :usage

	powershell -inputformat none -NoProfile -ExecutionPolicy unrestricted -command ". '.\packages.ps1'; Setup-Bundled-Modules; return Invoke-Psake %1 -framework 4.0x86"
	if %ERRORLEVEL% == 0 goto :eof
	exit /B %ERRORLEVEL%
	goto :eof

:usage
	echo Usage: .\go.cmd target
	echo Available targets:
	echo 	publish
	echo 	package
	echo 	install "target"
:eof
	  