rm -rf Builds/iOS
/Applications/Unity\ 4.6.8f1/Unity.app/Contents/MacOS/Unity -executeMethod AutoBuilder.PerformiOSBuild -projectPath $(pwd) -batchMode -quit
xcodebuild -project Builds/iOS/Unity-iPhone.xcodeproj
