param($installPath, $toolsPath, $package, $project)
$moniker = $project.Properties.Item("TargetFrameworkMoniker").Value
$frameworkName = New-Object System.Runtime.Versioning.FrameworkName($moniker)
Write-Host "TargetFrameworkMoniker: " $moniker
if ($frameworkName.Version.Build -ge 1)
{
    Write-Host "Adding Behaviors SDK (XAML)"
    $project.Object.References.AddSDK("Behaviors SDK (XAML)", "BehaviorsXamlSDKManaged, version=12.0")
}