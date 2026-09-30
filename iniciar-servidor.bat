@echo off
setlocal
rem RobloxServer - inicia o site NESTE PC com o IIS Express.
rem Uso: iniciar-servidor.bat [porta]      (padrao: 8080)
rem Depois abra http://localhost:8080/ no navegador e use "localhost:8080" no launcher.
rem Para outros PCs da rede / Raspberry Pi use o IIS completo (docs\setting-up.md).

set PORT=%~1
if "%PORT%"=="" set PORT=8080
set SITE=%~dp0RobloxServer.Web

if exist "%SITE%\bin\RobloxServer.dll" goto run

echo Compilando o RobloxServer...
set VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
if not exist "%VSWHERE%" goto nomsbuild
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set MSBUILD=%%i
if not defined MSBUILD goto nomsbuild

"%MSBUILD%" "%~dp0RobloxServer.sln" /nologo /v:minimal /p:Configuration=Release
if not errorlevel 1 goto run
echo Sem o pacote do .NET Framework 4.6, tentando compilar para o 4.8...
"%MSBUILD%" "%~dp0RobloxServer.sln" /nologo /v:minimal /p:Configuration=Release /p:TargetFrameworkVersion=v4.8
if errorlevel 1 goto buildfailed

:run
set IISEXPRESS=%ProgramFiles%\IIS Express\iisexpress.exe
if not exist "%IISEXPRESS%" set IISEXPRESS=%ProgramFiles(x86)%\IIS Express\iisexpress.exe
if not exist "%IISEXPRESS%" goto noiisexpress

echo.
echo RobloxServer rodando em http://localhost:%PORT%/
echo No launcher, use o endereco: localhost:%PORT%
echo Feche esta janela (ou aperte Q) para parar o servidor.
echo.
start "" cmd /c "timeout /t 4 >nul & start http://localhost:%PORT%/"
"%IISEXPRESS%" /path:"%SITE%" /port:%PORT% /clr:v4.0
goto end

:nomsbuild
echo.
echo O site ainda nao foi compilado e o MSBuild nao foi encontrado.
echo Opcao 1: baixe o site ja compilado em Actions ^> ultima execucao ^> Artifacts ^> RobloxServer-site
echo Opcao 2: instale o Visual Studio (ou Build Tools) com "ASP.NET and web development".
goto fail

:buildfailed
echo.
echo A compilacao falhou. Veja as mensagens acima.
goto fail

:noiisexpress
echo.
echo IIS Express nao encontrado. Baixe em https://www.microsoft.com/download/details.aspx?id=48264
echo (ou use o IIS completo, veja docs\setting-up.md).
goto fail

:fail
pause
exit /b 1

:end
endlocal
