@REM dotnet build  --source .\stock_api\ --output .\output\

dotnet publish --self-contained --runtime win-x64 -p:DebugType=Full -p:DebugSymbols=true
