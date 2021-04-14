echo "RUN ASC.Backup"
call dotnet ..\Services\ASC.Data.Backup\ASC.Data.Backup.dll --urls=http://0.0.0.0:5012 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=backup