echo "RUN ASC.Notify"
call dotnet ..\Services\ASC.ApiSystem\ASC.ApiSystem.dll --urls=http://0.0.0.0:5010 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=apisystem