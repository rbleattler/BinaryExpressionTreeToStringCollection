$testStrings = Get-Content $PSScriptRoot\TestStrings.txt
$RegexString = "(\w*(_?\w*)*?)"
$AllStrings = foreach ($string in $testStrings) {
    $Match = Select-String -Pattern $RegexString -InputObject $string -AllMatches
    $AllWords = @($Match.Matches.Value.Where{ 
            ![string]::IsNullOrWhiteSpace($PSItem) 
        })
    $AllWords = $AllWords | Sort-Object -Property Length -Descending # Because the longest string will be at the top... If the longest string contains nested 
    $newString = $string
    for ($i = 0; $i -lt $AllWords.Count; $i++) {
        Write-Debug "Working String : $newString"
        $newWord = 'item_{0}' -f $i
        $ReplaceWord = $AllWords[$i]
        Write-Debug "ReplaceWord: $ReplaceWord"
        Write-Debug "newWord: $newWord"
        if ($ReplaceWord.Length -le 1) {
            $newstring = $newWord
        } else {
            $newstring = $newString.Replace($AllWords[$i], $newWord)
        }
    }
    $newString
}

$AllStrings = $AllStrings | Select-Object -Unique | Sort-Object -Property Length

Set-Content -Path "$PSScriptRoot\TestStrings.txt" -Value $AllStrings