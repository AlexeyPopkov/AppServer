:: Очищаем папку Files\Services
:: Поддержка длинных имен файлов при удалении: https://superuser.com/a/1048242/71768
echo "Purge Files\Services folder"
mkdir %~dp0$emptyfolder
robocopy %~dp0$emptyfolder %~dp0Files\Services /purge > nul
rmdir %~dp0$emptyfolder

:: Переход в корневую директорию проекта
PUSHD %~dp0..\..\..
echo %cd%

dotnet publish common\services\ASC.ApiSystem\ASC.ApiSystem.csproj -c Release -o build\install\win\Files\Services\ASC.ApiSystem
dotnet publish common\services\ASC.Data.Backup\ASC.Data.Backup.csproj -c Release -o build\install\win\Files\Services\ASC.Data.Backup
dotnet publish products\ASC.CRM\Server\ASC.CRM.csproj --no-build -c Release -o build\install\win\Files\Services\ASC.CRM
dotnet publish common\services\ASC.Data.Storage.Encryption\ASC.Data.Storage.Encryption.csproj -c Release -o build\install\win\Files\Services\ASC.Data.Storage.Encryption
dotnet publish products\ASC.Files\Server\ASC.Files.csproj -c Release -o build\install\win\Files\Services\ASC.Files
dotnet publish products\ASC.Files\Service\ASC.Files.Service.csproj -c Release -o build\install\win\Files\Services\ASC.Files.Service
dotnet publish common\services\ASC.Data.Storage.Migration\ASC.Data.Storage.Migration.csproj -c Release -o build\install\win\Files\Services\ASC.Data.Storage.Migration
dotnet publish common\services\ASC.Notify\ASC.Notify.csproj -c Release -o build\install\win\Files\Services\ASC.Notify
dotnet publish products\ASC.People\Server\ASC.People.csproj -c Release -o build\install\win\Files\Services\ASC.People
dotnet publish products\ASC.Projects\Server\ASC.Projects.csproj -c Release -o build\install\win\Files\Services\ASC.Projects
dotnet publish common\services\ASC.Socket.IO.Svc\ASC.Socket.IO.Svc.csproj -c Release -o build\install\win\Files\Services\ASC.Socket.IO.Svc
dotnet publish common\services\ASC.Studio.Notify\ASC.Studio.Notify.csproj -c Release -o build\install\win\Files\Services\ASC.Studio.Notify
dotnet publish common\services\ASC.TelegramService\ASC.TelegramService.csproj -c Release -o build\install\win\Files\Services\ASC.TelegramService
dotnet publish common\services\ASC.Thumbnails.Svc\ASC.Thumbnails.Svc.csproj -c Release -o build\install\win\Files\Services\ASC.Thumbnails.Svc
dotnet publish common\services\ASC.UrlShortener.Svc\ASC.UrlShortener.Svc.csproj -c Release -o build\install\win\Files\Services\ASC.UrlShortener.Svc
dotnet publish web\ASC.Web.Api\ASC.Web.Api.csproj -c Release -o build\install\win\Files\Services\ASC.Web.Api
dotnet publish web\ASC.Web.Studio\ASC.Web.Studio.csproj -c Release -o build\install\win\Files\Services\ASC.Web.Studio

echo "Publish ASC.People project"
robocopy products\ASC.People\Client\ build\install\win\Files\Services\ASC.People.Client\ /MIR > nul

echo "Publish ASC.Web.Client project"
robocopy web\ASC.Web.Client\ build\install\win\Files\Services\ASC.Web.Client\ /MIR > nul

echo "Publish ASC.UrlShortener.Svc.csproj project"
robocopy common\ASC.UrlShortener\ build\install\win\Files\Services\ASC.UrlShortener\ /MIR > nul

echo "Publish ASC.Files.Client"
robocopy products\ASC.Files\Client\ build\install\win\Files\Services\ASC.Files.Client\ /MIR > nul

robocopy build\install\win\utils\OnlyofficeProxy build\install\win\Files\Services\OnlyofficeProxy\ /MIR > nul