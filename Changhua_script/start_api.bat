@echo off

:: 定義源路徑和目標路徑
set SOURCE_PATH=C:\Users\imsadmin\Desktop\appsettings
set DESTINATION_PATH=C:\stock_api\output\net6.0\win-x64

:: 刪除原先的appsettings.json文件（如果存在）
if exist "appsettings.json" del "appsettings.json"

:: 將appsettings_KimForest.json重命名為appsettings.json
ren "appsettings_KimForest.json" "appsettings.json"

:: 切換到目標目錄並運行 exe 文件
cd "%DESTINATION_PATH%"
start "" "stock_api.exe"
