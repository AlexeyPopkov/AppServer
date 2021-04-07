PUSHD %~dp0..
call runasadmin.bat "%~dpnx0"

echo on

if %errorlevel% == 0 (
	for /R "run_new\" %%f in (*.bat) do (
		call nssm start Onlyoffice%%~nf
	)
)

call nssm start nginx