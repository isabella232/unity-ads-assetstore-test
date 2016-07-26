using UnityEngine;

public class UIController : MonoBehaviour
{
	#if UNITY_ANDROID
	const string DefaultGameId = "1053943";
	#else
	const string DefaultGameId = "1065097";
	#endif

	public UnityEngine.UI.InputField GameIdInput;
	public UnityEngine.UI.Button InitializeButton;
	public UnityEngine.UI.Button ShowDefaultAdButton;
	public UnityEngine.UI.Button ShowRewardedAdButton;
	public UnityEngine.UI.Text LogText;
	public GameObject ConfigPanel;
	public UnityEngine.UI.InputField DefaultAdZoneIdInput;
	public UnityEngine.UI.InputField RewardedAdZoneIdInput;
	public UnityEngine.UI.Toggle TestModeToggle;
	public Transform BackgroundImage;

	internal bool AdsInitialized;

	private bool hasShownRewardedAd;  // since we have bug where zone is not reset, so in that case pass zone id
	private float adsInitializeTime;
	private bool adsInitialized;

	private const string GameIdPlayerPrefsKey = "GameId";
	private const string DefaultAdPlacementIdPlayerPrefsKey = "DefaultAdPlacementId";
	private const string RewardedAdPlacementIdPlayerPrefsKey = "RewardedAdPlacementId";

	private static UIController instance = null;
	public static UIController Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<UIController>();
			}
			return instance;
		}
	}

	void Start ()
	{
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
			GameIdInput.text = DefaultGameId;
		}

		if (PlayerPrefs.HasKey (DefaultAdPlacementIdPlayerPrefsKey))
		{
			DefaultAdZoneIdInput.text = PlayerPrefs.GetString (DefaultAdPlacementIdPlayerPrefsKey);
		}
		else
		{
			DefaultAdZoneIdInput.text = "video";
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
		if (!adsInitialized && Main.AdPlacementReady (DefaultAdZoneIdInput.text))
			adsInitialized = true; // has ads been available at some point? used to see if we managed to initialize correctly

		UpdateUI ();
	}
	
	public void UpdateUI ()
	{
		GameIdInput.interactable = !AdsInitialized;
		InitializeButton.interactable = !AdsInitialized;
		ShowDefaultAdButton.interactable = AdsInitialized && Main.AdPlacementReady(DefaultAdZoneIdInput.text);
		ShowRewardedAdButton.interactable = AdsInitialized && Main.AdPlacementReady(RewardedAdZoneIdInput.text);
	}

	public void Log (string text)
	{
		LogText.text = string.Format ("{0}\n{1}", text, LogText.text);
	}

	public void InitializeAds ()
	{
		Main.InitializeAds (GameIdInput.text, TestModeToggle.isOn);
		PlayerPrefs.SetString (GameIdPlayerPrefsKey, GameIdInput.text);
		PlayerPrefs.SetString (DefaultAdPlacementIdPlayerPrefsKey, DefaultAdZoneIdInput.text);
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
		Main.ShowAd ();  // we want to make sure this also works, as game devs might typically show ads this way
	}

	public void ShowRewardedAd ()
	{
		hasShownRewardedAd = true;
		Main.ShowAd (RewardedAdZoneIdInput.text);
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
}
