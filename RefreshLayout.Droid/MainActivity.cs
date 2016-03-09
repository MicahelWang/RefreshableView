using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Java.Util;

namespace RefreshLayout.Droid
{
    [Activity(Label = "RefreshLayout.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private RefreshLayout.Droid.RefreshableView _refreshableView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            init();

        }


        private void init()
        {
            // TODO Auto-generated method stub
            initView();
        }
        private void initView()
        {
            // TODO Auto-generated method stub
            _refreshableView = FindViewById<RefreshableView>(Resource.Id.refresh_root);
            initData();
        }
        private void initData()
        {
            _refreshableView.OnRefresh += _refreshableView_OnRefresh;
        }

        private void _refreshableView_OnRefresh(RefreshableView view)
        {
            Thread.Sleep(2000);
            Toast.MakeText(this, "刷新结束", ToastLength.Short).Show();
            _refreshableView.FinishRefresh();
        }
    }
}

