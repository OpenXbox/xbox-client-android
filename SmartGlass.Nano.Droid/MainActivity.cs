using Android.App;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Views;
using System.Collections.Generic;
using Android.Content.PM;

using SmartGlass;
using System.Threading.Tasks;

namespace SmartGlass.Nano.Droid
{
    [Activity(Label = "Xbox Nano Client", MainLauncher = true, Icon = "@mipmap/icon",
              ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        private List<string> _discovered;
        private ArrayAdapter<string> _consoleListAdapter;
        private ListView _consoleListView;
        private Button _refreshButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _consoleListView = FindViewById<ListView>(Resource.Id.lvConsoles);
            _refreshButton = FindViewById<Button>(Resource.Id.btnRefresh);

            _discovered = new List<string>();

            _consoleListAdapter = new ArrayAdapter<string>(
                this, Android.Resource.Layout.SimpleListItem1, _discovered);
            _consoleListView.Adapter = _consoleListAdapter;

            _consoleListView.ItemClick += ConsoleListView_ItemClick;
            _refreshButton.Click += RefreshButton_Click;
        }

        async Task Discover()
        {
            var discovered = await Device.DiscoverAsync();
            _discovered.Clear();
            foreach (Device dev in discovered)
            {
                _consoleListAdapter.Add(dev.Address.ToString());
            }
        }

        async void RefreshButton_Click(object sender, System.EventArgs e)
        {
            await Discover();
        }

        void ConsoleListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string ipAddr = _consoleListAdapter.GetItem(e.Position);

            var intent = new Android.Content.Intent(this, typeof(StreamActivity));
            intent.PutExtra("hostName", ipAddr);
            StartActivity(intent);
        }
    }
}

