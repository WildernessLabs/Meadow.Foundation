$path = "Meadow.Foundation.Peripherals"

$dirs = Get-ChildItem $path -Directory 

ForEach($dir in $dirs) {
    Write-Host($dir)

    $driver = "\Driver\$dir"
    if(Test-Path -Path ".\$path\$dir$driver\$dir.csproj" -PathType Leaf){
        $file = Get-ChildItem ".\$path\$dir$driver\$dir.csproj" -file | Select-Object DirectoryName, name
        #Write-Host($file)
        $xml = [xml](Get-Content ("{0}\{1}" -f $file.DirectoryName, $file.Name))
        
        # bump peripheral version
        $ogVersion = $xml.SelectSingleNode("//Project/PropertyGroup/Version").InnerText
        if($ogVersion -ne $null -And $ogVersion -NotLike "*-beta*"){
            $ogVersion = [version]$ogVersion;
            $newVersion = "{0}.{1}.{2}" -f $ogVersion.Major, $ogVersion.Minor, ($ogVersion.Build+1)

            Write-Host($ogVersion.ToString())
            Write-Host($newVersion)

            $contents = Get-Content ".\$path\$dir$driver\$dir.csproj"
            $contents = $contents -creplace $ogVersion.ToString(), $newVersion
            $contents | Set-Content ".\$path\$dir$driver\$dir.csproj" -encoding UTF8
        }
    }
    else{
        Write-Host("Does not exist - .\$path\$dir$driver\$dir.csproj")
    }   
}

#run again for the Libraries_and_Frameworks

$path = "Meadow.Foundation.Libraries_and_Frameworks"
$driver = ""

$dirs = Get-ChildItem $path -Directory 

ForEach($dir in $dirs) {
    Write-Host($dir)

    if(Test-Path -Path ".\$path\$dir$driver\$dir.csproj" -PathType Leaf){
        $file = Get-ChildItem ".\$path\$dir$driver\$dir.csproj" -file | Select-Object DirectoryName, name
        #Write-Host($file)
        $xml = [xml](Get-Content ("{0}\{1}" -f $file.DirectoryName, $file.Name))
        
        # bump peripheral version
        $ogVersion = $xml.SelectSingleNode("//Project/PropertyGroup/Version").InnerText
        if($ogVersion -ne $null){
            $ogVersion = [version]$ogVersion;
            $newVersion = "{0}.{1}.{2}" -f $ogVersion.Major, $ogVersion.Minor, ($ogVersion.Build+1)

            Write-Host($ogVersion.ToString())
            Write-Host($newVersion)

            $contents = Get-Content ".\$path\$dir$driver\$dir.csproj"
            $contents = $contents -creplace $ogVersion.ToString(), $newVersion
            $contents | Set-Content ".\$path\$dir$driver\$dir.csproj" -encoding UTF8
        }
    }
    else{
        Write-Host("Does not exist - .\$path\$dir$driver\$dir.csproj")
    }   
}