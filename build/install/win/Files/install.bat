PUSHD %~dp0

::call runasadmin.bat "%~dpnx0"

if %errorlevel% == 0 (
	for /R "run\" %%f in (*.bat) do (
		call nssm install Onlyoffice%%~nf "%%~f"
	)
)

call nssm install OnlyofficeProxy "%cd%\Services\OnlyofficeProxy\nginx.exe"


:: Configure logging

mkdir "C:\Logs\OnlyOfficeLogs"

if %errorlevel% == 0 (
	for /R "run\" %%f in (*.bat) do (
		call nssm set Onlyoffice%%~nf AppStdout "C:\Logs\OnlyOfficeLogs\Onlyoffice%%~nfStdout.log"
                call nssm set Onlyoffice%%~nf AppStderr "C:\Logs\OnlyOfficeLogs\Onlyoffice%%~nfStderr.log"
	)
)

call nssm set OnlyofficeProxy AppStdout "C:\Logs\OnlyOfficeLogs\nginx_out.log"
call nssm set OnlyofficeProxy AppStderr "C:\Logs\OnlyOfficeLogs\nginx_err.log"


if %errorlevel% == 0 (
	for /R "run\" %%f in (*.bat) do (
		call nssm start Onlyoffice%%~nf
	)
)

call nssm start OnlyofficeProxy


pause