#!/bin/bash

# create a map of package ids to the project path
declare -A packages
packages["Meadow"]="./Meadow.Core/source/Meadow.Core/Meadow.Core.csproj"
for proj in $(find . -name "*.csproj" -print | xargs grep -l "PackageId")
do
    id=$(grep '<PackageId' $proj | awk -F">" '{print $2}' | awk -F"<" '{print $1}')
    packages[$id]=$proj
done

# now, open up each csproj file and replace the PackageReferences with ProjectReferences
for proj in $(find . -name "*.csproj" -print | xargs grep -l "PackageReference")
do
    refs=$(grep '<PackageReference' $proj | awk -F"\"" '{print $2}')

    for package in $refs
    do
        # get the number of slashes to calculate the dir depth
        cnt=$(tr -dc '/' <<< $proj | awk '{ print length; }')

        prefix=""
        if [ $package = "Meadow" ]; then
            prefix="..\\\\..\\\\."
        else
            for ((i=2;i<cnt;i++))
            do
                prefix="$prefix..\\\\" 
            done
            prefix="$prefix." 
        fi

        # replace the PackageRefernece line with the appropriate folder depth and project path
        if [ -z ${packages["$package"]} ]; then
            echo "cannot find referenced package project for $package in $proj"
        else
            sed -i "s|<PackageReference Include=\"$package\".*|<ProjectReference Include=\"$prefix${packages["$package"]//\//\\\\}\" />|" $proj
        fi
    done
done