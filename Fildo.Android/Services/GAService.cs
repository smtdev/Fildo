namespace Fildo.Droid.Services
{
    using Android.Content;
    using Android.Gms.Analytics;

    public class GAService
    {
        public string TrackingId = "UA-75007385-1";

        private static GoogleAnalytics GAInstance;
        private static Tracker GATracker;

        #region Instantiation ...
        private static GAService thisRef;
        private GAService()
        {
            // no code req'd
        }

        public static GAService GetGASInstance()
        {
            if (thisRef == null)
                // it's ok, we can call this constructor
                thisRef = new GAService();
            return thisRef;
        }
        #endregion

        public void Initialize(Context AppContext)
        {
            GAInstance = GoogleAnalytics.GetInstance(AppContext.ApplicationContext);
            GAInstance.SetLocalDispatchPeriod(10);

            GATracker = GAInstance.NewTracker(TrackingId);
            GATracker.EnableExceptionReporting(true);
            GATracker.EnableAdvertisingIdCollection(true);
            GATracker.EnableAutoActivityTracking(true);
        }

        public void Track_App_Page(string PageNameToTrack)
        {
            GATracker.SetScreenName(PageNameToTrack);
            GATracker.Send(new HitBuilders.ScreenViewBuilder().Build());
        }

        public void Track_App_Event(string GAEventCategory, string EventToTrack)
        {
            HitBuilders.EventBuilder builder = new HitBuilders.EventBuilder();
            builder.SetCategory(GAEventCategory);
            builder.SetAction(EventToTrack);
            builder.SetLabel("AppEvent");

            GATracker.Send(builder.Build());
        }

        public void Track_App_Exception(string ExceptionMessageToTrack, bool isFatalException)
        {
            HitBuilders.ExceptionBuilder builder = new HitBuilders.ExceptionBuilder();
            builder.SetDescription(ExceptionMessageToTrack);
            builder.SetFatal(isFatalException);

            GATracker.Send(builder.Build());
        }
    }
}