echo "RUN ASC.Notify"
call dotnet ..\Services\ASC.Notify\ASC.Notify.dll --urls=http://0.0.0.0:5005 --pathToConf=..\..\Files\config  --$STORAGE_ROOT=..\..\Files\Data --log__dir=..\..\Logs --log__name=notify
