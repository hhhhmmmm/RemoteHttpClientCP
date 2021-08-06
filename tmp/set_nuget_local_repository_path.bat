@echo off
setlocal enableextensions
if errorlevel 1 echo error: unable to enable extensions

if not defined LocalNugetPackagesPath (
echo Установка пути...
set LocalNugetPackagesPath=D:\CrossPlatform\LocalNugetPackages
)

echo Путь LocalNugetPackagesPath: %LocalNugetPackagesPath%
mkdir %LocalNugetPackagesPath%

