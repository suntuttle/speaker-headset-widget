param([string]$Output = 'SpeakerWidget.exe')
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
  & "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /win32manifest:app.manifest /win32icon:app.ico "/out:$Output" /r:System.Windows.Forms.dll /r:System.Drawing.dll SpeakerWidget.cs
  if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
} finally { Pop-Location }

