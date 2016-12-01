param( [ string ]$version )

# Open the XML file
$manifest = [System.IO.Path]::Combine(
    $PSScriptRoot,
    "../Jubjubnest.Style.DotNet.Vsix/source.extension.vsixmanifest" )
$manifest | Write-Host
[ xml ]$vsix = Get-Content $manifest

# Get the identity element
$ns = New-Object System.Xml.XmlNamespaceManager $vsix.NameTable
$ns.AddNamespace( "ns", "http://schemas.microsoft.com/developer/vsx-schema/2011" )
$vsixVersion = $vsix.SelectSingleNode( "//ns:Identity", $ns ).Attributes[ "Version" ]

# Set the build number of the current version.
$vsixVersion.InnerText = New-Object Version( $version )

$vsix.Save( $manifest )