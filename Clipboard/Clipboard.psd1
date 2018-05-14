@{
    GUID = '425d8c84-5eca-491a-8f3f-468ede5485ec'
    Author='Dan Travison'
    CompanyName='Dan Travison'
    Copyright='� Dan Travison. All rights reserved.'
    Description = 'Cmdlet for interacting with the clipboard'
    NestedModules = @('Clipboard.Cmdlet.dll')
    FormatsToProcess=@('Clipboard.format.ps1xml')
    #TypesToProcess=@()
    ModuleVersion = '1.0.0.0'
    AliasesToExport = @()
    FunctionsToExport = @('Get-Clipboard', 'Set-Clipboard', 'Get-ClipboardFormat', 'Clear-Clipboard')
    CmdletsToExport = '*'
    PowerShellVersion = '6.0'
    CLRVersion = '4.0'
}
