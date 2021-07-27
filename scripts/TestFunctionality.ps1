Set-Location "$PSScriptRoot\.."
$Here = $PSScriptRoot
Add-Type -Path @(
    #Idk why it doesn't load for me... if this causes issues, just comment it out
    "~\.nuget\packages\nreco.lambdaparser\*\lib\netstandard2.0\NReco.LambdaParser.dll", 
    "$Here\..\bin\Debug\netcoreapp3.1\StringOps.dll"
)
$TreeParser = [StringOps.BinaryExpressionTreeParser]::new();
$LambdaParser = [NReco.Linq.LambdaParser]::new()
$Strings = @{
    String0 = "(param1 && param2 && param3 && (param4 || param5 || param6))"
    String1 = "(param1 || (param2 && param3 && param4 && (param5 || param6 || param7)))"
    String2 = "(param_1 && param_2 && param_3 && (param_4 || param_5 || param_6))"
    String3 = "(param_1 && param_2 && (param_3 || param_4) && (param_5 || param_6 || param_7))"
    String4 = "(param1 || param8 || ((param2 || param3) && param4 && (param5 || param6 || param7)))"
    String5 = "(param1 || param8 && (param2 && param3 && param4 && (param5 || param6 || param7)))"
    String6 = "(interface_id || (interface_name && (device_id || device_name))" # Bad String
}
$i = 0
$($Strings.Keys | Sort-Object).ForEach{
    $SetVar = @{
        Name  = $("Parsed$i")
        Value = $LambdaParser.Parse($Strings.$PSItem)
    }
    Set-Variable @SetVar
    $ThisVar = Get-Variable $SetVar.Name -ValueOnly
    $Out = $TreeParser.ToStringCollection($ThisVar)
    $OutText = ("$($SetVar.Name)" + $($SetVar.Value.ToString()) + "`n")
    $Host.UI.WriteLine([System.ConsoleColor]::Green, $Host.UI.RawUI.BackgroundColor, $OutText)
    $Out.ForEach{
        $Host.UI.WriteLine([System.ConsoleColor]::Cyan, $Host.UI.RawUI.BackgroundColor, $($_ -join ', '))
    }
    $VarName = "Out$i"
    Set-Variable -Name $VarName -Value $Out
    $Host.UI.WriteLine([System.ConsoleColor]::Green, $Host.UI.RawUI.BackgroundColor, "`n")
    $i++
}
