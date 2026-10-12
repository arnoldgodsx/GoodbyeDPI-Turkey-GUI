# Assets\hub.ico üretir: koyu yuvarlak kare üzerinde beyaz "hub" işareti —
# merkez nokta, etrafında ince halka ve halkadaki boşluğa oturan küçük bir uydu nokta.
# Kenar çubuğundaki işaret (MainWindow.xaml, "Mark") aynı oranlarla çizilir.
Add-Type -AssemblyName System.Drawing
$dir = Join-Path $PSScriptRoot '..\Assets'
New-Item -ItemType Directory -Force $dir | Out-Null

function RoundRect([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    $p
}
function Hex($h) { [System.Drawing.ColorTranslator]::FromHtml($h) }
function Dot($g, $brush, [float]$cx, [float]$cy, [float]$r) { $g.FillEllipse($brush, $cx - $r, $cy - $r, 2 * $r, 2 * $r) }

$white = New-Object System.Drawing.SolidBrush (Hex '#F4F4F2')
$pngs = foreach ($s in 16, 24, 32, 48, 64, 128, 256) {
    $bmp = New-Object System.Drawing.Bitmap $s, $s
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)

    # Zemin: yukarıdan aşağı çok hafif koyulaşan siyah, ince açık kenar
    $bg = RoundRect 0 0 $s $s ($s * 0.225)
    $fill = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.Point 0, 0), (New-Object System.Drawing.Point 0, $s), (Hex '#202023'), (Hex '#0A0A0B')
    $g.FillPath($fill, $bg)
    if ($s -ge 32) {
        $edge = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(40, 255, 255, 255)), ([Math]::Max(1, $s / 128))
        $inset = $edge.Width / 2
        $g.DrawPath($edge, (RoundRect $inset $inset ($s - 2 * $inset) ($s - 2 * $inset) ($s * 0.225 - $inset)))
    }

    $c = $s / 2
    $ring = $s * 0.27
    $stroke = [Math]::Max(1.6, $s * 0.052)
    $sat = 315                                   # uydu: sağ üst (saat yönünde, x ekseninden)
    $gap = 26                                    # uydunun iki yanında halkada açılan boşluk (derece)

    $pen = New-Object System.Drawing.Pen $white, $stroke
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    $g.DrawArc($pen, $c - $ring, $c - $ring, 2 * $ring, 2 * $ring, $sat + $gap, 360 - 2 * $gap)

    Dot $g $white $c $c ([Math]::Max(1.6, $s * 0.085))
    $rad = $sat * [Math]::PI / 180
    Dot $g $white ($c + $ring * [Math]::Cos($rad)) ($c + $ring * [Math]::Sin($rad)) ([Math]::Max(1.2, $s * 0.048))

    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    if ($s -eq 256) { $bmp.Save((Join-Path $env:TEMP 'hub-icon-preview.png')) }
    [pscustomobject]@{ Size = $s; Bytes = $ms.ToArray() }
}

# PNG gömülü ICO dosyası yaz
$fs = [System.IO.File]::Create((Resolve-Path -LiteralPath $dir).Path + '\hub.ico')
$w = New-Object System.IO.BinaryWriter $fs
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$pngs.Count)
$offset = 6 + 16 * $pngs.Count
foreach ($p in $pngs) {
    $dim = if ($p.Size -ge 256) { 0 } else { $p.Size }
    $w.Write([byte]$dim); $w.Write([byte]$dim); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]$p.Bytes.Length); $w.Write([uint32]$offset)
    $offset += $p.Bytes.Length
}
foreach ($p in $pngs) { $w.Write($p.Bytes) }
$w.Close()
