using UnityEngine;

public class UIController : MonoBehaviour
{
	public UnityEngine.UI.InputField GameIdInput;
	public UnityEngine.UI.Button InitializeButton;
	public UnityEngine.UI.Button ShowDefaultAdButton;
	public UnityEngine.UI.Button ShowRewardedAdButton;
	public UnityEngine.UI.Text LogText;
	public GameObject ConfigPanel;
	public UnityEngine.UI.InputField DefaultAdZoneIdInput;
	public UnityEngine.UI.InputField RewardedAdZoneIdInput;

	internal bool AdsInitialized;

	private bool hasShownRewardedAd;  // since we have bug where zone is not reset, so in that case pass zone id

	private const string GameIdPlayerPrefsKey = "GameId";
	private const string DefaultAdZoneIdPlayerPrefsKey = "DefaultAdZoneId";
	private const string RewardedAdZoneIdPlayerPrefsKey = "RewardedAdZoneId";

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
		ConfigPanel.SetActive (false);
		if (PlayerPrefs.HasKey (GameIdPlayerPrefsKey))
		{
			GameIdInput.text = PlayerPrefs.GetString (GameIdPlayerPrefsKey);
		}

		if (PlayerPrefs.HasKey (DefaultAdZoneIdPlayerPrefsKey)) {
			DefaultAdZoneIdInput.text = PlayerPrefs.GetString (DefaultAdZoneIdPlayerPrefsKey);
		}
		else
		{
			DefaultAdZoneIdInput.text = "defaultZone";
		}

		if (PlayerPrefs.HasKey (RewardedAdZoneIdPlayerPrefsKey)) {
			RewardedAdZoneIdInput.text = PlayerPrefs.GetString (RewardedAdZoneIdPlayerPrefsKey);
		}
		else
		{
			RewardedAdZoneIdInput.text = "rewardedVideoZone";
		}
	}

	void Update ()
	{
		UpdateUI ();
	}
	
	public void UpdateUI ()
	{
		InitializeButton.interactable = !AdsInitialized;
		ShowDefaultAdButton.interactable = AdsInitialized;
		ShowRewardedAdButton.interactable = AdsInitialized;
	}

	public void Log (string text)
	{
		LogText.text = string.Format ("{0}\n{1}", text, LogText.text);
	}

	public void InitializeAds ()
	{
		Main.InitializeAds (GameIdInput.text);
		PlayerPrefs.SetString (GameIdPlayerPrefsKey, GameIdInput.text);
		PlayerPrefs.SetString (DefaultAdZoneIdPlayerPrefsKey, DefaultAdZoneIdInput.text);
		PlayerPrefs.SetString (RewardedAdZoneIdPlayerPrefsKey, RewardedAdZoneIdInput.text);
	}

	public void ShowDefaultAd ()
	{
		if (!hasShownRewardedAd)
		{
			Main.ShowAd ();  // we want to make sure this also works, as game devs might typically show ads this way
		}
		else
		{
			Main.ShowAd (DefaultAdZoneIdInput.text);
		}
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
