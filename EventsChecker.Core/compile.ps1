param(
)

$compilator = $(
  if (test-path 'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe') { 
    'C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  } else { 
    'd:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\fsc.exe' 
  })
  
if ($true) {
  $res = "D:\temp\EventsChecker\EventsChecker.Core\bin\EventsChecker.Core.dll"
  & $compilator `
	"D:\temp\EventsChecker\EventsChecker.Core\Downloader.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\SimpleValueStorer.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\INotifier.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\IChecker.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\MultiIdsStorer.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\SimpleValueCheckerBase.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\SOReputationChecker.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\FlashBlogRepliesCountChecker.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\SOQuestionsChecker.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\SOAnswersChecker.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\SOCommentsChecker.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\CruiseControlChecker.fs" `
	"D:\temp\EventsChecker\EventsChecker.Core\Parsers.fs" `
  --target:library --platform:anycpu --out:$res `
  --reference:System.Runtime.Serialization `
  --reference:System.Xml `
  --reference:System.Xml.Linq `
  --reference:System.Web.Extensions `
  --reference:System.Configuration `
  --reference:D:\temp\EventsChecker\lib\SgmlReaderDll.dll
  
  Copy-Item D:\temp\EventsChecker\lib\SgmlReaderDll.dll D:\temp\EventsChecker\EventsChecker.Core\bin
}