# ./build.sh platform unity-version asset-store-url
# e.g.
#   ./build.sh android 5.4.0f1 https://cdn.unityads.unity3d.com/unitypackage/master/UnityAds.unitypackage
#   ./build.sh ios 5.4.0f1 https://cdn.unityads.unity3d.com/unitypackage/master/UnityAds.unitypackage

if [ -z "$1" ]
  then
    echo "Unity version number not specified, e.g. '5.4.0f1' (must be installed on the machine)"
    exit
fi

echo Downloading and importing package from $3...
# curl -s -o UnityAds.unitypackage $3 
# rc=$?; if [[ $rc != 0 ]]; then
#     echo "Failed to download package"
#     exit $rc
# fi

"/Applications/Unity $2/Unity.app/Contents/MacOS/Unity" -projectPath "$(pwd)" -importPackage "$(pwd)/UnityAds.unitypackage" -batchMode -quit
rc=$?; if [[ $rc != 0 ]]; then
    echo "Importing package failed. Please check ~/Library/Logs/Unity/Editor.log"
    exit $rc
fi
 
echo Building project with Unity $2 \(iOS\)...
rm -rf Builds/iOS
"/Applications/Unity $2/Unity.app/Contents/MacOS/Unity" -projectPath $(pwd) -executeMethod AutoBuilder.PerformiOSBuild -batchMode -quit
rc=$?; if [[ $rc != 0 ]]; then
    echo "Unity build for iOS failed. Please check ~/Library/Logs/Unity/Editor.log"
    exit $rc
fi

open Builds/iOS/Unity-iPhone.xcodeproj
# xcodebuild -project Builds/iOS/Unity-iPhone.xcodeproj

echo Building project with Unity $2 \(Android\)...
rm Builds/Android.apk
"/Applications/Unity $2/Unity.app/Contents/MacOS/Unity" -projectPath $(pwd) -executeMethod AutoBuilder.PerformAndroidBuild -batchMode -quit
rc=$?; if [[ $rc != 0 ]]; then
    echo "Unity build for Android failed. Please check ~/Library/Logs/Unity/Editor.log"
    exit $rc
fi

echo Installing on Android device...
adb install -r Builds/Android.apk

adb shell am start -S -a android.intent.action.MAIN -n com.unity3d.UnityAdsAssetStoreTest/com.unity3d.player.UnityPlayerActivity
