param( [ string ]$version )

cd $PSScriptRoot\..

mkdir dist

Copy-Item `
    "Jubjubnest.Style.DotNet.Vsix\bin\Release\Jubjubnest.Style.DotNet.Vsix.vsix" `
    "dist\Jubjubnest.Style.DotNet.vsix"
