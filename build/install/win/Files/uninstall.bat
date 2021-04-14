PUSHD %~dp0

if %errorlevel% == 0 (
	for /R "run\" %%f in (*.bat) do (
		call nssm stop Onlyoffice%%~nf
	)
)

call nssm stop OnlyofficeProxy

if %errorlevel% == 0 (
	for /R "run\" %%f in (*.bat) do (
		call nssm remove Onlyoffice%%~nf confirm
	)
)

call nssm remove OnlyofficeProxy confirm


:: Remove log dir

rmdir /Q /S "C:\Logs\OnlyOfficeLogs"

pause