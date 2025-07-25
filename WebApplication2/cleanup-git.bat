@echo off
echo ?? Git Repository Cleanup Script
echo ================================

echo.
echo ?? Current directory: %CD%
echo.

echo ?? Checking Git status...
git status --porcelain

echo.
echo ?? Removing .vs folder from Git tracking...
git rm -r --cached .vs/ 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ??  .vs folder not tracked or already removed
) else (
    echo ? .vs folder removed from Git tracking
)

echo.
echo ?? Removing other Visual Studio temp files from Git tracking...
git rm -r --cached bin/ 2>nul
git rm -r --cached obj/ 2>nul
git rm --cached *.user 2>nul
git rm --cached *.suo 2>nul

echo.
echo ?? Adding .gitignore...
git add .gitignore

echo.
echo ?? Adding project files (excluding ignored files)...
git add .

echo.
echo ?? Checking what will be committed...
git status

echo.
echo ?? Ready to commit! You can now run:
echo    git commit -m "Add .gitignore and clean up Visual Studio temp files"
echo.

pause