PUSHD %~dp0

call runasadmin.bat "%~dpnx0"

if %errorlevel% == 0 (
	for /R "Files\run\" %%f in (*.bat) do (
		call nssm remove Onlyoffice%%~nf confirm
	)
)

call nssm remove OnlyofficeProxy confirm

