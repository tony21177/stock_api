@REM dotnet build  --source .\stock_api\ --output .\output\

@REM dotnet publish --self-contained --runtime win-x64 -p:DebugType=Full -p:DebugSymbols=true

dotnet publish -c Release --self-contained --runtime win-x64 -p:PublishReadyToRun=true -p:PublishTrimmed=false
