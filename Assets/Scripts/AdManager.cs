using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour,
    IUnityAdsInitializationListener,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    public static AdManager Instance { get; private set; }

    // Placeholder store credentials. Replace with real Unity Dashboard values before release,
    // and turn TestMode off for production builds.
    [SerializeField] private string iosGameId = "1234567";
    [SerializeField] private string rewardedAdUnitId = "Rewarded_iOS";
    [SerializeField] private bool testMode = true;

    public bool IsRewardedAdReady { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        if (Advertisement.isInitialized)
        {
            LoadRewardedAd();
            return;
        }

        Advertisement.Initialize(iosGameId, testMode, this);
    }

    public void LoadRewardedAd()
    {
        IsRewardedAdReady = false;
        Advertisement.Load(rewardedAdUnitId, this);
    }

    public void ShowRewardedAd()
    {
        Advertisement.Show(rewardedAdUnitId, this);
    }

    // IUnityAdsInitializationListener
    public void OnInitializationComplete()
    {
        Debug.Log("[AdManager] Unity Ads initialization complete.");
        LoadRewardedAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"[AdManager] Unity Ads initialization failed: {error} - {message}");
    }

    // IUnityAdsLoadListener
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId != rewardedAdUnitId)
        {
            return;
        }

        IsRewardedAdReady = true;
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        IsRewardedAdReady = false;
        Debug.LogError($"[AdManager] Failed to load ad unit {adUnitId}: {error} - {message}");
    }

    // IUnityAdsShowListener
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"[AdManager] Failed to show ad unit {adUnitId}: {error} - {message}");
        LoadRewardedAd();
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId != rewardedAdUnitId)
        {
            return;
        }

        IsRewardedAdReady = false;

        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            GameManager.Instance?.OnRevivalAdCompleted();
        }

        LoadRewardedAd();
    }
}
