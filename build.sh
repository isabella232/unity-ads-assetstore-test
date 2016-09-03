# ./build.sh platform unity-version
# e.g.
#   ./build.sh android 5.4.0f1
#   ./build.sh ios 5.4.0f1

if [ -z "$1" ]; then
    echo "Build Unity Ads SDK example project"
    echo
    echo "./build.sh platform unity-version"
    echo
    echo "Example:"
    echo "  ./build.sh android 5.4.0f1"
    echo "  ./build.sh ios 5.4.0f1"
    echo
    echo "Specified Unity version e.g. '5.4.0f1' must be installed on the machine, in folder /Applications/Unity (version)"
    exit 1
fi

# echo Downloading and importing package from $3...
# curl -s -o UnityAds.unitypackage $3 
# rc=$?; if [[ $rc != 0 ]]; then
#     echo "Failed to download package"
#     exit $rc
# fi

# Import package into project

UNITY="/Applications/Unity $2/Unity.app/Contents/MacOS/Unity"
if [ ! -f "$UNITY" ]; then
    echo "Could not find Unity installed at '$UNITY'. Please verify it's installed and available in that location"
    exit 1
fi

if [ -f $(pwd)/UnityAds.unitypackage ]; then
    echo "Import Unity Ads asset store package..."
    "$UNITY" -projectPath "$(pwd)" -importPackage "$(pwd)/UnityAds.unitypackage" -batchMode -quit
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Importing package failed. Please check ~/Library/Logs/Unity/Editor.log"
        exit $rc
    fi
fi

if [ "$1" == "android" ]; then
    echo Building and running project with Unity $2 \(Android\)...
    rm Builds/Android.apk
    "$UNITY" -projectPath $(pwd) -executeMethod AutoBuilder.PerformAndroidBuild -batchMode -quit
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Unity build for Android failed. Please check ~/Library/Logs/Unity/Editor.log"
        exit $rc
    fi

    echo Installing on Android device...
    adb install -r Builds/Android.apk

    adb shell am start -S -a android.intent.action.MAIN -n com.unity3d.UnityAdsAssetStoreTest/com.unity3d.player.UnityPlayerActivity
fi

if [ "$1" == "ios" ]; then
    echo Building project with Unity $2 \(iOS\)...
    rm -rf Builds/iOS
    "$UNITY" -projectPath $(pwd) -executeMethod AutoBuilder.PerformiOSBuild -batchMode -quit
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Unity build for iOS failed. Please check ~/Library/Logs/Unity/Editor.log"
        exit $rc
    fi

    open Builds/iOS/Unity-iPhone.xcodeproj
    # xcodebuild -project Builds/iOS/Unity-iPhone.xcodeproj
fi
