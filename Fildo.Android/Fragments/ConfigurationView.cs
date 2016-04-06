namespace Fildo.Droid.Fragments
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Support.V4.Widget;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Core.ViewModels;
    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.Views;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using System.Globalization;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Droid.FullFragging.Fragments;
    using MvvmCross.Droid.Support.V7.RecyclerView;
    using Views;
    using System.Linq;
    using System.Collections.Generic;
    using Acr.UserDialogs;
    using MvvmCross.Platform;
    using System;

    using Fildo.Droid.Services;

    [Activity(Label = "Configuration", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class ConfigurationView : MvxFragment
    {
        private string culture;
        private Bindables.BindableProgress progress;
        private CultureInfo cultureInfo;
        private bool saved;
        private Android.Widget.CheckBox saveUnderArtistFolder;
        private Android.Widget.CheckBox saveUnderExternalSd;
        private MvxSpinner spinnerProxies;
        private string proxy;


        public bool Saved
        {
            get { return this.saved; }
            set
            {
                this.saved = value;
                if (value)
                {
                    var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
                    var prefEditor = prefs.Edit();
                    prefEditor.PutBoolean("SaveUnderArtistFolder", this.saveUnderArtistFolder.Checked);
                    prefEditor.PutBoolean("SaveExternalSD", this.saveUnderExternalSd.Checked);
                    if (!string.IsNullOrEmpty(this.culture))
                    {
                        prefEditor.PutString("CultureForced", this.culture);
                    }
                    if (!string.IsNullOrEmpty(this.proxy))
                    {
                        prefEditor.PutBoolean("UseProxy", true);
                        prefEditor.PutString("ProxyConf", this.proxy);
                    }
                    else
                    {
                        prefEditor.PutBoolean("UseProxy", false);
                        prefEditor.PutString("ProxyConf", this.proxy);
                    }

                    prefEditor.Commit();
                }
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            View view = this.BindingInflate(Resource.Layout.Configuration, null);
            
            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.cultureInfo = ((MainView)this.Activity).CultureInfo;

            string cultureForced = prefs.GetString("CultureForced", this.culture);
            string proxyForced = prefs.GetString("ProxyConf", string.Empty);

            if (!string.IsNullOrEmpty(((BaseViewModel)((MainView)this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView)this.Activity).FindViewById<Android.Widget.LinearLayout>(Resource.Id.miniPlayer).Visibility = ViewStates.Visible;
            }

            if (this.ViewModel != null)
            {
                view.FindViewById<Android.Widget.CheckBox>(Resource.Id.checkboxsavealbum).Text = ((BaseViewModel) this.ViewModel).GetString("SaveUnderFolder", this.cultureInfo);
                view.FindViewById<Android.Widget.CheckBox>(Resource.Id.checkboxexternal).Text = ((BaseViewModel) this.ViewModel).GetString("SaveExternalSD", this.cultureInfo);
                view.FindViewById<Android.Widget.Button>(Resource.Id.BtnSave).Text = ((BaseViewModel) this.ViewModel).GetString("Save", this.cultureInfo);
                view.FindViewById<Android.Widget.TextView>(Resource.Id.spinnerCultureText).Text = ((BaseViewModel) this.ViewModel).GetString("SpinnerCultureText", this.cultureInfo);
                view.FindViewById<Android.Widget.TextView>(Resource.Id.spinnerProxyText).Text = ((BaseViewModel) this.ViewModel).GetString("SpinnerProxyText", this.cultureInfo);
            }

            var spinnerCulture = view.FindViewById<MvxSpinner>(Resource.Id.spinnerCulture);
            spinnerCulture.ItemSelected += this.SpinnerCulture_ItemSelected;
            this.spinnerProxies = view.FindViewById<MvxSpinner>(Resource.Id.spinnerProxy);
            this.spinnerProxies.ItemSelected += this.SpinnerProxies_ItemSelected;

            int cfTemp = ((List<string>)spinnerCulture.ItemsSource).IndexOf(cultureForced);
            if (cfTemp > -1)
            {
                spinnerCulture.SetSelection(cfTemp);
            }

            if (!string.IsNullOrEmpty(proxyForced))
            {
                int pxTemp = ((List<string>) this.spinnerProxies.ItemsSource).IndexOf(proxyForced);
                if (pxTemp > -1)
                {
                    this.spinnerProxies.SetSelection(pxTemp);
                }
                else
                {
                    this.spinnerProxies.SetSelection(0);
                }
            }
            else
            {
                this.spinnerProxies.SetSelection(0);
            }
            

            this.progress = new Bindables.BindableProgress(view.Context, this.ViewModel);

            var set = this.CreateBindingSet<ConfigurationView, ConfigurationViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(this.progress).For(p => p.NoInternet).To(vm => vm.NoInternet);
            set.Bind(this).For(p => p.Saved).To(vm => vm.Saved);
            set.Apply();

            this.saveUnderArtistFolder = view.FindViewById<Android.Widget.CheckBox>(Resource.Id.checkboxsavealbum);
            this.saveUnderExternalSd = view.FindViewById<Android.Widget.CheckBox>(Resource.Id.checkboxexternal);

            this.saveUnderArtistFolder.Checked = prefs.GetBoolean("SaveUnderArtistFolder", false);
            this.saveUnderExternalSd.Checked = prefs.GetBoolean("SaveExternalSD", false);
            GAService.GetGASInstance().Track_App_Page("Configuration");
            return view;
        }

        private void SpinnerProxies_ItemSelected(object sender, Android.Widget.AdapterView.ItemSelectedEventArgs e)
        {
            var selected = ((List<string>)((MvxSpinner)sender).ItemsSource)[e.Position];
            string text = selected.ToString();
            if (text.ToLower().Contains("proxy"))
            {
                this.proxy = null;
            }
            else
            {
                this.proxy = text;
            }
        }

        private void SpinnerCulture_ItemSelected(object sender, Android.Widget.AdapterView.ItemSelectedEventArgs e)
        {
            if (e.Id == 0)
            {
                this.culture = string.Empty;
            }
            else if (e.Id == 1)
            {
                this.culture = "es";
            }
            else if (e.Id == 2)
            {
                this.culture = "en";
            }
            else if (e.Id == 3)
            {
                this.culture = "ca-ES";
            }
            else if (e.Id == 4)
            {
                this.culture = "fr";
            }
            else if (e.Id == 5)
            {
                this.culture = "de";
            }
            else if (e.Id == 6)
            {
                this.culture = "it";
            }
            else if (e.Id == 7)
            {
                this.culture = "pt";
            }
        }
    }
}