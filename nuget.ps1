$version = "1.0.0"
$releaseNote = "Initial release"

dotnet build DDigit.RecordTransactions `
  --configuration Release `
  -property:PackageReleaseNotes=$releaseNote `
  -property:Version=$version

Get-ChildItem -Path "*$version.nupkg" -Recurse | Copy-Item -Destination "C:\Users\daan\Desktop\DDLib open source export"
