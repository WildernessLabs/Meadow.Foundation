#!/bin/bash

scriptdir="$( cd "$(dirname "$0")" ; pwd -P )"

SLN=$scriptdir/source/Meadow.Foundation.sln

NUGET=nuget
if [[ $(command -v nuget) == "" ]]; then
    NUGET=/Library/Frameworks/Mono.framework/Versions/Current/bin/nuget
fi

$NUGET restore $SLN

UNAMECMD=`uname`
if [[ "$UNAMECMD" == "Darwin" ]]; then
    echo "Mac OS detected, using MSBuild from Visual Studio."
    MSBUILD="mono '/Applications/Visual Studio.app/Contents/Resources/lib/monodevelop/bin/MSBuild/Current/bin/msbuild.dll'"
else
    echo "Unknown OS, defaulting to msbuild."
    MSBUILD=msbuild
fi

if [[ $(command -v msbuild) == "" ]]; then
    MSBUILD=/Library/Frameworks/Mono.framework/Versions/Current/bin/msbuild
fi

eval $MSBUILD /t:restore $SLN
eval $MSBUILD $SLN