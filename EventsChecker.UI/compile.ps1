param(
)

$compilator = $(
  if (test-path 'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe') { 
    'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  } else { 
    'd:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  })
  
if ($true) {
  $res = "D:\temp\EventsNotifier\EventsChecker.UI\bin\Debug\EventsChecker.UI.exe"
  & $compilator `
	"D:\temp\EventsNotifier\EventsChecker.UI\Types.fs" `
	"D:\temp\EventsNotifier\EventsChecker.UI\Notifiers.fs" `
	"D:\temp\EventsNotifier\EventsChecker.UI\CheckersHealth.fs" `
	"D:\temp\EventsNotifier\EventsChecker.UI\Controls.fs" `
	"D:\temp\EventsNotifier\EventsChecker.UI\Program2.fs" `
  --target:winexe --platform:x86 --out:$res `
  --reference:System.Runtime.Serialization `
  --reference:System.Xml `
  --reference:System.Xml.Linq `
  --reference:System.Web.Extensions `
  --reference:System.Configuration `
  --reference:D:\temp\EventsNotifier\lib\SgmlReaderDll.dll `
  --reference:D:\temp\EventsNotifier\lib\Growl.Connector.dll `
  --reference:D:\temp\EventsNotifier\lib\Growl.CoreLibrary.dll `
  --reference:D:\temp\EventsNotifier\EventsChecker.Core\bin\EventsChecker.Core.dll `
  --reference:d:\temp\EventsNotifier\packages\NLog.2.0.0.2000\lib\net40\NLog.dll 
  
  Copy-Item D:\temp\EventsNotifier\lib\SgmlReaderDll.dll D:\temp\EventsNotifier\EventsChecker.UI\bin\Debug
  Copy-Item D:\temp\EventsNotifier\lib\nlog.config D:\temp\EventsNotifier\EventsChecker.UI\bin\Debug
}