#!/bin/bash

# create a map of package ids to the project path
declare -A packageIds
declare -A packageVersions

# manually add Meadow.Core path
packageIds["Meadow.Core.csproj"]=$(grep '<PackageId' ../../Meadow.Core/source/Meadow.Core/Meadow.Core.csproj | awk -F">" '{print $2}' | awk -F"<" '{print $1}')
packageVersions["Meadow.Core.csproj"]=$(grep '<Version' ../../Meadow.Core/source/Meadow.Core/Meadow.Core.csproj | awk -F">" '{print $2}' | awk -F"<" '{print $1}')

for file in $(find . -name "*.csproj" -print | xargs grep -l "PackageId")
do
    projName=$(basename $file)
    id=$(grep 'PackageId' $file | awk -F">" '{print $2}' | awk -F"<" '{print $1}')
    version=$(grep 'Version' $file | awk -F">" '{print $2}' | awk -F"<" '{print $1}')
    packageIds[$projName]=$id
    packageVersions[$projName]=$version
done

# loop through each csproj file and replace ProjectReference with PackageReference
for file in $(find . -name "*.csproj" -print | xargs grep -l "ProjectReference")
do
    refs=$(grep 'ProjectReference' $file | awk -F"\"" '{print $2}')

    for project in $refs
    do
        filename=$(basename $project)
        if [ -z ${packageIds["$filename"]} ]; then
            echo "cannot find package for $filename in $file"
        else
            sed -i "s|<ProjectReference.*\\$filename.*|<PackageReference Include=\"${packageIds["$filename"]}\" Version=\"${packageVersions["$filename"]}\" />|" $file
        fi
    done
done