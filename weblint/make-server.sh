HOME=/Users/dhowe/git/dialogic

DIA=$HOME/dialogic
LNT=$HOME/weblint
CSC=/Library/Frameworks/Mono.framework/Versions/Current/Commands/csc

# build dialogic
rm $DIA/bin/Debug/Dialogic.*
$CSC /noconfig /nowarn:1701,1702,2008 /langversion:4 /nostdlib+ /errorreport:prompt /warn:4 /define:DEBUG /errorendlocation /preferreduilang:en-US /highentropyva- /reference:/Library/Frameworks/Mono.framework/Versions/5.8.1/lib/mono/2.0-api/mscorlib.dll /reference:/Library/Frameworks/Mono.framework/Versions/5.8.1/lib/mono/2.0-api/System.Core.dll /reference:/Library/Frameworks/Mono.framework/Versions/5.8.1/lib/mono/2.0-api/System.dll /debug+ /debug:portable /optimize- /out:$DIA/bin/Debug/Dialogic.dll /target:library /utf8output /langversion:4 $DIA/Properties/AssemblyInfo.cs $DIA/*.cs

ls -l $DIA/bin/Debug
echo =============================================================

# build weblint
rm $LNT/bin/Debug/weblint.*

$CSC /noconfig /nowarn:1701,1702,2008 /langversion:4 /nostdlib+ /platform:anycpu32bitpreferred /errorreport:prompt /warn:4 /define:DEBUG /errorendlocation /preferreduilang:en-US /highentropyva+ /reference:/$HOME/dialogic/bin/Debug/Dialogic.dll /reference:/Library/Frameworks/Mono.framework/Versions/5.8.1/lib/mono/4.5-api/mscorlib.dll /reference:/Library/Frameworks/Mono.framework/Versions/5.8.1/lib/mono/4.5-api/System.Core.dll /reference:/Library/Frameworks/Mono.framework/Versions/5.8.1/lib/mono/4.5-api/System.dll /debug+ /debug:portable /optimize- /out:$LNT/bin/Debug/weblint.exe /subsystemversion:6.00 /target:exe /utf8output /langversion:4 $LNT/Properties/AssemblyInfo.cs $LNT/LintServer.cs 

ls -l $LNT/bin/Debug


