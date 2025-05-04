@echo off
echo ===================================================
echo      AspireMCP Server - Build and Run Script
echo ===================================================
echo.

SET ASPIRE_API_DIR=%~dp0AspireAPI

echo Step 1: Building AspireAPI project...
cd %ASPIRE_API_DIR%
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Error: Build failed with exit code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
echo Build completed successfully.
echo.

echo Step 2: Starting AspireMCP server...
echo Server URL: http://localhost:5000/mcp
echo.
echo Press Ctrl+C to stop the server.
echo To test the server, open another command prompt and run test-server.bat
echo.

cd %ASPIRE_API_DIR%
dotnet run

echo Server stopped.
exit /b 0