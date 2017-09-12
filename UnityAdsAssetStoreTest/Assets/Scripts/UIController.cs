using System;
using System.Collections;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public UnityEngine.UI.InputField GameIdInput;
    public UnityEngine.UI.Button InitializeButton;
    public UnityEngine.UI.Button ShowDefaultAdButton;
    public UnityEngine.UI.Button ShowRewardedAdButton;
    public UnityEngine.UI.Button ShowCoroutineAdButton;
    public UnityEngine.UI.Text LogText;
    public GameObject ConfigPanel;
    public UnityEngine.UI.InputField RewardedAdPlacementIdInput;
    public UnityEngine.UI.Toggle TestModeToggle;
    public UnityEngine.UI.Toggle DebugModeToggle;
    public UnityEngine.UI.Text ToggleAudioButtonText;
    public GameObject AdvancedModePanel;
    public UnityEngine.UI.Toggle AdvancedModeToggle;
    public UnityEngine.UI.Text FPS;
    public LoadTesting LoadTesting; // need this to call coroutines

    private static UIController instance = null;
    private float adsInitializeTime;
    private bool adsInitialized = false;
    private float deltaTime; // for FPS

    private const string GameIdPlayerPrefsKey = "GameId";
    private const string RewardedAdPlacementIdPlayerPrefsKey = "RewardedAdPlacementId";

    void Start ()
    {
        InvokeRepeating ("UpdateFPSText", 1, 1);
        ConfigPanel.SetActive (false);
        AdvancedModePanel.SetActive (AdvancedModeToggle.isOn);

        string message;
        if (Ads.IsEnabledAndSupported (out message))
        {
            Log (string.Format ("Unity version: {0}, Ads version: {1}", Application.unityVersion, Ads.Version));

            ConfigPanel.SetActive (false);
            if (PlayerPrefs.HasKey (GameIdPlayerPrefsKey))
            {
                GameIdInput.text = PlayerPrefs.GetString (GameIdPlayerPrefsKey);
            }
            else
            {
                GameIdInput.text = Ads.AdsGameId;
            }

            if (PlayerPrefs.HasKey (RewardedAdPlacementIdPlayerPrefsKey))
            {
                RewardedAdPlacementIdInput.text = PlayerPrefs.GetString (RewardedAdPlacementIdPlayerPrefsKey);
            }
            else
            {
                RewardedAdPlacementIdInput.text = "rewardedVideo";
            }
            DebugModeToggleClicked ();
        }
        else
        {
            Log (message);
            UpdateUI ();
            InitializeButton.interactable = false;
            ShowCoroutineAdButton.interactable = false;
            TestModeToggle.interactable = false;
            DebugModeToggle.interactable = false;
        }
    }

    public static UIController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIController> ();
            }
            return instance;
        }
    }

    public void Log (string message)
    {
        string text = string.Format ("{0:HH:mm:ss} {1}", DateTime.Now, message);
        Debug.Log ("=== " + text + " ===");
        LogText.text = string.Format ("{0}\n{1}", text, LogText.text);
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

    public void ToggleAudio ()
    {
        var audio = this.GetComponent<AudioSource> ();

        if (audio.isPlaying)
        {
            audio.Stop ();
            ToggleAudioButtonText.text = "Play audio";
        }
        else
        {
            audio.Play ();
            ToggleAudioButtonText.text = "Mute audio";
        }
    }

    public void AllocateMemory()
    {
        LoadTesting.Allocate100MB ();
    }

    public void ToggleCPULoad()
    {
        LoadTesting.ToggleCPULoad();
    }

    public void DebugModeToggleClicked ()
    {
        Ads.SetDebugMode (DebugModeToggle.isOn);
    }

    public void AdvancedModeToggleClicked ()
    {
        AdvancedModePanel.SetActive (AdvancedModeToggle.isOn);
    }

    private void UpdateUI ()
    {
        GameIdInput.interactable = !adsInitialized;
        InitializeButton.interactable = !adsInitialized;
        TestModeToggle.interactable = !adsInitialized;
        ShowDefaultAdButton.interactable = adsInitialized && Ads.DefaultAdPlacementReady ();
        ShowRewardedAdButton.interactable = adsInitialized && (Ads.RewardedAdPlacementReady (RewardedAdPlacementIdInput.text) != null);
    }

    void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Escape))
        {
            // use back button on Android to close config
            if (ConfigPanel.activeSelf)
                HideConfigButtonClicked ();
        }

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        #if UNITY_ADS_PACKAGE
        if (!adsInitialized && Ads.DefaultAdPlacementReady ())
            adsInitialized = true; // has ads been available at some point? used to see if we managed to initialize correctly
        #endif

        UpdateUI ();
    }

    private void UpdateFPSText()
    {
        float fps = 1.0f / deltaTime;
        FPS.text = fps.ToString("FPS: 0.0");
    }

    public void InitializeAdsButtonClicked ()
    {
        Ads.InitializeAds (GameIdInput.text, TestModeToggle.isOn);
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
        Ads.ShowAd ();  // we want to make sure this also works, as game devs might typically show ads this way
    }

    public void ShowRewardedAdButtonClicked ()
    {
        string rewardedPlacementId = Ads.RewardedAdPlacementReady (RewardedAdPlacementIdInput.text);

        if (string.IsNullOrEmpty (rewardedPlacementId))
        {
            Log ("Rewarded ad not ready");
        }
        else
        {
            Ads.ShowAd (rewardedPlacementId);
        }
    }

    public void ShowCoroutineAdButtonClicked ()
    {
        StopAllCoroutines();
        StartCoroutine(ShowAdCouroutine());
    }

    internal IEnumerator ShowAdCouroutine()
    {
        float startTime = Time.time;

        if (!Ads.IsInitialized)
        {
            Log ("Initializing ads from coroutine...");
            Ads.InitializeAds(GameIdInput.text, TestModeToggle.isOn);
        }

        while (!Ads.DefaultAdPlacementReady ())
        {
            float time = Time.time - startTime;
            yield return new WaitForSeconds(0.5f);

            if (time > 30.0f)
            {
                Log ("Failed to initialize ads, please verify that you entered correct game id");
                yield break;
            }
        }

        Ads.ShowAd ();
    }
}
