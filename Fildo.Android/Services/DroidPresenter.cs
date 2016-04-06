namespace Fildo.Droid.Services
{
    using Android.App;
    using Core.IPlatform;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.FullFragging.Fragments;
    using MvvmCross.Droid.Views;
    using System;

    public class DroidPresenter : MvxAndroidViewPresenter
    {
        private readonly IMvxViewModelLoader viewModelLoader;
        private readonly IFragmentTypeLookup fragmentTypeLookup;
        private FragmentManager fragmentManager;

        public DroidPresenter(IMvxViewModelLoader viewModelLoader, IFragmentTypeLookup fragmentTypeLookup)
        {
            this.fragmentTypeLookup = fragmentTypeLookup;
            this.viewModelLoader = viewModelLoader;
        }

        public void RegisterFragmentManager(FragmentManager fragmentManager, MvxFragment initialFragment)
        {
            this.fragmentManager = fragmentManager;

            this.showFragment(initialFragment, false);
        }

        public override void Show(MvxViewModelRequest request)
        {
            Type fragmentType;
            if (this.fragmentManager == null || !this.fragmentTypeLookup.TryGetFragmentType(request.ViewModelType, out fragmentType))
            {
                base.Show(request);

                return;
            }

            var fragment = (MvxFragment)Activator.CreateInstance(fragmentType);
            fragment.ViewModel = this.viewModelLoader.LoadViewModel(request, null);

            this.showFragment(fragment, true);
        }
        
        private void showFragment(MvxFragment fragment, bool addToBackStack)
        {
            var transaction = this.fragmentManager.BeginTransaction();
            
            if (addToBackStack && !fragment.ToString().StartsWith("LoginView"))
                transaction.AddToBackStack(fragment.GetType().Name);
            try
            {
                transaction
                .Replace(Resource.Id.contentFrame, fragment)
                .Commit();
            }
            catch (Exception)
            {
                
            }
            
        }

        public override void Close(IMvxViewModel viewModel)
        {
            var currentFragment = this.fragmentManager.FindFragmentById(Resource.Id.contentFrame) as MvxFragment;
            if (currentFragment != null && currentFragment.ViewModel == viewModel)
            {
                this.fragmentManager.PopBackStackImmediate();

                return;
            }

            base.Close(viewModel);
        }
    }
}