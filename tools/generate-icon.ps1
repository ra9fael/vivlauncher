$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore, PresentationFramework, WindowsBase
$root = Split-Path -Parent $PSScriptRoot
[xml]$svg = Get-Content -LiteralPath (Join-Path $root 'prototypes/dual-runtime/icon-blue.svg') -Raw
$background = $svg.DocumentElement.SelectSingleNode("*[local-name()='path']").GetAttribute('fill')
$arrow = $svg.DocumentElement.SelectSingleNode("*[local-name()='svg']")
$left, $top, $width, $height = $arrow.GetAttribute('viewBox').Split(' ')
$scaleX = [double]$arrow.GetAttribute('width') / [double]$width
$scaleY = [double]$arrow.GetAttribute('height') / [double]$height
$offsetX = [double]$arrow.GetAttribute('x') - [double]$left * $scaleX
$offsetY = [double]$arrow.GetAttribute('y') - [double]$top * $scaleY
$matrix = ([Windows.Media.Matrix]::new($scaleX, 0, 0, $scaleY, $offsetX, $offsetY)).ToString([Globalization.CultureInfo]::InvariantCulture)
$polygons = foreach ($polygon in $arrow.SelectNodes("*[local-name()='polygon']")) {
    $points = $polygon.GetAttribute('points').Split(' ')
    '                <GeometryDrawing Brush="{0}" Geometry="M{1},{2} L{3} Z" />' -f $arrow.GetAttribute('fill'), $points[0], $points[1], ($points[2..($points.Length - 1)] -join ' ')
}
$xaml = @"
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <DrawingImage x:Key="BrandIcon">
        <DrawingImage.Drawing>
            <DrawingGroup>
                <GeometryDrawing Brush="$background" Geometry="M0,0 H48 V48 H0 Z" />
                <DrawingGroup>
                    <DrawingGroup.Transform><MatrixTransform Matrix="$matrix" /></DrawingGroup.Transform>
$($polygons -join "`n")
                </DrawingGroup>
            </DrawingGroup>
        </DrawingImage.Drawing>
    </DrawingImage>
</ResourceDictionary>
"@
$resources = Join-Path $root 'VivLauncher.App/Resources'
[IO.File]::WriteAllText((Join-Path $resources 'BrandIcon.xaml'), $xaml.Replace("`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
$image = [Windows.Markup.XamlReader]::Parse($xaml)['BrandIcon']
$sizes = 16, 20, 24, 32, 40, 48, 64, 128, 256
$frames = foreach ($size in $sizes) {
    $visual = [Windows.Media.DrawingVisual]::new()
    $drawing = $visual.RenderOpen()
    $drawing.DrawImage($image, [Windows.Rect]::new(0, 0, $size, $size))
    $drawing.Close()
    $bitmap = [Windows.Media.Imaging.RenderTargetBitmap]::new($size, $size, 96, 96, [Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder = [Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream = [IO.MemoryStream]::new()
    try { $encoder.Save($stream); ,$stream.ToArray() }
    finally { $stream.Dispose() }
}
$writer = [IO.BinaryWriter]::new([IO.File]::Create((Join-Path $resources 'app.ico')))
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$sizes.Count)
    $offset = 6 + 16 * $sizes.Count
    for ($index = 0; $index -lt $sizes.Count; $index++) {
        $sizeByte = [byte]($sizes[$index] % 256)
        $writer.Write($sizeByte)
        $writer.Write($sizeByte)
        $writer.Write([uint16]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$frames[$index].Length)
        $writer.Write([uint32]$offset)
        $offset += $frames[$index].Length
    }
    foreach ($frame in $frames) { $writer.Write([byte[]]$frame) }
}
finally { $writer.Dispose() }
