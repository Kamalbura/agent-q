@echo off
REM RunAstra.bat - Build and run Assistant.UI
set SOLUTION=Assistant.sln
set PROJECT=Assistant.UI
set CONFIG=Debug

echo Restoring solution...
dotnet restore %SOLUTION%

echo Building solution...
dotnet build %SOLUTION% -c %CONFIG%

echo Running project...
dotnet run --project %PROJECT% --configuration %CONFIG%
