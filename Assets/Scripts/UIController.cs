using UnityEngine;

public class UIController : MonoBehaviour
{
	public UnityEngine.UI.Text GameIdInput;
	public UnityEngine.UI.Button InitializeButton;
	public UnityEngine.UI.Button ShowDefaultAdButton;
	public UnityEngine.UI.Button ShowRewardedAdButton;
	public UnityEngine.UI.Text LogText;

	internal bool AdsInitialized;

	private bool hasShownRewardedAd;  // since we have bug where zone is not reset, so in that case pass zone id

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

	void Update()
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

	public void InitializeAds()
	{
		Main.InitializeAds (GameIdInput.text);
	}

	public void ShowDefaultAd()
	{
		if (!hasShownRewardedAd)
		{
			Main.ShowAd ();  // we want to make sure this also works, as game devs might typically show ads this way
		}
		else
		{
			Main.ShowAd ("defaultZone");
		}
	}

	public void ShowRewardedAd()
	{
		hasShownRewardedAd = true;
		Main.ShowAd ("rewardedVideoZone");
	}

	public void Quit()
	{
		Application.Quit ();
	}
}
