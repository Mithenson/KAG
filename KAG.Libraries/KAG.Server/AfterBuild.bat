echo on

echo Copying shared libraries to client solution...
xcopy .\bin\Debug\KAG.Shared*.dll .\..\..\KAG.Unity\Assets\Core\Libraries /y

echo Copying libraries to DarkRift...
xcopy .\bin\Debug\*.dll .\..\..\KAG.DarkRift\Plugins /y /EXCLUDE:DarkRiftCopyExclusions.txt

echo Done!