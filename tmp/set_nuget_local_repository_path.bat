@echo off
setlocal enableextensions
if errorlevel 1 echo error: unable to enable extensions

if not defined LocalNugetPackagesPath (
echo ��⠭���� ���...
set LocalNugetPackagesPath=D:\CrossPlatform\LocalNugetPackages
)

echo ���� LocalNugetPackagesPath: %LocalNugetPackagesPath%
mkdir %LocalNugetPackagesPath%

