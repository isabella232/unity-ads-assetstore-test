using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class UIController : MonoBehaviour
{
	#if UNITY_ANDROID
	private const string AdsGameId = "";
	#else
	private const string AdsGameId = "";
	#endif

	public UnityEngine.UI.InputField GameIdInput;
	public UnityEngine.UI.Button InitializeButton;
	public UnityEngine.UI.Button ShowDefaultAdButton;
	public UnityEngine.UI.Button ShowRewardedAdButton;
	public UnityEngine.UI.Text LogText;
	public GameObject ConfigPanel;
	public UnityEngine.UI.InputField RewardedAdPlacementIdInput;
	public UnityEngine.UI.Toggle TestModeToggle;
	public UnityEngine.UI.Text ToggleAudioButtonText;

	private float adsInitializeTime;
	private bool adsInitialized;

	private const string GameIdPlayerPrefsKey = "GameId";
	private const string RewardedAdPlacementIdPlayerPrefsKey = "RewardedAdPlacementId";

	void Start ()
	{
		ConfigPanel.SetActive (false);
		Log (string.Format ("Unity version: {0}, Ads version: {1}", Application.unityVersion, Advertisement.version));

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
			RewardedAdPlacementIdInput.text = PlayerPrefs.GetString (RewardedAdPlacementIdPlayerPrefsKey);
		}
		else
		{
			RewardedAdPlacementIdInput.text = "rewardedVideo";
		}
	}

	private void Log (string message)
	{
		string text = string.Format ("{0:HH:mm:ss} {1}", DateTime.Now, message);
		Debug.Log ("=== " + text + " ===");
		LogText.text = string.Format ("{0}\n{1}", text, LogText.text);
	}

	void Update ()
	{
		if (!adsInitialized && Advertisement.IsReady ())
			adsInitialized = true; // has ads been available at some point? used to see if we managed to initialize correctly

		UpdateUI ();
	}

	private void UpdateUI ()
	{
		GameIdInput.interactable = !adsInitialized;
		InitializeButton.interactable = !adsInitialized;
		TestModeToggle.interactable = !adsInitialized;
		ShowDefaultAdButton.interactable = adsInitialized && Advertisement.IsReady ();
		ShowRewardedAdButton.interactable = adsInitialized && (RewardedAdPlacementReady () != null);
	}

	public void InitializeAdsButtonClicked ()
	{
		InitializeAds (GameIdInput.text, TestModeToggle.isOn);
		PlayerPrefs.SetString (GameIdPlayerPrefsKey, GameIdInput.text);
		PlayerPrefs.SetString (RewardedAdPlacementIdPlayerPrefsKey, RewardedAdPlacementIdInput.text);

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

	public void ShowDefaultAdButtonClicked ()
	{
		ShowAd ();  // we want to make sure this also works, as game devs might typically show ads this way
	}

	public void ShowRewardedAdButtonClicked ()
	{
		string rewardedPlacementId = RewardedAdPlacementReady ();

		if (string.IsNullOrEmpty (rewardedPlacementId))
		{
			Log ("Rewarded ad not ready");
		}
		else
		{
			ShowAd (rewardedPlacementId);
		}
	}

	public void ShowCoroutineAdButtonClicked ()
	{
		StopAllCoroutines();
		StartCoroutine(ShowAdCouroutine());
	}

	public void ShowConfigButtonClicked ()
	{
		ConfigPanel.SetActive (true);
	}

	public void HideConfigButtonClicked ()
	{
		ConfigPanel.SetActive (false);
	}

	public void QuitButtonClicked ()
	{
		Application.Quit ();
	}

	internal IEnumerator ShowAdCouroutine()
	{
		float startTime = Time.time;

		if (!Advertisement.isInitialized)
		{
			Log ("Initializing ads from coroutine...");
			Advertisement.Initialize(GameIdInput.text, TestModeToggle.isOn);
		}

		while (!Advertisement.IsReady())
		{
			float time = Time.time - startTime;
			yield return new WaitForSeconds(0.5f);

			if (time > 30.0f)
			{
				Log ("Failed to initialize ads, please verify that you entered correct game id");
				yield break;
			}
		}

		ShowOptions options = new ShowOptions();
		options.resultCallback = ShowAdResultCallback;
		Advertisement.Show(null, options);
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

		if (placementId == null)
		{
			Log ("Showing ad for default placement");
		}
		else
		{
			Log (string.Format ("Showing ad for placement '{0}'", placementId));
		}

		Advertisement.Show (placementId, options);
	}

	private void ShowAdResultCallback(ShowResult result)
	{
		Log ("Ad completed with result: " + result);
	}

	private string RewardedAdPlacementReady ()
	{
		// default rewarded placement id has changed over time, check each of these
		string[] placementIds = { RewardedAdPlacementIdInput.text, "rewardedVideo", "rewardedVideoZone", "incentivizedZone" };

		foreach (var placementId in placementIds)
		{
			if (Advertisement.IsReady (placementId))
				return placementId;
		}

		return null;
	}

	public void ToggleAudio()
	{
		var audio = this.GetComponent<AudioSource>();

		if (audio.isPlaying)
		{
			audio.Stop();
			ToggleAudioButtonText.text = "Play audio";
		}
		else
		{
			audio.Play();
			ToggleAudioButtonText.text = "Mute audio";
		}
	}
}
