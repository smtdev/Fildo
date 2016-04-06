using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Views;
using Fildo.Core.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using System.Linq.Expressions;
using Android.Support.Design.Widget;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Fildo.Droid.Bindables;
using MvvmCross.Droid.FullFragging.Fragments;

using MvvmCross.Binding.Droid.BindingContext;
using Fildo.Droid.Views;

namespace Fildo.Droid.Fragments
{
    using Fildo.Droid.Services;

    public class SearchResultView : MvxFragment
    {
        DrawerLayout menuDrawerLayout;
        private MvxActionBarDrawerToggle menuDrawerToggle;
        private Toolbar toolbar;
        private MvxRecyclerView ntResults;
        private MvxRecyclerView vkResults;
        private MvxRecyclerView qqResults;

        private BindableProgress progress;
        private BindablePopup popup;
        private MvxRecyclerView xiamiResults;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.AutoCompleteResultsView, null);

            if (!string.IsNullOrEmpty(((BaseViewModel)((MainView)this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView)this.Activity).FindViewById<Android.Widget.LinearLayout>(Resource.Id.miniPlayer).Visibility = ViewStates.Visible;
            }

            this.ntResults = view.FindViewById<MvxRecyclerView>(Resource.Id.ntresults);
            this.vkResults = view.FindViewById<MvxRecyclerView>(Resource.Id.vkresults);
            this.qqResults = view.FindViewById<MvxRecyclerView>(Resource.Id.qqresults);
            this.xiamiResults = view.FindViewById<MvxRecyclerView>(Resource.Id.xiamiresults);

            this.ntResults.Visibility = ViewStates.Visible;
            this.vkResults.Visibility = ViewStates.Gone;
            this.qqResults.Visibility = ViewStates.Gone;

            TabLayout tabs = view.FindViewById<TabLayout>(Resource.Id.tabs);
            tabs.Visibility = ViewStates.Visible;
            tabs.AddTab(tabs.NewTab().SetText("NETEASE"));
            tabs.AddTab(tabs.NewTab().SetText("HUAWEI"));
            tabs.AddTab(tabs.NewTab().SetText("VK"));
            tabs.AddTab(tabs.NewTab().SetText("QQMUSIC"));
            tabs.TabSelected += this.Tabs_TabSelected;

            this.progress = new BindableProgress(view.Context, this.ViewModel);
            this.popup = new BindablePopup(view.Context, this.ViewModel, view, this.Activity);
            this.popup.Dismissed += Popup_Dismissed;
            var set = this.CreateBindingSet<SearchResultView, SearchResultViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(this.popup).For(p => p.Visible).To(vm => vm.ShowCaptcha);
            set.Bind(this.popup).For(p => p.CaptchaUrl).To(vm => vm.CaptchaUrl);
            
            set.Apply();
            GAService.GetGASInstance().Track_App_Page("Results");
            return view;
        }

        private void Popup_Dismissed(object sender, bool isCaptchaResolved)
        {
            if (!isCaptchaResolved)
            {
                this.popup.Visible = true;
            }
        }
        
        private void Tabs_TabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            if (e.Tab.Position == 0)
            {
                this.ntResults.Visibility = ViewStates.Visible;
                this.vkResults.Visibility = ViewStates.Gone;
                this.qqResults.Visibility = ViewStates.Gone;
                this.xiamiResults.Visibility = ViewStates.Gone;
            }
            else if (e.Tab.Position == 1)
            {
                this.ntResults.Visibility = ViewStates.Gone;
                this.xiamiResults.Visibility = ViewStates.Visible;
                this.vkResults.Visibility = ViewStates.Gone;
                this.qqResults.Visibility = ViewStates.Gone;
            }
            else if (e.Tab.Position == 2)
            {
                this.ntResults.Visibility = ViewStates.Gone;
                this.qqResults.Visibility = ViewStates.Gone;
                this.vkResults.Visibility = ViewStates.Visible;
                this.xiamiResults.Visibility = ViewStates.Gone;
            }
            else
            {
                this.ntResults.Visibility = ViewStates.Gone;
                this.vkResults.Visibility = ViewStates.Gone;
                this.xiamiResults.Visibility = ViewStates.Gone;
                this.qqResults.Visibility = ViewStates.Visible;
            }
        }
    }
}