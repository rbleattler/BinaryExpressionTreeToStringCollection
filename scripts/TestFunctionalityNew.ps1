Set-Location "$PSScriptRoot\.."
# $DebugPreference = "Continue"
$DebugPreference = "SilentlyContinue"
$VerbosePreference = 'SilentlyContinue'
$Host.PrivateData.WarningForegroundColor = [System.ConsoleColor]::DarkRed
$Here = $PSScriptRoot
Add-Type -Path @(
    #Idk why it doesn't load for me... if this causes issues, just comment it out
    "~\.nuget\packages\nreco.lambdaparser\*\lib\netstandard2.0\NReco.LambdaParser.dll", 
    # "$Here\..\bin\Debug\netcoreapp3.1\TreeToString.dll"
    "$Here\..\bin\Debug\net5.0\TreeToString.dll"
)
$ExtendedParser = [TreeToString.ExtendedBinaryExpression.ExtendedBinaryExpressionParser]::new();
$LambdaParser = $ExtendedParser.LambdaParser
$Strings = @{}
$i = 0
$RawStrings = (Get-Content "$PSScriptRoot\TestStrings.txt")
$RawStrings.ForEach{
    $Name = 'String{0}' -f $i
    $Strings.$Name = $ExtendedParser.LevelString($PSItem)
    $i++
}

$KnownTypes = @(
    # [System.Linq.Expressions.ExpressionType]::Equal,
    [System.Linq.Expressions.ExpressionType]::AndAlso,
    [System.Linq.Expressions.ExpressionType]::OrElse,
    [System.Linq.Expressions.ExpressionType]::MemberAccess
)

$Parsed = $Strings.Keys.ForEach{
    $ItemNumber = [regex]::Match($PSItem, "(\d{1,3})").Value
    Write-Host -Object "Parsing $_" -ForegroundColor Green
    $ParsedString = $LambdaParser.Parse($Strings.$PSItem)
    # if ($ParsedString.Right.NodeType -eq 'MemberAccess' -and -not ($ParsedString.Left.NodeType -eq 'MemberAccess')) {
    #     Write-Host -Object "Improper MemberAccess" -ForegroundColor Yellow 
    #     $newString = $Strings.$PSItem.replace('((', '(').replace('))', ')')   
    #     $ParsedString = $LambdaParser.Parse($newString)
    # }
    #@ Don't delete until we confirm this has been implemented properly
    # if ($ParsedString.NodeType -eq [System.Linq.Expressions.ExpressionType]::Parameter) {
    #     Write-Host -Object "Parameter Type" -ForegroundColor Yellow 
    #     $newString = '{0} || {0}' -f $ParsedString.Name
    #     $ParsedString = $LambdaParser.Parse($newString).left
    # }
    #@
    # if ($ParsedString.NodeType -notin $KnownTypes) {
    #     Write-Warning $ParsedString
    #     Write-Warning $ParsedString.NodeType
    #     break
    # }

    $EBE = [TreeToString.ExtendedBinaryExpression.ExtendedBinaryExpression]::new($ParsedString)
    $ParsedNode = $EBE.ToParsedNode()
    $NewObject = [PSCustomObject]@{
        Name       = [int]$ItemNumber
        Value      = $EBE 
        ParsedNode = $ParsedNode
    }
    $null = $NewObject | Add-Member -Type ScriptMethod -Name ToParsedNode -Value { $this.Value.ToParsedNode() }
    $NewObject
}

# $testString = "(item_2 || (item_0 && item_4)) && (item_3 || (item_1 && item_5))"
# $EBE = [TreeToString.ExtendedBinaryExpression.ExtendedBinaryExpression]::new($LambdaParser.Parse($testString))
# $EBE.ParseNodes()
# $ExtendedParser.Parse($EBE)
# $DebugPreference = "Continue"
# $VerbosePreference = "Continue"
# $Parsed.Value.ParseNodes()
# $ParsedNode = $Parsed.Value[0].ToParsedNode()
# $ParsedNode
# $RightEntries = $ExtendedParser.Parse($Parsed[6].Node.Right.Right).entries
# $LeftEntries = $ExtendedParser.Parse($Parsed[6].Node.Right.Left).entries
# $parsednode = [TreeToString.ExtendedBinaryExpression.ParsedNode]::new($Parsed[0].NodeType)
# foreach ($rightEntry in $RightEntries) {
#     $newlist = [System.Collections.Generic.List[System.Object]]::new()
#     $newList.Add($rightEntry)
#     foreach ($leftEntry in $leftEntries) {
#         $newList.Add($leftEntry)
#     }
#     $parsednode.AddEntry($newlist)
# }
# $DebugPreference = "Continue"
# $VerbosePreference = "Continue"
# $i = 0
# $parsed.ForEach{
#     @{
#         $i = $PSItem.ToString()
#     }
#     $i++
# }
# [int]$ID = Read-Host -Prompt "Select ID..."

# $ThisParsed = $Parsed[$ID]
# $LeftEntries = $ThisParsed.ParsedLeftNode.Entries
# $RightEntries = $ThisParsed.ParsedRightNode.Entries

# $parsedEntries = $Parsed.ForEach{
#     $Entries = $PSItem.ParsedNode.Entries
#     Write-Host -ForegroundColor DarkGreen -Object "Original String"
#     Write-Host -ForegroundColor Green -Object $PSItem.Node.ToString().Replace('.IsTrue', '').Replace('AndAlso', '&&').Replace('OrElse', '||')
#     Write-Host "`n"
# }
