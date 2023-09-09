# Read lines from etypes.h
$lines = Get-Content -Path 'C:\Users\rando\source\repos\wireshark\wireshark_source\epan\etypes.h'

# Initialize an empty array to hold the transformed lines
$transformedLines = @()

# Loop through each line in the file
foreach ($line in $lines) {
    # Use regex to capture the two arguments for the define lines that start with 0x
    if ($line -match '#define\s+(\w+)\s+(0x[\da-fA-F]+)') {
        # Capture the groups
        $arg1 = $matches[1]
        $arg2 = $matches[2]
        
        # Transform the line and add it to the array
        $transformedLines += "{ $arg2, $arg1 }"
    }
}

# Join the transformed lines into a single string, separated by commas and newlines
$joinedTransformedLines = $transformedLines -join ",`r`n"

# Write the result to a new file
Set-Content -Path 'C:\Users\rando\source\repos\netryoshak\Netryoshka\scripts\ether1.txt' -Value $joinedTransformedLines

