if [ -z "$1" ]; then
    echo "Unity version number not specified, e.g. '5.4.0f1' (must be installed on the machine)"
    exit
fi

echo Building project with Unity $1...
rm -rf Builds/Android.apk
"/Applications/Unity $1/Unity.app/Contents/MacOS/Unity" -executeMethod AutoBuilder.PerformAndroidBuild -projectPath $(pwd) -batchMode -quit

rc=$?; if [[ $rc != 0 ]]; then
    echo "Unity build failed. Please check ~/Library/Logs/Unity/Editor.log"
    exit $rc
fi

echo Installing on Android device...
adb install -r Builds/Android.apk

adb shell am start -S -a android.intent.action.MAIN -n com.unity3d.UnityAdsAssetStoreTest/com.unity3d.player.UnityPlayerActivity
