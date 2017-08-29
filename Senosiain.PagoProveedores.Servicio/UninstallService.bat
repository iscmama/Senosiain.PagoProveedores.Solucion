@ECHO OFF
 
REM The following directory is for .NET 2.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%
 
echo Desistalando Servicio en el Servidor...
echo ---------------------------------------------------

installutil.exe /u "D:\InventivaGS\2014\codigo\Senosiain.Solucion\Senosiain.PagoProveedores.Servicio\bin\Debug\Senosiain.PagoProveedores.Servicio.exe"
echo ---------------------------------------------------
echo Completado.
pause