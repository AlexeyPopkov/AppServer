echo "RUN ASC.People"
call dotnet ..\Services\ASC.People\ASC.People.dll --urls=http://0.0.0.0:5004 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=people