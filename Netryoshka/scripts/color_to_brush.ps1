# PowerShell script to convert Color definitions to SolidColorBrushes

# Path to your input file
$inputFile = "C:\Users\rando\source\repos\netryoshka\Netryoshka\Skins\Dark.xaml"

# Read the lines from the input file
$content = Get-Content -Path $inputFile

# Process each line
$output = $content | ForEach-Object {
    # Check if the line is a color definition and not an XML comment
    if ($_ -match '^\s*<Color x:Key="([^"]+)">#([^<]+)</Color>')  {
        # Extract the key and color values using regex capture groups
        $key = $matches[1]
        $color = $matches[2]

        # Return the SolidColorBrush definition
        "<SolidColorBrush x:Key=`"$key`Brush`" Color=`"{StaticResource $key}`" />"
    } else {
        # Return the line as-is (including XML comments or any other content)
        $_
    }
}

# Output the result to the console
$output

# If you want to save the output to a file, uncomment the line below:
# $output | Out-File "path_to_output.xml"

