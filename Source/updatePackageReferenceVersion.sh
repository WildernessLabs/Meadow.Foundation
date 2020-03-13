#!/bin/bash

if [ "$#" -ne "2" ]; then
    printf "missing args, usage: updatePackageReferenceVersion.sh packageReferenceName targetVersion\nex: updatePackageReferenceVersion.sh Meadow.Foundation 1.2.3\n"
    exit
fi

packageName=$1 #name of the package reference to update
targetVersion=$2 #version that we want to change to

# loop through each csproj file and replace ProjectReference with PackageReference
for file in $(find . -name "*.csproj" -print | xargs grep -l "PackageReference")
do
    refs=$(grep 'PackageReference' $file | awk -F"\"" '{print $2}')

    # loop through each PackageReference and update matches
    for project in $refs
    do
        if [ $project = $packageName ]; then
            echo "updating $file"
            sed -i "s|<PackageReference.*\\$packageName\".*|<PackageReference Include=\"$packageName\" Version=\"$targetVersion\" />|" $file
        fi
    done
done