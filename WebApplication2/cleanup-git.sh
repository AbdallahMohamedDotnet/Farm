#!/bin/bash

echo "?? Git Repository Cleanup Script"
echo "================================"
echo ""

echo "?? Current directory: $(pwd)"
echo ""

echo "?? Checking Git status..."
git status --porcelain
echo ""

echo "?? Removing .vs folder from Git tracking..."
if git rm -r --cached .vs/ 2>/dev/null; then
    echo "? .vs folder removed from Git tracking"
else
    echo "??  .vs folder not tracked or already removed"
fi
echo ""

echo "?? Removing other Visual Studio temp files from Git tracking..."
git rm -r --cached bin/ 2>/dev/null || echo "bin/ not tracked"
git rm -r --cached obj/ 2>/dev/null || echo "obj/ not tracked"
git rm --cached *.user 2>/dev/null || echo "No .user files tracked"
git rm --cached *.suo 2>/dev/null || echo "No .suo files tracked"
echo ""

echo "?? Adding .gitignore..."
git add .gitignore
echo ""

echo "?? Adding project files (excluding ignored files)..."
git add .
echo ""

echo "?? Checking what will be committed..."
git status
echo ""

echo "?? Ready to commit! You can now run:"
echo "   git commit -m \"Add .gitignore and clean up Visual Studio temp files\""
echo ""