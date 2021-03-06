using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using GalaSoft.MvvmLight.Helpers;
using MALClient.Android.Activities;
using MALClient.Android.Web;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Forums.Items;

namespace MALClient.Android.BindingInformation
{
    public class ForumPostBidningInfo : BindingInfo<ForumTopicMessageEntryViewModel>
    {
        public ForumPostBidningInfo(View container, ForumTopicMessageEntryViewModel viewModel, bool fling) : base(container, viewModel, fling)
        {
            PrepareContainer();
        }

        protected override void InitBindings()
        {
           if(Fling)
                return;

            Bindings.Add(this.SetBinding(() => ViewModel.Data.HtmlContent).WhenSourceChanges(() =>
            {
                Container.FindViewById<WebView>(Resource.Id.ForumTopicPageItemWebView).LoadDataWithBaseURL(null, ResourceLocator.CssManager.WrapWithCss(ViewModel.Data.HtmlContent), "text/html; charset=utf-8", "UTF-8", null);
            }));

            Bindings.Add(this.SetBinding(() => ViewModel.ComputedHtmlHeight).WhenSourceChanges(() =>
            {
                var webView = Container.FindViewById(Resource.Id.ForumTopicPageItemWebView);
                if (webView.Height < ViewModel.ComputedHtmlHeight)
                {
                    UpdateViewWithNewHeight(webView, (int)ViewModel.ComputedHtmlHeight);
                }
                else
                {
                    UpdateViewWithNewHeight(webView, DimensionsHelper.DpToPx(200));
                }
            }));
        }

        private DataJavascriptInterface _dataJavascriptInterface;

        protected override void InitOneTimeBindings()
        {
            Container.FindViewById<TextView>(Resource.Id.ForumTopicPageItemPostAuthor).Text =
                ViewModel.Data.Poster.MalUser.Name;
            Container.FindViewById<TextView>(Resource.Id.ForumTopicPageItemPostNumber).Text =
                ViewModel.Data.MessageNumber;


            if (!Fling)
            {
                var webView = Container.FindViewById<WebView>(Resource.Id.ForumTopicPageItemWebView);
                if (webView.Tag == null)
                {
                    var jsInterface = new DataJavascriptInterface(Container.Context);
                    webView.Settings.JavaScriptEnabled = true;
                    webView.AddJavascriptInterface(jsInterface, "android");
                    webView.Tag = jsInterface;
                    _dataJavascriptInterface = jsInterface;
                }
                else
                    _dataJavascriptInterface = webView.Tag as DataJavascriptInterface;

                _dataJavascriptInterface.NewResponse += DataJavascriptInterfaceOnNewResponse;
            }

        }

        private void UpdateViewWithNewHeight(View view,int height)
        {
            var param = view.LayoutParameters;
            param.Height = height;
            view.LayoutParameters = param;
            //Container.Invalidate();
        }

        private void DataJavascriptInterfaceOnNewResponse(object sender, string s)
        {
            MainActivity.CurrentContext.RunOnUiThread(() =>
            {
                ViewModel.ComputedHtmlHeight = DimensionsHelper.DpToPx(int.Parse(s))*1.1;
            });

        }

        protected override void DetachInnerBindings()
        {
            if(_dataJavascriptInterface != null)
            _dataJavascriptInterface.NewResponse -= DataJavascriptInterfaceOnNewResponse;
            _dataJavascriptInterface = null;
        }
    }
}