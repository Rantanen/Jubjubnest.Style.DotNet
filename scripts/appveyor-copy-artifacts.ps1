param( [ string ]$version )

cd $PSScriptRoot\..

mkdir dist

Copy-Item `
    "Jubjubnest.Style.DotNet.Vsix\bin\Release\Jubjubnest.Style.DotNet.Vsix.vsix" `
    "dist\Jubjubnest.Style.DotNet.vsix"

Copy-Item `
    "Jubjubnest.Style.DotNet\bin\Release\Jubjubnest.Style.DotNet.*.nupkg" `
    "dist\"
    
