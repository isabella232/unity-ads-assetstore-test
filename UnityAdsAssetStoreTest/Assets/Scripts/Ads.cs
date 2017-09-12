using UnityEngine;
#if UNITY_ADS_PACKAGE
using UnityEngine.Advertisements;
#endif

public class Ads
{
    #if UNITY_ANDROID
    public const string AdsGameId = "";
    #else
    public const string AdsGameId = "";
    #endif

    public static string Version
    {
        get
        {
            #if UNITY_ADS_PACKAGE
            return Advertisement.version;
            #else
            return string.Empty;
            #endif
        }
    }

    public static bool IsEnabledAndSupported (out string message)
    {
        #if !UNITY_ADS_PACKAGE
        message = "Ads not enabled. Set UNITY_ADS_PACKAGE define (or use File->AutoBuilder menu to set it)";
        return false;
        #else
        #if UNITY_ANDROID || UNITY_IOS
        message = string.Empty;
        return true;
        #else
        message = "Ads is only supported on iOS and Android. Please change build target";
        return false;
        #endif
        #endif
    }

    public static bool IsInitialized
    {
        #if UNITY_ADS_PACKAGE
        get { return Advertisement.isInitialized; }
        #else
        get { return false; }
        #endif
    }

    public static void SetDebugMode (bool value)
    {
        #if UNITY_ADS_PACKAGE
        // TODO: Doesn't work with SDK 1.5 - SDK specific conditional could solve this?
        Advertisement.debugMode = value;
        Log ("Debug mode: " + Advertisement.debugMode);
        #endif
    }

    public static bool DefaultAdPlacementReady ()
    {
        #if UNITY_ADS_PACKAGE
        return Advertisement.IsReady ();
        #else
        return false;
        #endif
    }

    public static string RewardedAdPlacementReady (string customRewardedPlacementId)
    {
        #if UNITY_ADS_PACKAGE
        // default rewarded placement id has changed over time, check each of these
        string[] placementIds = { customRewardedPlacementId, "rewardedVideo", "rewardedVideoZone", "incentivizedZone" };

        foreach (var placementId in placementIds)
        {
            if (Advertisement.IsReady (placementId))
                return placementId;
        }
        #endif
        return null;
    }

    public static void InitializeAds (string gameId, bool testMode)
    {
        string message;
        if (!IsEnabledAndSupported(out message))
        {
            Log (message);
            return;
        }

        if ((gameId == null) || (gameId.Trim ().Length == 0))
        {
            Log ("Please provide a game id");
            return;
        }

        #if UNITY_ADS_PACKAGE
        Log (string.Format ("Initializing ads for game id {0}...", gameId));
        Advertisement.Initialize (gameId, testMode);
        #endif
    }

    public static void ShowAd ()
    {
        ShowAd (null);
    }

    public static void ShowAd (string placementId)
    {
        if (!IsInitialized)
        {
            Log ("Ads hasn't been initialized yet. Cannot show ad");
            return;
        }

        #if UNITY_ADS_PACKAGE
        if (!Advertisement.IsReady (placementId))
        {
            if (placementId == null)
            {
                Log ("Ads not ready for default placement. Please wait a few seconds and try again");
            }
            else
            {
                Log (string.Format("Ads not ready for placement '{0}'. Please wait a few seconds and try again", placementId));
            }

            return;
        }

        ShowOptions options = new ShowOptions
        {
            gamerSid = "example_player_id",
            resultCallback = ShowAdResultCallback
        };

        if (placementId == null)
        {
            Log ("Showing ad for default placement");
        }
        else
        {
            Log (string.Format ("Showing ad for placement '{0}'", placementId));
        }

        Advertisement.Show (placementId, options);
        #endif
    }

    #if UNITY_ADS_PACKAGE
    private static void ShowAdResultCallback(ShowResult result)
    {
        Log ("Ad completed with result: " + result);
    }
    #endif

    private static void Log(string message)
    {
        UIController.Instance.Log (message);
    }
}
