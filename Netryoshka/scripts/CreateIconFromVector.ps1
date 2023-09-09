# Path to the Inkscape executable. Use .com not .exe
$inkscapePath = "C:\Program Files\Inkscape\bin\inkscape.com"
# Path to the ImageMagick's convert executable
$convertPath = "C:\Users\rando\source\repos\ImageMagick-Windows\VisualMagick\bin\convert.exe"
# Input SVG file
$inputFile = "C:\Users\rando\Desktop\netty.svg"
# Output ICO file
$outputFile = "C:\Users\rando\Desktop\icon.ico"

# Create an array of sizes
$sizes = @(16, 32, 48, 128, 256)

# Loop through each size and create PNGs using Inkscape
foreach ($size in $sizes) {
    & $inkscapePath -o "$size.png" -w $size -h $size $inputFile
}

# Use ImageMagick's convert tool to create the ICO file from the PNGs
& $convertPath 16.png, 32.png, 48.png, 128.png, 256.png -colors 256 $outputFile

# Clean up temporary PNG files
Remove-Item "16.png", "32.png", "48.png", "128.png", "256.png"
