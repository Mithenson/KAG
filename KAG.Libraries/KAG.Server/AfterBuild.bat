echo on

echo Copying shared libraries to client solution...
xcopy .\bin\Debug\KAG.Shared*.dll .\..\..\KAG.Unity\Assets\Core\Libraries /y

echo Copying libraries to DarkRift...
xcopy .\bin\Debug\*.dll .\..\..\KAG.DarkRift\Plugins /y /EXCLUDE:DarkRiftCopyExclusions.txt

echo Zipping DarkRift...
del .\..\..\KAG.DarkRift\DarkRift.Server.Console.zip
"C:\Program Files\7-Zip\7z.exe" a -tzip .\..\..\KAG.DarkRift\DarkRift.Server.Console.zip .\..\..\KAG.DarkRift\*

echo Done!