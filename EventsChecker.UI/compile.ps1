param(
)

$compilator = $(
  if (test-path 'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe') { 
    'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  } else { 
    'd:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  })
  
if ($true) {
  $res = "D:\temp\EventsChecker\EventsChecker.UI\bin\Debug\EventsChecker.UI.exe"
  & $compilator `
	"D:\temp\EventsChecker\EventsChecker.UI\Types.fs" `
	"D:\temp\EventsChecker\EventsChecker.UI\Notifiers.fs" `
	"D:\temp\EventsChecker\EventsChecker.UI\Controls.fs" `
	"D:\temp\EventsChecker\EventsChecker.UI\Program2.fs" `
  --target:winexe --platform:anycpu --out:$res `
  --reference:System.Runtime.Serialization `
  --reference:System.Xml `
  --reference:System.Xml.Linq `
  --reference:System.Web.Extensions `
  --reference:System.Configuration `
  --reference:D:\temp\EventsChecker\lib\SgmlReaderDll.dll `
  --reference:D:\temp\EventsChecker\lib\Growl.Connector.dll `
  --reference:D:\temp\EventsChecker\lib\Growl.CoreLibrary.dll `
  --reference:D:\temp\EventsChecker\EventsChecker.Core\bin\EventsChecker.Core.dll
  
  Copy-Item D:\temp\EventsChecker\lib\SgmlReaderDll.dll D:\temp\EventsChecker\EventsChecker.UI\bin\Debug
}