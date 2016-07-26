using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class Main : MonoBehaviour
{
	void Start ()
	{
		Log (String.Format ("Unity version: {0}, Ads version: {1}", Application.unityVersion, Advertisement.version));
	}

	void Update ()
	{
		UIController ui = UIController.Instance;

		if (ui)
		{
			ui.AdsInitialized = Advertisement.isInitialized;
		}
	}

	public static void InitializeAds (string gameId, bool testMode)
	{
		if (!Advertisement.isSupported)
		{
			Log ("Ads not supported on this platform");
			return;
		}

		if ((gameId == null) || (gameId.Trim ().Length == 0))
		{
			Log ("Please provide a game id");
			return;
		}

		Log (string.Format ("Initializing ads for game id {0}...", gameId));
		Advertisement.Initialize (gameId, testMode);
	}

	public static void ShowAd ()
	{
		ShowAd (null);
	}

	public static void ShowAd (string zoneId)
	{
		if (!Advertisement.isInitialized)
		{
			Log ("Ads hasn't been initialized yet. Cannot show ad");
			return;
		}

		if (!Advertisement.IsReady (zoneId))
		{
			if (zoneId == null)
			{
				Log ("Ads not ready for default zone. Please wait a few seconds and try again");
			}
			else
			{
				Log (string.Format("Ads not ready for zone '{0}'. Please wait a few seconds and try again", zoneId));
			}

			return;
		}

		ShowOptions options = new ShowOptions
		{
			resultCallback = ShowAdResultCallback
		};
		Advertisement.Show (zoneId, options);
	}

	private static void ShowAdResultCallback(ShowResult result)
	{
		Log (string.Format ("Ad completed with result: {0}", result));
	}

	public static void Log(string message)
	{
		string text = String.Format ("{0:HH:mm:ss} {1}", DateTime.Now, message);
		Debug.Log ("=== " + text + " ===");

		UIController ui = UIController.Instance;
		if (ui)
		{
			ui.Log (text);
		}
	}

	public static bool AdPlacementReady(string id)
	{
		return Advertisement.IsReady (id);
	}
}
