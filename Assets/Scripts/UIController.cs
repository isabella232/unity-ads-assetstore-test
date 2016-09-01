using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class UIController : MonoBehaviour
{
	#if UNITY_ANDROID
	public const string AdsGameId = "14851";
	#else
	public const string AdsGameId = "14850";
	#endif

	public UnityEngine.UI.InputField GameIdInput;
	public UnityEngine.UI.Button InitializeButton;
	public UnityEngine.UI.Button ShowDefaultAdButton;
	public UnityEngine.UI.Button ShowRewardedAdButton;
	public UnityEngine.UI.Text LogText;
	public GameObject ConfigPanel;
	public UnityEngine.UI.InputField RewardedAdZoneIdInput;
	public UnityEngine.UI.Toggle TestModeToggle;
	public Transform BackgroundImage;

	private float adsInitializeTime;
	private bool adsInitialized;

	private const string GameIdPlayerPrefsKey = "GameId";
	private const string RewardedAdPlacementIdPlayerPrefsKey = "RewardedAdPlacementId";

	void Start ()
	{
		Log (string.Format ("Unity version: {0}, Ads version: {1}", Application.unityVersion, Advertisement.version));

		// manually scale background picture
		float scaleX = (0.75f/960) * (float)Screen.width;
		float scaleY = (0.75f/375) * (float)Screen.height;
		float scale = Mathf.Max (scaleX, scaleY);
		BackgroundImage.localScale = new Vector2 (scale, scale);

		ConfigPanel.SetActive (false);
		if (PlayerPrefs.HasKey (GameIdPlayerPrefsKey))
		{
			GameIdInput.text = PlayerPrefs.GetString (GameIdPlayerPrefsKey);
		}
		else
		{
			GameIdInput.text = AdsGameId;
		}

		if (PlayerPrefs.HasKey (RewardedAdPlacementIdPlayerPrefsKey))
		{
			RewardedAdZoneIdInput.text = PlayerPrefs.GetString (RewardedAdPlacementIdPlayerPrefsKey);
		}
		else
		{
			RewardedAdZoneIdInput.text = "rewardedVideo";
		}
	}

	void Update ()
	{
		if (!adsInitialized && AdPlacementReady ())
			adsInitialized = true; // has ads been available at some point? used to see if we managed to initialize correctly

		UpdateUI ();
	}

	public void UpdateUI ()
	{
		GameIdInput.interactable = !adsInitialized;
		InitializeButton.interactable = !adsInitialized;
		ShowDefaultAdButton.interactable = adsInitialized && AdPlacementReady();
		ShowRewardedAdButton.interactable = adsInitialized && AdPlacementReady(RewardedAdZoneIdInput.text);
	}

	public void Log (string message)
	{
		string text = string.Format ("{0:HH:mm:ss} {1}", DateTime.Now, message);
		Debug.Log ("=== " + text + " ===");
		LogText.text = string.Format ("{0}\n{1}", text, LogText.text);
	}

	public void InitializeAds ()
	{
		InitializeAds (GameIdInput.text, TestModeToggle.isOn);
		PlayerPrefs.SetString (GameIdPlayerPrefsKey, GameIdInput.text);
		PlayerPrefs.SetString (RewardedAdPlacementIdPlayerPrefsKey, RewardedAdZoneIdInput.text);

		adsInitializeTime = Time.time;
		Invoke ("CheckForAdsInitialized", 5);
	}

	private void CheckForAdsInitialized()
	{
		if (!adsInitialized)
		{
			float timeSinceInitialize = Time.time - adsInitializeTime;
			if (timeSinceInitialize > 30)
			{
				Log("Failed to initialize ads withing 30 seconds. Please verify you entered correct game id and placement ids and/or check device log for additional information");
				return;
			}

			Log (string.Format("Initializing - {0:#} secs...", timeSinceInitialize));
			Invoke("CheckForAdsInitialized", 5);
		}
	}

	public void ShowDefaultAd ()
	{
		ShowAd ();  // we want to make sure this also works, as game devs might typically show ads this way
	}

	public void ShowRewardedAd ()
	{
		ShowAd (RewardedAdZoneIdInput.text);
	}

	public void ShowCoroutineAd ()
	{
		StopAllCoroutines();
		StartCoroutine(ShowAdCouroutine());
	}

	public void ShowConfig ()
	{
		ConfigPanel.SetActive (true);
	}

	public void HideConfig ()
	{
		ConfigPanel.SetActive (false);
	}

	public void Quit ()
	{
		Application.Quit ();
	}

	internal IEnumerator ShowAdCouroutine()
	{
		float startTime = Time.time;

		if (!Advertisement.isInitialized)
		{
			Advertisement.Initialize(AdsGameId, true);
		}

		while (!Advertisement.IsReady())
		{
			float time = Time.time - startTime;
			yield return new WaitForSeconds(0.5f);

			// if unable to load the ad before timeout, give up and give the player their reward anyway
			if (time > 30.0f)
			{
				Log ("Failed to initialize ads");
				yield break;
			}
		}

		ShowOptions options = new ShowOptions();
		options.resultCallback = ShowAdResultCallback;
		Advertisement.Show(options);
	}

	private void InitializeAds (string gameId, bool testMode)
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

	private void ShowAd ()
	{
		ShowAd (null);
	}

	private void ShowAd (string placementId)
	{
		if (!Advertisement.isInitialized)
		{
			Log ("Ads hasn't been initialized yet. Cannot show ad");
			return;
		}

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
			resultCallback = ShowAdResultCallback
		};
		Advertisement.Show (placementId, options);
	}

	private void ShowAdResultCallback(ShowResult result)
	{
		Log ("Ad completed with result: " + result);
	}

	private bool AdPlacementReady()
	{
		return Advertisement.IsReady ();
	}

	private bool AdPlacementReady(string id)
	{
		return Advertisement.IsReady (id);
	}
}
