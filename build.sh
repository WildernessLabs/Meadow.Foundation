scriptdir="$( cd "$(dirname "$0")" ; pwd -P )"

SLN=$scriptdir/source/Meadow.Foundation.sln

NUGET=nuget
if [[ $(command -v nuget) == "" ]]; then
    NUGET=/Library/Frameworks/Mono.framework/Versions/Current/bin/nuget
fi

$NUGET restore $SLN

MSBUILD=msbuild
if [[ $(command -v msbuild) == "" ]]; then
    MSBUILD=/Library/Frameworks/Mono.framework/Versions/Current/bin/msbuild
fi

$MSBUILD /t:restore $SLN
$MSBUILD $SLN