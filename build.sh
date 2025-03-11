#!/bin/bash
# Script to build for different runtime targets

build_target() {
    local runtime=$1
    local output_dir="publish/$runtime"
    
    echo "Building for $runtime..."
    dotnet publish -c Release -r $runtime --self-contained=true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:AppEnableTraceLogging=false -o $output_dir
    
    if [ $? -eq 0 ]; then
        echo "Build successful for $runtime. Output in $output_dir"
    else
        echo "Build failed for $runtime"
    fi
}

# Create output directory
mkdir -p publish

# Common targets
echo "Building for common targets..."
build_target "win-x64"
build_target "win-x86"
build_target "win-arm64"
build_target "linux-x64"
build_target "linux-musl-x64"
build_target "linux-arm64"
build_target "linux-musl-arm64" 
build_target "osx-x64"
build_target "osx-arm64"

echo "All builds completed!"
