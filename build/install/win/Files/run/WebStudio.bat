echo "RUN ASC.Web.Studio"
call dotnet ..\Services\ASC.Web.Studio\ASC.Web.Studio.dll --urls=http://0.0.0.0:5003 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=studio