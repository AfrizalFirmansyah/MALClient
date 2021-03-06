using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Shehabic.Droppy;
using FFImageLoading;
using GalaSoft.MvvmLight.Helpers;
using MALClient.Android.Activities;
using MALClient.Android.BindingConverters;
using MALClient.Android.DIalogs;
using MALClient.Android.Flyouts;
using MALClient.Android.PagerAdapters;
using MALClient.Android.Resources;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Details;

namespace MALClient.Android.Fragments
{
    public partial class AnimeDetailsPageFragment : MalFragmentBase
    {
        private static AnimeDetailsPageNavigationArgs _navArgs;
        private AnimeDetailsPageViewModel ViewModel;
        private DroppyMenuPopup _menu;

        protected override void Init(Bundle savedInstanceState)
        {
            ViewModel = ViewModelLocator.AnimeDetails;
            ViewModel.RegisterOneTimeOnPropertyChangedAction(nameof(ViewModel.AnimeMode), SetupForAnimeMode);
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            ViewModel.Init(_navArgs);
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(ViewModel.DetailImage))
            {
                if (AnimeDetailsPageShowCoverImage != null)
                    ImageService.Instance.LoadUrl(ViewModel.DetailImage, TimeSpan.FromDays(7))
                        .FadeAnimation(false).Success(() => AnimeDetailsPageShowCoverImage.AnimateFadeIn())
                        .Into(AnimeDetailsPageShowCoverImage);
            }
        }

        protected override void InitBindings()
        {
            AnimeDetailsPagePivot.Adapter = new AnimeDetailsPagerAdapter(FragmentManager);
            AnimeDetailsPageTabStrip.SetViewPager(AnimeDetailsPagePivot);
           
            Bindings = new List<Binding>();

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.MyScoreBind,
                    () => AnimeDetailsPageScoreButton.Text));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.MyStatusBind,
                    () => AnimeDetailsPageStatusButton.Text));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.MyEpisodesBind,
                    () => AnimeDetailsPageWatchedButton.Text));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.MyVolumesBind,
                    () => AnimeDetailsPageReadVolumesButton.Text));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.LoadingGlobal,
                    () => AnimeDetailsPageLoadingOverlay.Visibility).ConvertSourceToTarget(Converters.BoolToVisibility));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.AddAnimeVisibility,
                    () => AnimeDetailsPageAddSection.Visibility).ConvertSourceToTarget(Converters.BoolToVisibility));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.IsIncrementButtonEnabled,
                    () => AnimeDetailsPageIncrementButton.Enabled));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.IsDecrementButtonEnabled,
                    () => AnimeDetailsPageDecrementButton.Enabled));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.AddAnimeVisibility,
                    () => AnimeDetailsPageUpdateSection.Visibility).ConvertSourceToTarget(Converters.BoolToVisibilityInverted));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.AddAnimeVisibility,
                    () => AnimeDetailsPageIncDecSection.Visibility).ConvertSourceToTarget(Converters.BoolToVisibilityInverted));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.DetailsPivotSelectedIndex).WhenSourceChanges(() => AnimeDetailsPagePivot.SetCurrentItem(ViewModel.DetailsPivotSelectedIndex,true)));

            
            Bindings.Add(
                this.SetBinding(() => ViewModel.IsFavourite).WhenSourceChanges(() =>
                { 
                    AnimeDetailsPageFavouriteButton.SetImageResource(ViewModel.IsFavourite
                        ? Resource.Drawable.icon_favourite
                        : Resource.Drawable.icon_fav_outline);
                }));

            AnimeDetailsPageFavouriteButton.SetCommand("Click",ViewModel.ToggleFavouriteCommand);
            AnimeDetailsPageIncrementButton.SetCommand("Click",ViewModel.IncrementEpsCommand);
            AnimeDetailsPageDecrementButton.SetCommand("Click",ViewModel.DecrementEpsCommand);
            AnimeDetailsPageMoreButton.Click +=
                (sender, args) =>
                {
                   _menu = AnimeDetailsPageMoreFlyoutBuilder.BuildForAnimeDetailsPage(Activity, AnimeDetailsPageMoreButton,
                        ViewModel);
                   _menu.Show();
                };
            AnimeDetailsPageAddButton.SetCommand("Click",ViewModel.AddAnimeCommand);

            
            //OneTime

            AnimeDetailsPageWatchedLabel.Text = ViewModel.WatchedEpsLabel;
            


            //

            //Events
            AnimeDetailsPageStatusButton.Click += AnimeDetailsPageStatusButtonOnClick;
            AnimeDetailsPageScoreButton.Click += AnimeDetailsPageScoreButtonOnClick;
            AnimeDetailsPageWatchedButton.Click += AnimeDetailsPageWatchedButtonOnClick;
            AnimeDetailsPageReadVolumesButton.Click += AnimeDetailsPageVolumesButtonOnClick;


        }

        private void AnimeDetailsPageWatchedButtonOnClick(object sender, EventArgs eventArgs)
        {
            AnimeUpdateDialogBuilder.BuildWatchedDialog(ViewModel.AnimeItemReference as AnimeItemViewModel,
                (model, s) =>
                {
                    ViewModel.WatchedEpsInput = s;
                    ViewModel.ChangeWatchedCommand.Execute(null);
                });
        }

        private void AnimeDetailsPageScoreButtonOnClick(object sender, EventArgs eventArgs)
        {
            AnimeUpdateDialogBuilder.BuildScoreDialog(ViewModel.AnimeItemReference, i =>
            {
                ViewModel.ChangeScoreCommand.Execute(i);
            });
        }

        private void AnimeDetailsPageStatusButtonOnClick(object sender, EventArgs eventArgs)
        {
            AnimeUpdateDialogBuilder.BuildStatusDialog(ViewModel.AnimeItemReference,ViewModel.AnimeMode, status =>
            {
                ViewModel.ChangeStatus(status);
            });
        }

        private void AnimeDetailsPageVolumesButtonOnClick(object sender, EventArgs e)
        {
            AnimeUpdateDialogBuilder.BuildWatchedDialog(ViewModel.AnimeItemReference as AnimeItemViewModel,
                (model, s) =>
                {
                    ViewModel.ReadVolumesInput = s;
                    ViewModel.ChangeVolumesCommand.Execute(null);
                },true);
        }

        private void SetupForAnimeMode()
        {
            if (ViewModel.AnimeMode)
            {
                AnimeDetailsPageReadVolumesButton.Visibility =
                    AnimeDetailsPageReadVolumesLabel.Visibility = ViewStates.Gone;
            }
            else
            {
                AnimeDetailsPageReadVolumesButton.Visibility =
                    AnimeDetailsPageReadVolumesLabel.Visibility = ViewStates.Visible;
            }
        }

        protected override void Cleanup()
        {
            AnimeDetailsPageStatusButton.Click -= AnimeDetailsPageStatusButtonOnClick;
            AnimeDetailsPageScoreButton.Click -= AnimeDetailsPageScoreButtonOnClick;
            AnimeDetailsPageWatchedButton.Click -= AnimeDetailsPageWatchedButtonOnClick;
            AnimeDetailsPageReadVolumesButton.Click -= AnimeDetailsPageVolumesButtonOnClick;
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        public override int LayoutResourceId => Resource.Layout.AnimeDetailsPage;

        public static AnimeDetailsPageFragment BuildInstance(object args)
        {
            _navArgs = args as AnimeDetailsPageNavigationArgs;
            return new AnimeDetailsPageFragment();
        }
    }
}