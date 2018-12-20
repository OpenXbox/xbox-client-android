using System;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Content.PM;
using Android.Support.V4.Widget;
using Android.Graphics;
using Java.Lang;

using SmartGlass;
using SmartGlass.Nano.Droid;

namespace SmartGlass.Nano.Droid
{
    [Activity(Label = "Xbox Nano Client", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private bool IsConnecting = false;
        private List<SmartGlass.Device> _consoles;
        private Model.ConsoleAdapter _consoleListAdapter;

        private SwipeRefreshLayout _refreshLayout;
        private ListView _consoleListView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _refreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swiperefresh);
            _consoleListView = FindViewById<ListView>(Resource.Id.lvConsolesList);

            _refreshLayout.Refresh += RefreshLayout_Refresh;

            _consoles = new List<SmartGlass.Device>();
            _consoleListAdapter = new Model.ConsoleAdapter(this, _consoles);
            _consoleListView.Adapter = _consoleListAdapter;

            _consoleListView.ItemClick += ConsoleListView_ItemClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // set the menu layout on Main Activity  
            MenuInflater.Inflate(Resource.Menu.MainMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_refresh:
                    RefreshLayout_Refresh(this, new EventArgs());
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        async Task RefreshConsoles()
        {
            var discovered = await Device.DiscoverAsync();
            foreach (Device device in discovered)
            {
                var existingItem = _consoles.FirstOrDefault(x => x.LiveId == device.LiveId);
                if (existingItem != null)
                {
                    int i = _consoles.IndexOf(existingItem);
                    _consoles[i] = device;
                }
                else
                {
                    _consoles.Add(device);
                }
            }
        }

        private async void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            await RefreshConsoles();
            this.RunOnUiThread(() =>
            {
                _consoleListAdapter.NotifyDataSetChanged();
            });
            _refreshLayout.Refreshing = false;
        }

        async void ConsoleListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (IsConnecting)
                return;

            IsConnecting = true;
            SmartGlass.Device device = _consoleListAdapter[e.Position];

            try
            {
                await Model.ConsoleConnection.Initialize(device.Address.ToString());

                var intent = new Android.Content.Intent(this, typeof(StreamActivity));
                StartActivity(intent);
            }
            catch (System.Exception ex)
            {
                var toast = Toast.MakeText(this,
                    System.String.Format("Connecting to {0} failed! error: {1}", device.Address, ex.Message),
                    ToastLength.Short);

                toast.Show();
            }

            IsConnecting = false;
        }
    }
}

