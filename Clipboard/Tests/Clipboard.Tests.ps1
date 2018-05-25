using namespace PowerShell.Clipboard
Import-Module .\Clipboard.psd1

enum Platform
{
    Unknown = '0';
    Windows = '1';
    Linux = '2';
    OSX = '3';
}

function Get-Platform
{
    [Platform] $value = [Platform]::Unknown

    if ($PSHOME.EndsWith('\WindowsPowerShell\v1.0', [System.StringComparison]::OrdinalIgnoreCase))
    {
       $value = [Platform]::Windows
    }
    elseif ((Get-Variable -Name IsWindows -ErrorAction Ignore) -and $IsWindows)
    {
        $value = [Platform]::Windows
    }
    elseif ((Get-Variable -Name IsLinux -ErrorAction Ignore) -and $IsLinux)
    {
        $value = [Platform]::Linux
    }
    elseif ((Get-Variable -Name IsOSX -ErrorAction Ignore) -and $IsOSX)
    {
        $value = [Platform]::OSX
    }
    return $value
}

Describe "Clipboard" -Tag @('CI') `
{
    BeforeAll {
        [Platform] $platform = Get-Platform
    }

    Context 'Clear Clipboard' `
    {
        BeforeEach {
            [Clipboard]::SetText("Clear Clipboard test", [ClipboardTextFormat]::Text)
        }

        It 'Verifies the clipboard is empty after [Clipboard]::Clear' -skip:($platform -ne [Platform]::Windows) `
        {
            [Clipboard]::Contains([ClipboardTextFormat]::Text) | Should -Be $true
            [Clipboard]::Clear()
            $formats = [Clipboard]::GetFormats()

            $formats.Count | Should -Be 0
        }

        It 'Verifies the clipboard is empty after Clear-Clipboard' -skip:($platform -ne [Platform]::Windows) `
        {
            [Clipboard]::Contains([ClipboardTextFormat]::Text) | Should -Be $true
            Clipboard\Clear-Clipboard
            $formats = [Clipboard]::GetFormats()
            $formats.Count | Should -Be 0
        }
    }

    Context 'Set and Get-Clipboard' `
    {
        BeforeEach {
            [Clipboard]::Clear()
        }

        It 'Verifies the clipboard can be set and retrieved as Unicode' -skip:($platform -ne [Platform]::Windows) `
        {
			$expected = 'Get-Clipbooard Text test'
			Clipboard\Set-Clipboard -Value $expected -Unicode
            $actual = Clipboard\Get-Clipboard -Unicode
			$actual | Should -Be $expected
        }

		It 'Verifies the clipboard can be set and retrieved as Text' -skip:($platform -ne [Platform]::Windows) `
        {
			$expected = 'Get-Clipbooard Text test'
			Clipboard\Set-Clipboard -Value $expected -Text
            $actual = Clipboard\Get-Clipboard -Text
			$actual | Should -Be $expected
        }

		It 'Verifies the clipboard can be set and retrieved as HTML' -skip:($platform -ne [Platform]::Windows) `
        {
			$expected = @'
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>HTML TABLE</title>
</head><body>
<table>
<colgroup><col/><col/><col/></colgroup>
<tr><th>Name</th><th>Age</th><th>Friends</th></tr>
<tr><td>John Doe</td><td>42</td><td>System.Object[]</td></tr>
</table>
</body></html>
'@
            Clipboard\Set-Clipboard -Value $expected -HTML
            $actual = Clipboard\Get-Clipboard -HTML
			$actual | Should -Be $expected
        }

        It 'Verifies the clipboard can be set and retrieved as a FileList (CF_HDROP)' -skip:($platform -ne [Platform]::Windows) `
        {
            $expected = @(
                [IO.Path]::Combine($TestDrive, "foo", "bar.txt"),
                [IO.Path]::Combine($TestDrive, "bar", "bar.txt"),
                [IO.Path]::Combine($TestDrive, "baz", "bar.txt")
            )
            Clipboard\Set-Clipboard -Values $expected -FileList
            $actual = Clipboard\Get-Clipboard -FileList

            $actual | Should -Not -BeNull
            $actual.Count | Should -Be $expected.Count

            for ($x = 0; $x -lt $expected.Count; $x++)
            {
                $exp = $expected[$x]
                $act = $actual[$x]
                $act | Should -Be $exp
            }
        }

        It 'Verifies the clipboard can be set and retrieved as a XmlSpreadSheet' -skip:($platform -ne [Platform]::Windows) `
        {
            $expected = @'
<?xml version="1.0"?>
<?mso-application progid="Excel.Sheet"?>
<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
 xmlns:o="urn:schemas-microsoft-com:office:office"
 xmlns:x="urn:schemas-microsoft-com:office:excel"
 xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
 xmlns:html="http://www.w3.org/TR/REC-html40">
 <Styles>
  <Style ss:ID="Default" ss:Name="Normal">
   <Alignment ss:Vertical="Bottom"/>
   <Borders/>
   <Font ss:FontName="Calibri" ss:Color="#000000"/>
   <Interior/>
   <NumberFormat/>
   <Protection/>
  </Style>
  <Style ss:ID="s15">
   <Alignment ss:Vertical="Bottom"/>
  </Style>
  <Style ss:ID="s19">
   <Alignment ss:Vertical="Bottom"/>
   <NumberFormat ss:Format="mm/dd/yy;@"/>
  </Style>
  <Style ss:ID="s21">
   <Alignment ss:Horizontal="Right" ss:Vertical="Bottom"/>
   <NumberFormat ss:Format="[$-F400]h:mm:ss\ AM/PM"/>
  </Style>
  <Style ss:ID="s22">
   <Alignment ss:Horizontal="Right" ss:Vertical="Bottom"/>
  </Style>
 </Styles>
 <Worksheet ss:Name="BP">
  <Table ss:ExpandedColumnCount="4" ss:ExpandedRowCount="8" ss:StyleID="s15"
   ss:DefaultRowHeight="13.8">
   <Column ss:StyleID="s19" ss:Width="54.599999999999994"/>
   <Column ss:StyleID="s21" ss:Width="58.8"/>
   <Column ss:StyleID="s22" ss:AutoFitWidth="0" ss:Width="49.2"/>
   <Column ss:StyleID="s15" ss:AutoFitWidth="0" ss:Width="30.599999999999998"/>
   <Row>
    <Cell><Data ss:Type="DateTime">2016-08-24T00:00:00.000</Data></Cell>
    <Cell><Data ss:Type="String">7:35pm</Data></Cell>
    <Cell><Data ss:Type="String">138/76</Data></Cell>
    <Cell><Data ss:Type="Number">71</Data></Cell>
   </Row>
   <Row>
    <Cell><Data ss:Type="DateTime">2016-09-25T00:00:00.000</Data></Cell>
    <Cell><Data ss:Type="String">8:15pm</Data></Cell>
    <Cell><Data ss:Type="String">120/68</Data></Cell>
    <Cell><Data ss:Type="Number">87</Data></Cell>
   </Row>
   <Row>
    <Cell><Data ss:Type="DateTime">2016-10-16T00:00:00.000</Data></Cell>
    <Cell><Data ss:Type="String">5:30pm</Data></Cell>
    <Cell><Data ss:Type="String">132/73</Data></Cell>
    <Cell><Data ss:Type="Number">61</Data></Cell>
   </Row>
  </Table>
 </Worksheet>
</Workbook>
'@
            Clipboard\Set-Clipboard -Value $expected -XmlSpreadsheet
            $actual = Clipboard\Get-Clipboard -XmlSpreadsheet
			$actual | Should -Be $expected
        }

        It 'Verifies the clipboard can be set and retrieved as RTF' -skip:($platform -ne [Platform]::Windows) `
        {
            $expected = @'
{\rtf\ansi{\fonttbl{\f0 Consolas;}}{\colortbl;\red0\green0\blue139;\red0\green0\blue0;\red138\green43\blue226;\red0\green0\blue255;}\f0 \fs19 \cf1 \cb0 \highlight0 using\cf2  \cf1 namespace\cf2  \cf3 PowerShell.Clipboard\cf2 \par \cf4 Import-Module\cf2  \cf3 .\\Clipboard.psd1}
'@
            Clipboard\Set-Clipboard -Value $expected -Rtf
            $actual = Clipboard\Get-Clipboard -Rtf
			$actual | Should -Be $expected
        }

        It 'Verifies the clipboard can be set and retrieved as CSV' -skip:($platform -ne [Platform]::Windows) `
        {
            $jsonValue = @'
{
"one" : 1,
"two" : 2,
"three" : 3,
"text" : "text"
}
'@
            $expected = ConvertFrom-Json -InputObject $jsonValue | ConvertTo-Csv | Out-String
            Clipboard\Set-Clipboard -Value $expected -CSV
            $actual = Clipboard\Get-Clipboard -CSV
			$actual | Should -Be $expected
        }

    }

    Context 'Get-ClipboardFormat' `
    {
        It 'Verifies expected formats' -skip:($platform -ne [Platform]::Windows) `
        {
            $expectedFormats = @('Unicode', 'Locale', 'Text', 'OemText')
            Clipboard\Set-Clipboard -Value 'Get-ClipboardFormats' -Unicode

            $formats = Clipboard\Get-ClipboardFormat
            $actualFormats = @{}
            foreach ($format in $formats)
            {
                $actualFormats[$format.Name, $format]
            }

            foreach ($name in $expected)
            {
                $value = $actualFormats[$name]
                $value | Should -Be $name
            }
        }

        It 'Verifies empty format list with clipboard is empty' -skip:($platform -ne [Platform]::Windows) `
        {
            Clipboard\Clear-Clipboard
            $formats = Clipboard\Get-ClipboardFormat
            $formats.Count | Should -Be 0
        }
    }
}