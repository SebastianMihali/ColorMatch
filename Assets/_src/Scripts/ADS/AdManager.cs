using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener,IUnityAdsShowListener
{
    [SerializeField] private bool _isTest;
    
    [Header("GameID")]
    [SerializeField] private string _androidGameId;
    [SerializeField] private string _iosGameId;
    private string _gameId;
    
    [Header("InterstitialAd")]
    [SerializeField] private string _androidInterstitialId;
    [SerializeField] private string _iosInterstitialId;
    private string _interstitialId;
    private bool _hasInterstitialAd;
    
    [Header("GameID")]
    [SerializeField] private string _androidRewardId;
    [SerializeField] private string _iosRewardId;
    private string _rewardId;
    private bool _hasRewardAd;
    
    private bool _isInitialized;
    public bool IsInitialized { get; }

    private Action<bool> _rewardAdCallback;
    
    private void Awake()
    {
        _isInitialized = false;
        _hasInterstitialAd = false;
        _hasRewardAd = false;
        
        #if UNITY_IOS
            _gameId = _iosGameId;
            _interstitialId = _iosInterstitialId;
            _rewardId = _iosRewardId;
        #elif UNITY_ANDROID || UNITY_EDITOR
            _gameId = _androidGameId;
            _interstitialId = _androidInterstitialId;
            _rewardId = _androidRewardId;
        #endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId,_isTest,this);
        }
    }

    public void OnInitializationComplete()
    {
        _isInitialized = true;
        LoadInterstitialAd();
        LoadRewardAd();
    }
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        _isInitialized = false;
    }

    private void LoadInterstitialAd()
    {
        Advertisement.Load(_interstitialId,this);
    }
    private void LoadRewardAd()
    {
        Advertisement.Load(_rewardId,this);
    }
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId == _interstitialId)
        {
            _hasInterstitialAd = true;
        }
        if (placementId == _rewardId)
        {
            _hasRewardAd = true;
        }
    }
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading {placementId} ad : {message} ");
    }

    public void ShowInterstitialAd()
    {
        if(!_hasInterstitialAd) return;
        Advertisement.Show(_interstitialId,this);
    }
    public void ShowRewardAd()
    {
        if(!_hasRewardAd) return;
        Advertisement.Show(_rewardId,this);
    }
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing {placementId} ad : {message} ");
    }
    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"start showing {placementId} ad");
    }
    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"{placementId} ad click");
    }
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == _interstitialId)
        {
            _hasInterstitialAd = false;
            LoadInterstitialAd();
        }

        if (placementId == _rewardId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                _rewardAdCallback.Invoke(true);
            }
            else
            {
                _rewardAdCallback.Invoke(false);
            }
            
            _hasRewardAd = false;
            LoadRewardAd();
        }
    }

    public void SetRewardAdCallback(Action<bool> callback) => _rewardAdCallback = callback;
}
