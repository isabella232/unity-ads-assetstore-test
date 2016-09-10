# NOTE: This is a test script used by Unity Ads team to verify Ads SDK versions
# against different Unity versions. Parameters and usage of this script may change
# in future versions, but is included here as example of how to automate this
# process, which might be relevant to others

if [ -z "$2" ]; then
    echo "Build Unity Ads SDK example project on OS X"
    echo
    echo "./build.sh platform unity-path [Ads SDK version]"
    echo
    echo "Examples:"
    echo "  ./build.sh android \"/Application/Unity 5.4.0f3\""
    echo "  ./build.sh ios \"/Application/Unity 5.4.0f3\" 2.0.3"
    echo
    echo "Unity version e.g. \"5.4.0f3\" must be installed on the machine in specified folder"
    echo "If specifying SDK version (2.0.0 or higher), this will be downloaded and imported into the project automatically"
    exit 1
fi

UNITY="$2/Unity.app/Contents/MacOS/Unity"
if [ ! -f "$UNITY" ]; then
    echo "Could not find Unity executable in '$UNITY'. Please verify it's installed and available in that location"
    exit 1
fi

if [ ! -z "$3" ]; then
    SDK_URL="http://cdn.unityads.unity3d.com/unitypackage/$3/UnityAds.unitypackage"
    echo Downloading Ads SDK from $SDK_URL...
    curl -s -o UnityAds.unitypackage $SDK_URL
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Failed to download package"
        exit $rc
    fi

    # Import package into project
    echo "Importing Ads SDK plugin..."
    "$UNITY" -projectPath "$(pwd)" -importPackage "$(pwd)/UnityAds.unitypackage" -batchMode -quit
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Importing package failed. Please check ~/Library/Logs/Unity/Editor.log"
        exit $rc
    fi

    # UNITY_ADS define is used to be able to import plugin from command line without compile erros in project
    echo "Setting UNITY_ADS define..."
    "$UNITY" -projectPath "$(pwd)" -executeMethod AutoBuilder.EnableAds -batchMode -quit
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Defining UNITY_ADS failed. Please check ~/Library/Logs/Unity/Editor.log"
        exit $rc
    fi
fi

if [ "$1" == "android" ]; then
    echo Building project for Android...
    APK_PATH="Builds/Android.apk"
    if [ -f $APK_PATH ]; then
        rm $APK_PATH
    fi
    "$UNITY" -projectPath $(pwd) -executeMethod AutoBuilder.PerformAndroidBuild -batchMode -quit
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Unity build for Android failed. Please check ~/Library/Logs/Unity/Editor.log"
        exit $rc
    fi

    if [ ! -f $APK_PATH ]; then
        echo "Failed to build APK file ($APK_PATH)"
        exit 1
    fi

    # TODO: Enable automatic deployment on device
    # echo Installing on Android device...
    # adb install -r Builds/Android.apk
    # adb shell am start -S -a android.intent.action.MAIN -n com.unity3d.UnityAdsAssetStoreTest/com.unity3d.player.UnityPlayerActivity
fi

if [ "$1" == "ios" ]; then
    echo Building project for iOS...
    if [ -d Builds/iOS ]; then
        rm -rf Builds/iOS
    fi
    "$UNITY" -projectPath $(pwd) -executeMethod AutoBuilder.PerformiOSBuild -batchMode -quit
    rc=$?; if [[ $rc != 0 ]]; then
        echo "Unity build for iOS failed. Please check ~/Library/Logs/Unity/Editor.log"
        exit $rc
    fi

    # open Builds/iOS/Unity-iPhone.xcodeproj
    # xcodebuild -project Builds/iOS/Unity-iPhone.xcodeproj
fi
