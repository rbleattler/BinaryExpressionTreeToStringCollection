Set-Location "$PSScriptRoot\.."
# $DebugPreference = "Continue"
$Host.PrivateData.WarningForegroundColor = [System.ConsoleColor]::DarkRed
$Here = $PSScriptRoot
Add-Type -Path @(
    #Idk why it doesn't load for me... if this causes issues, just comment it out
    "~\.nuget\packages\nreco.lambdaparser\*\lib\netstandard2.0\NReco.LambdaParser.dll", 
    # "$Here\..\bin\Debug\netcoreapp3.1\TreeToString.dll"
    "$Here\..\bin\Debug\net5.0\TreeToString.dll"
)
$TreeParser = [TreeToString.BinaryExpressionTreeParser]::new();
$ListUtils = [TreeToString.ListUtilities]::new()
$TestClass = [TreeToString.TestClass]::new()
$LambdaParser = $TreeParser.LambdaParser
$Strings = @{}
$i = 0
$RawStrings = (Get-Content "$PSScriptRoot\TestStrings.txt")
$RawStrings.ForEach{
    $Name = 'String{0}' -f $i
    $Strings.$Name = $PSItem
    $i++
}
$BadStrings = [System.Collections.ArrayList]::new()
$i = 0
$($Strings.Keys | Sort-Object).ForEach{
    $LevelString = $TreeParser.LevelString($Strings.$PSItem)
    $SetVar = @{
        Name  = $("Parsed$i")
        Value = $LambdaParser.Parse($LevelString)
    }
    Set-Variable @SetVar
    $ThisVar = Get-Variable $SetVar.Name -ValueOnly
    # Write-Verbose "ThisVar : $ThisVar"
    $OutText = ("$($SetVar.Name) : " + $($SetVar.Value.ToString()) + "`n")
    try {
        $Out = $TreeParser.ToStringCollection($ThisVar)
    } catch {
        Write-Warning "FAIL"
        Write-Warning $OutText
        $null = $BadStrings.Add($ThisVar)
        $Out = $null
        $OutText = "Errored"
    }
    $Host.UI.WriteLine([System.ConsoleColor]::Green, $Host.UI.RawUI.BackgroundColor, $OutText)
    $Out.ForEach{
        $Host.UI.WriteLine([System.ConsoleColor]::Cyan, $Host.UI.RawUI.BackgroundColor, $($_ -join ', '))
    }
    $VarName = "Out$i"
    Set-Variable -Name $VarName -Value $Out
    $Host.UI.WriteLine([System.ConsoleColor]::Green, $Host.UI.RawUI.BackgroundColor, "`n")
    $i++
    Clear-Variable Out
    Clear-Variable ThisVar
}
# $rootList = $TreeParser.ToStringCollection($ThisVar.Left); 
# $processList = $TreeParser.ToStringCollection($ThisVar.Right)
# Write-Debug "Merge List test : "


# $mainList = [System.Collections.ArrayList]::new()
# $rootList.ForEach{
#     $newBaseList = [System.Collections.ArrayList]::new()
#     $rootItem = $PSItem
#     if ($rootItem.Count -gt 1) {
#         $rootItem.ForEach{
#             $null = $newBaseList.Add($PSItem)
#         }
#     } else {
#         $null = $newBaseList.Add($rootItem)
#     }
#     $processList.ForEach{
#         $newList = [System.Collections.ArrayList]::new($newBaseList)
#         $processItem = $PSItem
#         if ($processItem.Count -gt 1) {
#             $processItem.ForEach{
#                 $null = $newList.Add($PSItem)
#             }
#         } else {
#             $null = $newList.Add($processItem)
#         }
#         $null = $mainList.Add($newList)
#     }
#     $newBaseList = $null
#     $newList = $null
# }

# $l.ForEach{
#     $litem = $_
#     $r.ForEach{
#         $TestClass.OrMerge($litem, $_)
#     }
# } #<== this works, but below doesnt...

# $newList = [System.Collections.ArrayList]::new()
# foreach ($leftItem in $left) {
#     foreach ($rightItem in $right) {
#         $newItem = $TestClass.OrMerge($leftItem, $rightItem)
#         $newItem.ForEach{
#             $newList.Add($PSItem)
#         }
#     }
# }

$DebugPreference = "SilentlyContinue"