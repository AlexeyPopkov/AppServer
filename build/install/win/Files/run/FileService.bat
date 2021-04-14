echo "RUN ASC.Files"
call dotnet ..\Services\ASC.Files.Service\ASC.Files.Service.dll --urls=http://0.0.0.0:5009 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=files