param(
)

if ($true) {
  $res = "D:\temp\EventsNotifier\PowerShellRunner\bin\Debug\PowerShellRunner.dll"
  & 'c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe' `
  'd:\temp\EventsNotifier\PowerShellRunner\PowerShellRunner.csproj ' '/property:Configuration=Debug;Platform=x86'
}

# error FSharp.Core.sigdata not found alongside FSharp.Core [d:\temp\EventsNotifier\EventsChecker.Core\EventsChecker.Core.fsproj]
# resolved: copy sigdata & optdata from F# install dir (C:\prgs\dev\FSharp-2.0.0.0\v4.0\bin\) to 
# C:\Windows\Microsoft.NET\assembly\GAC_MSIL\FSharp.Core\v4.0_4.0.0.0__b03f5f7f11d50a3a\