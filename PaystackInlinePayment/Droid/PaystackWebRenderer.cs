using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Plugin.PaystackInlinePayment;
using Plugin.PaystackInlinePayment.Renderers;
using Android.Webkit;
using Java.Interop;
using Android.Graphics;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(PaystackWebRenderer))]
namespace Plugin.PaystackInlinePayment.Droid
{
    public class PaystackWebRenderer : ViewRenderer<HybridWebView, Android.Webkit.WebView>
    {
        const string CallBackJavaScriptFunction = "function invokeCSharpAction(data){jsBridge.invokeCallbackAction(data);}";
        const string CloseJavaScriptFunction = "function invokeCSharpCloseAction(){jsBridge.invokeCloseAction();}";

        Context _context;
        public HybridWebViewRenderer(Context context) : base(context)
        {
            _context = context;
        }
        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                var webview = new Android.Webkit.WebView(_context);
                webview.Settings.JavaScriptEnabled = true;
                this.SetNativeControl(webview);
            }
            if (e.OldElement != null)
            {
                //unsubscribe from events
                Control.RemoveJavascriptInterface("jsBridge");
                var hybridWebView = e.OldElement as HybridWebView;
                hybridWebView.CleanUp();
            }
            if (e.NewElement != null)
            {
                //subscribe to Events
                var webviewElement = (HybridWebView)Element;
                Control.AddJavascriptInterface(new JSBridge(this), "jsBridge");
                Control.SetWebViewClient(new CustomWebViewClient(webviewElement.Data));
                Control.LoadUrl(string.Format("file:///android_asset/Content/{0}", Element.Uri));
                InjectJS(CallBackJavaScriptFunction);
                InjectJS(CloseJavaScriptFunction);
            }
        }

        void InjectJS(string script)
        {
            if (Control != null)
            {
                Control.LoadUrl(string.Format("javascript: {0}", script));
            }
        }
    }

    class CustomWebViewClient : WebViewClient
    {
        string Record = "";
        public CustomWebViewClient(string record)
        {
            Record = record;
        }
        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            base.OnPageFinished(view, url);

            view.LoadUrl(string.Format("javascript:payWithPaystack({0})", Record));
        }
        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            base.OnPageStarted(view, url, favicon);
        }
    }
   
}
