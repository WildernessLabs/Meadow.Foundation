
$path = "Meadow.Foundation.Peripherals"
$dirs = Get-ChildItem $path -Directory 

ForEach($dir in $dirs) {
    Write-Host($dir)

    if(Test-Path -Path ".\$path\$dir\Driver\$dir\$dir.csproj" -PathType Leaf){
        $file = Get-ChildItem ".\$path\$dir\Driver\$dir\$dir.csproj" -file | Select-Object DirectoryName, name
        #Write-Host($file)
        $xml = [xml](Get-Content ("{0}\{1}" -f $file.DirectoryName, $file.Name))
        
        # bump peripheral version
        $ogVersion = $xml.SelectSingleNode("//Project/PropertyGroup/Version").InnerText
        if($ogVersion -ne $null){
            $ogVersion = [version]$ogVersion;
            $newVersion = "{0}.{1}.{2}" -f $ogVersion.Major, $ogVersion.Minor, ($ogVersion.Build+1)

            Write-Host($ogVersion.ToString())
            Write-Host($newVersion)

            $contents = Get-Content ".\$path\$dir\Driver\$dir\$dir.csproj"
            $contents = $contents -creplace $ogVersion.ToString(), $newVersion
            $contents | Set-Content ".\$path\$dir\Driver\$dir\$dir.csproj" -encoding UTF8
        }
    }
    else{
        Write-Host("Does not exist - .\$path\$dir\Driver\$dir\$dir.csproj")
    }   
}