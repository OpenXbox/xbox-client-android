using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net;
using System.Threading;
using Android.OS;
using Android.Graphics;
using Android.Util;

namespace SmartGlass.Nano.Droid.Model
{
    public class ConsoleAdapter : BaseAdapter<SmartGlass.Device>
    {
        List<SmartGlass.Device> items;
        Activity context;
        public ConsoleAdapter(Activity context, List<SmartGlass.Device> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override SmartGlass.Device this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.ConsoleView, null);
            view.FindViewById<TextView>(Resource.Id.FirstLine).Text =
                String.Format("{0} ({1})", item.Name, item.Address.ToString());
            view.FindViewById<TextView>(Resource.Id.SecondLine).Text = item.LiveId;
            // view.FindViewById<ImageView>(Resource.Id.Image).SetImageResource(item.ImageResourceId);
            return view;
        }
    }
}