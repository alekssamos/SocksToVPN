@echo off
REM Batch script to build for different runtime targets

echo Creating output directory...
if not exist publish mkdir publish

echo Building for win-x64...
dotnet publish -c Release -r win-x64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\win-x64
if %ERRORLEVEL% NEQ 0 echo Build failed for win-x64

echo Building for win-x86...
dotnet publish -c Release -r win-x86 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\win-x86
if %ERRORLEVEL% NEQ 0 echo Build failed for win-x86

echo Building for win-arm64...
dotnet publish -c Release -r win-arm64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\win-arm64
if %ERRORLEVEL% NEQ 0 echo Build failed for win-arm64

echo Building for linux-x64...
dotnet publish -c Release -r linux-x64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\linux-x64
if %ERRORLEVEL% NEQ 0 echo Build failed for linux-x64

echo Building for linux-musl-x64...
dotnet publish -c Release -r linux-musl-x64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\linux-musl-x64
if %ERRORLEVEL% NEQ 0 echo Build failed for linux-musl-x64

echo Building for linux-arm64...
dotnet publish -c Release -r linux-arm64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\linux-arm64
if %ERRORLEVEL% NEQ 0 echo Build failed for linux-arm64

echo Building for linux-musl-arm64...
dotnet publish -c Release -r linux-musl-arm64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\linux-musl-arm64
if %ERRORLEVEL% NEQ 0 echo Build failed for linux-musl-arm64

echo Building for osx-x64...
dotnet publish -c Release -r osx-x64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\osx-x64
if %ERRORLEVEL% NEQ 0 echo Build failed for osx-x64

echo Building for osx-arm64...
dotnet publish -c Release -r osx-arm64 --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o publish\osx-arm64
if %ERRORLEVEL% NEQ 0 echo Build failed for osx-arm64

echo All builds completed!
pause
