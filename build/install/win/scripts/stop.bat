PUSHD %~dp0..
call runasadmin.bat "%~dpnx0"

if %errorlevel% == 0 (
	for /R "run_new\" %%f in (*.bat) do (
		call nssm stop Onlyoffice%%~nf
	)
)

call nssm stop nginx