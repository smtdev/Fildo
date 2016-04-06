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
    public class BindableProgress
    {
        private readonly Context context;
        private readonly IMvxViewModel viewModel;
        private readonly CultureInfo culture;
        public BindableProgress(Context context, IMvxViewModel viewModel)
        {
            this.context = context;
            this.viewModel = viewModel;
            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.culture = new CultureInfo(prefs.GetString("CultureForced", System.Threading.Thread.CurrentThread.CurrentUICulture.Name));
        }

        private ProgressDialog dialog;

        public bool Visible {
            get { return this.dialog != null;  }
            set
            {
                if (value == this.Visible)
                {
                    return;
                }

                if (value)
                {
                    this.dialog = new ProgressDialog(this.context);
                    this.dialog.Show();
                }
                else
                {
                    this.dialog.Hide();
                    this.dialog = null;
                }
            }
        }

        public bool NoInternet
        {
            get { return this.dialog != null; }
            set
            {
                if (value == this.Visible)
                {
                    return;
                }

                if (value)
                {
                    this.dialog = new ProgressDialog(this.context);
                    this.dialog.SetMessage(((BaseViewModel)this.viewModel).GetString("NoInternet", this.culture));
                    this.dialog.SetCancelable(false);
                    this.dialog.Show();
                }
                else
                {
                    this.dialog.Hide();
                    this.dialog = null;
                }
            }
        }

        public bool NewVersion
        {
            get { return this.dialog != null; }
            set
            {
                if (value == this.NewVersion)
                {
                    return;
                }

                if (value)
                {
                    AlertDialog ad = new AlertDialog.Builder(this.context).Create();
                    ad.SetTitle("INFO");
                    
                    ad.SetMessage(((BaseViewModel)this.viewModel).GetString("NewVersion", this.culture));
                    ad.SetCancelable(false);
                    ad.SetCanceledOnTouchOutside(false);
                    ad.SetButton(((BaseViewModel)this.viewModel).GetString("DownloadIt", this.culture), delegate
                    {
                        var uri = Android.Net.Uri.Parse("http://fildo.net/android");
                        var intent = new Intent(Intent.ActionView, uri);
                        this.context.StartActivity(intent);
                    });
                   
                    ad.Show();
                }
                else
                {
                    this.dialog.Hide();
                    this.dialog = null;
                }
            }
        }
    }
}