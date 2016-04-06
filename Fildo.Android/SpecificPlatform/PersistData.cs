using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Fildo.Droid;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using Fildo.Core.IPlatform;
using System.Collections.Generic;
using Android.Util;
using System.Linq;
using Java.IO;

namespace Fildo.Droid.SpecificPlatform
{
    public class PersistData : IPersist
    {
        private readonly ISharedPreferences prefs;
        public PersistData()
        {
            this.prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
        }
        public List<string> GetStringList(string key)
        {
            var temp = this.prefs.GetString(key, string.Empty);
            return temp.Split(';').ToList();
        }

        public string GetString(string key)
        {
            return this.prefs.GetString(key, string.Empty);
        }

        public void PersistStringList(List<string> toPersist, string key)
        {
            string value = String.Join(";", toPersist);
            var prefEditor = this.prefs.Edit();
            prefEditor.PutString(key, value);
            prefEditor.Commit();
        }
    }
}