﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Cavokator
{
    class NotamContainer
    {
        public bool connectionError;

        public List<string> NotamRaw { get; set; } = new List<string>();
    }
}