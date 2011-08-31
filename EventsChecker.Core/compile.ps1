param(
)

$compilator = $(
  if (test-path 'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe') { 
    'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  } else { 
    'd:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  })
  
if ($true) {
  $res = "D:\temp\EventsNotifier\EventsChecker.Core\bin\EventsChecker.Core.dll"
  & $compilator `
	"D:\temp\EventsNotifier\EventsChecker.Core\Downloader.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\SimpleValueStorer.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\INotifier.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\IChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\MultiIdsStorer.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\SimpleValueCheckerBase.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\FlashBlogRepliesCountChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\SOReputationChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\SOQuestionsChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\SOAnswersChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\SOCommentsChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\CruiseControlChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\DirectoryContentChecker.fs" `
	"D:\temp\EventsNotifier\EventsChecker.Core\Parsers.fs" `
  --target:library --platform:anycpu --out:$res `
  --reference:System.Runtime.Serialization `
  --reference:System.Xml `
  --reference:System.Xml.Linq `
  --reference:System.Web.Extensions `
  --reference:System.Configuration `
  --reference:D:\temp\EventsNotifier\lib\SgmlReaderDll.dll `
  --reference:d:\temp\EventsNotifier\packages\NLog.2.0.0.2000\lib\net40\NLog.dll 
  
  Copy-Item D:\temp\EventsNotifier\lib\SgmlReaderDll.dll D:\temp\EventsNotifier\EventsChecker.Core\bin
}