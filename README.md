# Unity Ads Asset Store test project

Test project for Unity Ads SDK using Asset Store package (<https://www.assetstore.unity3d.com/en/#!/content/66123>). Project is maintained using Unity 5.0, however can be opened in any later Unity version.

## How to use this project

First

1. Switch to relevant repo branch, e.g. `5.0`, using e.g. `git checkout 5.0` from command line (branch name indicates which Unity version is used to maintain project, can be opened in later Unity versions)

Then

1. Open `UnityAdsAssetStoreTest` project in Unity
1. Open MainScene
1. Open Asset Store window
1. Search for "Unity Ads" and download/import either Unity Ads SDK 1.x or 2.x
1. Set `UNITY_ADS_PACKAGE` scripting define, either in Player Settings, or from `File->AutoBuilder->Enable Ads` menu
1. Play in editor or deploy to your Android or iOS device

Alternatively see `build.sh` script for how to automate importing and building project.

## Logging

Unity Ads related device logs are written with topic `UnityAds`, e.g. to filter relevant logs on Android, use:

```
$ adb logcat -v time UnityAds:V *:S
```

## Support

Please use <http://forum.unity3d.com/forums/unity-ads.67> for questions related to this project.

Hope you find this project useful as example and test application for Unity Ads asset store package.

Best regards,  
Your Unity Ads team
