echo "RUN ASC.CRM"
call dotnet ..\Services\ASC.CRM\ASC.CRM.dll --urls=http://0.0.0.0:5021 --pathToConf=..\config  --$STORAGE_ROOT=..\Data --log__dir=C:\Logs --log__name=crm