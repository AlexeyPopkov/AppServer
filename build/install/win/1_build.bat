:: Переход в корневую директорию проекта
PUSHD %~dp0..\..\..

echo "ASC.Web.Components"
call build\install\win\scripts\build\components.sh

echo "ASC.Web.Common"
call build\install\win\scripts\build\common.sh

echo "ASC.Web.Client"
call build\install\win\scripts\build\client.sh

echo "ASC.Web.People.Client"
call build\install\win\scripts\build\people.sh

echo "ASC.Web.Files.Client"
call build\install\win\scripts\build\files.sh

echo "ASC.UrlShortener"
call build\install\win\scripts\build\urlshortener.sh

echo "ASC.Thumbnails"
call build\install\win\scripts\build\thumbnails.sh

echo "ASC.Socket.IO"
call build\install\win\scripts\build\socket.sh

echo "ASC.Web.sln"
dotnet build ASC.Web.sln --configuration Release /fl1 /flp1:LogFile=build\install\win\Logs\build_ASC.Web.sln.log;Verbosity=Normal

pause
