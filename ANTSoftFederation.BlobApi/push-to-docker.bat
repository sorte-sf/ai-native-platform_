@echo off
setlocal enabledelayedexpansion

:: Major.minor
set BASE_VERSION=2.1

:: Date/time for uniqueness
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set ldt=%%I
set BUILD_DATE=!ldt:~0,8!
set BUILD_TIME=!ldt:~8,4!
set VERSION_TAG=%BASE_VERSION%.%BUILD_DATE%.%BUILD_TIME%

echo Docker tag: %VERSION_TAG%

dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer -p:ContainerImageTag=%VERSION_TAG%
endlocal
