echo "RUN ASC.Files"
call dotnet ..\Services\ASC.Files\ASC.Files.dll --urls=http://0.0.0.0:5007 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=files