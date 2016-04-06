namespace Fildo.Droid.Bindables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using Core.ViewModels;
    using System.Globalization;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Binding.Droid.Views;
    using Android.Util;

    using Fildo.Droid.Views;

    public class BindablePopup : MvxNotifyPropertyChanged
    {
        private readonly Context context;
        private readonly IMvxViewModel viewModel;
        private readonly CultureInfo culture;
        private readonly View view;
        private PopupWindow pw;
        private string captchaUrl;
        private Activity activity;

        public bool IsResolved { get; set; }


        public event EventHandler<bool> Dismissed;

        public BindablePopup(Context context, IMvxViewModel viewModel, View view, Activity activity)
        {
            this.context = context;
            this.viewModel = viewModel;
            this.view = view;

            this.activity = activity;
            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.culture = new CultureInfo(prefs.GetString("CultureForced", System.Threading.Thread.CurrentThread.CurrentUICulture.Name));
        }
        
        /// <summary>
        /// Establece u obtiene el valor para CaptchaUrl
        /// </summary>
        /// <value>
        /// El valor de CaptchaUrl.
        /// </value>
        public string CaptchaUrl
        {
            get
            {
                return this.captchaUrl;
            }

            set
            {
                this.captchaUrl = value;
                this.RaisePropertyChanged(() => this.CaptchaUrl);
            }
        }
        
        public bool Visible {
            get { return this.pw != null;  }
            set
            {
                if (value)
                {
                    LayoutInflater inflater = (LayoutInflater) this.context.GetSystemService(Context.LayoutInflaterService);
                    var dpsize = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 400, this.view.Resources.DisplayMetrics);

                    this.pw = new PopupWindow(
                        inflater.Inflate(Resource.Layout.PopupCaptcha, null, false),
                        LinearLayout.LayoutParams.MatchParent,
                        LinearLayout.LayoutParams.MatchParent,
                        true);

                    ((MvxImageView) this.pw.ContentView.FindViewById(Resource.Id.CaptchaImage)).ImageUrl = this.CaptchaUrl;

                    ((Button) this.pw.ContentView.FindViewById(Resource.Id.BntCaptcha)).Click += this.BindablePopup_Click;
                    View container = this.activity.FindViewById(Resource.Id.mainContainer);
                    this.pw.ShowAtLocation(container, GravityFlags.Fill, 0, 0);
                    this.pw.DismissEvent += Pw_DismissEvent;
                    this.IsResolved = false;
                }
                else
                {
                    this.pw.Dismiss();
                    this.pw = null;
                    this.IsResolved = true;
                }
            }
        }

        private void Pw_DismissEvent(object sender, EventArgs e)
        {
            this.Dismissed?.Invoke(sender, this.IsResolved);
        }

        private void BindablePopup_Click(object sender, EventArgs e)
        {
            ((SearchResultViewModel)this.viewModel).ResolveCaptcha(((EditText)this.pw.ContentView.FindViewById(Resource.Id.CaptchaText)).Text);
            this.IsResolved = true;
            this.pw.Dismiss();
            this.pw = null;
        }
    }
}