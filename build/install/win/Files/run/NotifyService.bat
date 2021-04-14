echo "RUN ASC.Notify"
call dotnet ..\Services\ASC.Notify\ASC.Notify.dll --urls=http://0.0.0.0:5005 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=notify
