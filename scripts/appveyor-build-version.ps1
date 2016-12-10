param( [ string ]$version )
$version = New-Object Version( $version )

"Setting version to $version" | Write-Host

################
# VSIX version #
################

# Open the XML file
$vsixmanifestPath = [System.IO.Path]::Combine(
    $PSScriptRoot,
    "../Jubjubnest.Style.DotNet.Vsix/source.extension.vsixmanifest" )
[ xml ]$vsixmanifest = Get-Content $vsixmanifestPath

# Get the identity element
$ns = New-Object System.Xml.XmlNamespaceManager $vsixmanifest.NameTable
$ns.AddNamespace( "ns", "http://schemas.microsoft.com/developer/vsx-schema/2011" )
$vsixVersion = $vsixmanifest.SelectSingleNode( "//ns:Identity", $ns ).Attributes[ "Version" ]

# Set the build number of the current version.
$vsixVersion.InnerText = $version

$vsixmanifest.Save( $vsixmanifestPath )

####################
# Assembly version #
####################

(Get-Content "$PSScriptRoot/../Jubjubnest.Style.DotNet/Properties/AssemblyInfo.cs") `
    -replace '/\* APPVEYOR-BUILD-VERSION \*/ "[^"]*"', "`"$version`"" |
    Out-File "$PSScriptRoot/../Jubjubnest.Style.DotNet/Properties/AssemblyInfo.cs"


