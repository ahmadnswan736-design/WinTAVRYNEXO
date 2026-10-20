@echo off
echo ==========================================================
echo 🚀 Building WinTAVRYNEXO Native C# .NET 8 Appx/MSIX Package
echo 👤 Publisher: Radwan Almsora
echo 🔑 Package Name: RadwanAlmsora.WinTAVRYNEXO
echo 🏷️ Publisher ID: CN=DB86E487-92CC-4FB9-BDBC-F161416E3A4E
echo 🆔 Store ID: 9NTD5KLHN1X6
echo ==========================================================

dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
echo.
echo ✅ تم بناء ملف WinTAVRYNEXO.exe الأصلي بنجاح!
pause
