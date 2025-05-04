@echo off
echo ===================================================
echo      AspireMCP Server - Test Script
echo ===================================================
echo.

SET SERVER_URL=http://localhost:5000/mcp
SET TEST_SCRIPTS_DIR=%~dp0TestScripts

echo Step 1: Checking if AspireMCP server is running...
curl -s -o nul -w "%%{http_code}" %SERVER_URL%/capabilities > temp.txt
set /p STATUS=<temp.txt
del temp.txt

if not "%STATUS%"=="200" (
    echo Error: AspireMCP server does not appear to be running at %SERVER_URL%
    echo Please start the server using run-aspire-mcp.bat before running tests.
    exit /b 1
)
echo Server is running. Status code: %STATUS%
echo.

echo Step 2: Setting up test environment...
cd %TEST_SCRIPTS_DIR%

:: Check if Node.js dependencies are installed
if not exist "node_modules" (
    echo Installing Node.js dependencies...
    npm install
    if %ERRORLEVEL% neq 0 (
        echo Error: Failed to install Node.js dependencies
        exit /b %ERRORLEVEL%
    )
    echo Dependencies installed successfully.
) else (
    echo Node.js dependencies already installed.
)
echo.

echo Step 3: Running tests against %SERVER_URL%...
node test-mcp-server.js --server=%SERVER_URL%
SET TEST_RESULT=%ERRORLEVEL%

echo.
echo ===================================================
echo Test Summary
echo ===================================================
if %TEST_RESULT% equ 0 (
    echo All tests passed successfully!
) else (
    echo Some tests failed. See the output above for details.
)

exit /b %TEST_RESULT%