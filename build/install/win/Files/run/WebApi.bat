echo "RUN ASC.Web.Api"
call dotnet ..\Services\ASC.Web.Api\ASC.Web.Api.dll --urls=http://0.0.0.0:5000 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=api