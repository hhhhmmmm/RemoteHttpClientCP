nuget update -self
mkdir ..\NuGetPackages

rem call set_nuget_local_repository_path.bat

rem dotnet pack RemoteHttpClientCP.csproj -c Release
rem dotnet pack RemoteHttpClientCP.csproj -c Release --force -o .\

dotnet pack RemoteHttpClientCP.csproj  -c Release --include-symbols  -o ..\\NuGetPackages --force  -p:PackageVersion=20.09.02.0


