# Hub kurulumu (yönetici olarak çalıştırın). Tekrar çalıştırmak güvenlidir.
#  - Eski ayrı uygulamayı (GoodbyeDPI Panel) durdurur, kısayollarını kaldırır
#  - Masaüstüne ve Başlat menüsüne (tüm kullanıcılar) "Hub" kısayolu koyar
#  - Hub'ı başlatır; Hub ilk açılışta UAC'siz açılış için zamanlanmış görevini kendisi kurar
$ErrorActionPreference = 'Stop'
$dist = Join-Path (Split-Path $PSScriptRoot) 'dist'
$exe = Join-Path $dist 'Hub.exe'
if (-not (Test-Path $exe)) { throw "Önce derleyin: $exe bulunamadı" }

Get-Process 'GoodbyeDPI Panel' -ErrorAction SilentlyContinue | Stop-Process -Force

$desktop = [Environment]::GetFolderPath('Desktop')
$startMenu = Join-Path $env:ProgramData 'Microsoft\Windows\Start Menu\Programs'
foreach ($old in "$startMenu\GoodbyeDPI.lnk") {
    if (Test-Path $old) { Remove-Item $old -Force }
}

$shell = New-Object -ComObject WScript.Shell
foreach ($lnk in "$desktop\Hub.lnk", "$startMenu\Hub.lnk") {
    $s = $shell.CreateShortcut($lnk)
    $s.TargetPath = $exe
    $s.WorkingDirectory = $dist
    $s.IconLocation = "$exe,0"
    $s.Description = 'GoodbyeDPI-Turkey arayüzü'
    $s.Save()
}

Start-Process $exe -ArgumentList '--show' -WorkingDirectory $dist
