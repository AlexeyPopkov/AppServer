echo "RUN ASC.Projects"
call dotnet ..\Services\ASC.Projects\ASC.Projects.dll --urls=http://0.0.0.0:5020 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=projects