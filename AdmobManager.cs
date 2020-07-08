using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdmobManager
{
    bool userConsent = false;
    RewardedAd bonusReward;

    public AdmobManager()
    {
        Initialize();
    }

    public void Initialize()
    {
        Debug.Log("Starting admob");
        MobileAds.Initialize(initStatus =>
        {
            bonusReward = new RewardedAd("ca-app-pub-3940256099942544/5224354917"); // Change to: ca-app-pub-3116414566105309/1733180732
            Debug.Log("Admob loaded");
        });
    }

    public void TestReward()
    {
        AdRequest request = new AdRequest.Builder().Build();

        bonusReward.OnAdClosed += GetError;
        bonusReward.OnAdFailedToLoad += GetError;
        bonusReward.OnAdFailedToShow += GetError;

        bonusReward.OnUserEarnedReward += GetReward;
        bonusReward.LoadAd(request);

    }

    public void GetError(object sender, EventArgs args)
    {
        CanvasManager.Instance.ShowMessage("Muy mal");
    }

    public void GetReward(object sender, EventArgs args)
    {
        CanvasManager.Instance.ShowMessage("Muy bien");
    }

}
