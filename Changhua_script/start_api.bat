@echo off

:: 定義源路徑和目標路徑
set DESTINATION_PATH=C:\stock_api\output\net6.0\win-x64

:: 刪除原先的appsettings.json文件（如果存在）
if exist "%DESTINATION_PATH%\appsettings.json" (
    del "%DESTINATION_PATH%\appsettings.json"
    echo appsettings.json deleted
) else (
    echo appsettings.json not exist
)

:: 將appsettings_KimForest.json重命名為appsettings.json
if exist "%DESTINATION_PATH%\appsettings_KimForest.json" (
    copy "%DESTINATION_PATH%\appsettings_KimForest.json" "%DESTINATION_PATH%\appsettings.json"
    echo copied appsettings_KimForest.json to appsettings.json
) else (
    echo appsettings_KimForest.json not exist
    pause
    exit /b 1
)

:: 切換到目標目錄並運行 exe 文件
cd "%DESTINATION_PATH%"
start "" "stock_api.exe"

pause
