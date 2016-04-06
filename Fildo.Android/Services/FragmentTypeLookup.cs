namespace Fildo.Droid.Services
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
    using Core.IPlatform;
    using MvvmCross.Platform.IoC;
    using MvvmCross.Droid.FullFragging.Fragments;

    public class FragmentTypeLookup : IFragmentTypeLookup
    {
        private readonly IDictionary<string, Type> _fragmentLookup = new Dictionary<string, Type>();

        public FragmentTypeLookup()
        {
            this._fragmentLookup =
                (from type in this.GetType().Assembly.ExceptionSafeGetTypes()
                 where !type.IsAbstract
                    && !type.IsInterface
                    && typeof(MvxFragment).IsAssignableFrom(type)
                    && type.Name.EndsWith("View")
                 select type).ToDictionary(this.getStrippedName);
        }

        public bool TryGetFragmentType(Type viewModelType, out Type fragmentType)
        {
            var strippedName = this.getStrippedName(viewModelType);
            if (strippedName == "Main")
            {
                strippedName = "MainContent";
            }

            if (!this._fragmentLookup.ContainsKey(strippedName))
            {
                fragmentType = null;

                return false;
            }

            fragmentType = this._fragmentLookup[strippedName];

            return true;
        }

        private string getStrippedName(Type type)
        {
            return type.Name
                       .TrimEnd("View".ToCharArray())
                       .TrimEnd("ViewModel".ToCharArray());
        }

    }
}