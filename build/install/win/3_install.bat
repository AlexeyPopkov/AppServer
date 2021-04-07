PUSHD %~dp0

call runasadmin.bat "%~dpnx0"

if %errorlevel% == 0 (
	for /R "Files\run\" %%f in (*.bat) do (
		call nssm install Onlyoffice%%~nf "%%~f"
	)
)

call nssm install OnlyofficeProxy "%cd%\Files\Services\OnlyofficeProxy\nginx.exe"


:: Configure logging

if %errorlevel% == 0 (
	for /R "Files\run\" %%f in (*.bat) do (
		call nssm set Onlyoffice%%~nf AppStdout "%cd%\Logs\OnlyOfficeLogs\Onlyoffice%%~nfStdout.log"
                call nssm set Onlyoffice%%~nf AppStderr "%cd%\Logs\OnlyOfficeLogs\Onlyoffice%%~nfStderr.log"
	)
)

call nssm set OnlyofficeProxy AppStdout "%cd%\Logs\OnlyOfficeLogs\nginx_out.log"
call nssm set OnlyofficeProxy AppStderr "%cd%\Logs\OnlyOfficeLogs\nginx_err.log"