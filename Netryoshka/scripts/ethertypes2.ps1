# Initialize a dictionary to hold the mappings
$dict = @{}

# Read and process packet-ethertype.c
$packetLines = Get-Content -Path 'C:\Users\rando\source\repos\wireshark\wireshark_source\epan\dissectors\packet-ethertype.c'
foreach ($line in $packetLines) {
    if ($line -match '{\s*(\w+),\s*"([^"]+)"\s*}') {
        $key = $matches[1]
        $value = $matches[2]
        $dict[$key] = $value
    }
}

# Read output.txt
$outputLines = Get-Content -Path 'C:\Users\rando\source\repos\netryoshka\Netryoshka\scripts\ether1.txt'

# Initialize an array to hold the updated lines
$updatedLines = @()

# Replace the variable names
foreach ($line in $outputLines) {
    if ($line -match '{\s*(0x[\da-fA-F]+),\s*(\w+)\s*}') {
        $hex = $matches[1]
        $varName = $matches[2]
        
        # Replace the variable name if found in the dictionary; otherwise, keep the original name
        if ($dict.ContainsKey($varName)) {
            $replacement = $dict[$varName]
        } else {
            $replacement = $varName
        }
        
        $updatedLines += "{ $hex, `"$replacement`" }"
    }
}

# Join the updated lines into a single string
$joinedUpdatedLines = $updatedLines -join ",`r`n"

# Write the result to a new file
Set-Content -Path 'C:\Users\rando\source\repos\netryoshka\Netryoshka\scripts\ether2.txt' -Value $joinedUpdatedLines
