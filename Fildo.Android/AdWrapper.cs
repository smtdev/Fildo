namespace Fildo.Droid
{
    using Android.Content;
    using Android.Gms.Ads;

    public static class AdWrapper
    {
        public static AdView ConstructStandardBanner(Context con, AdSize adsize, string UnitID)
        {
            var ad = new AdView(con) { AdSize = adsize, AdUnitId = UnitID };
            return ad;
        }

        public static AdView CustomBuild(this AdView ad)
        {
            var requestbuilder = new AdRequest.Builder();
            ad.LoadAd(requestbuilder.Build());
            return ad;
        }

        public static InterstitialAd ConstructFullPageAdd(Context con, string UnitID)
        {
            var ad = new InterstitialAd(con) { AdUnitId = UnitID };
            return ad;
        }

        public static InterstitialAd CustomBuild(this InterstitialAd ad)
        {
            var requestbuilder = new AdRequest.Builder();
            ad.LoadAd(requestbuilder.Build());
            return ad;
        }
    }
}