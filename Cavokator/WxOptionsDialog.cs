﻿using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;


namespace Cavokator
{
    class WxOptionsDialog : DialogFragment
    {

        public event EventHandler<WXOptionsDialogEventArgs> SpinnerChanged;
        public event EventHandler<WXOptionsDialogEventArgs> SeekbarChanged;
        public event EventHandler<WXOptionsDialogEventArgs> SwitchChanged;


        // Configuration header
        private TextView _configurationText;

        // Type of weather group
        private Spinner _metarOrTaforSpinner;
        private TextView _metarOrTaforText;

        // Metar delay group
        private TextView _metarHoursText;
        private SeekBar _metarHoursSeekBar;
        private TextView _metarHoursSeekBarText;
        
        // Save data group
        private TextView _saveDataText;
        private Switch _saveDataSwitch;

        
        // Dismiss button
        private Button _dismissBialogButton;
        
        // Configuration
        private int _maxSpinnerHours = 12;

        private int _spinnerSelection;
        private int _hoursBefore;
        private bool _mostRecent;
        private bool _saveData;

        // Convert strings to order in spinner
        public WxOptionsDialog(string metar_or_tafor, int hoursBefore, bool mostRecent, bool saveData)
        {
            switch (metar_or_tafor)
            {
                case "metar_and_tafor":
                    _spinnerSelection = 0;
                    break;
                case "only_metar":
                    _spinnerSelection = 1;
                    break;
                case "only_tafor":
                    _spinnerSelection = 2;
                    break;
            }
            
            this._hoursBefore = hoursBefore;

            this._mostRecent = mostRecent;

            this._saveData = saveData;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Inflate view
            var view = inflater.Inflate(Resource.Layout.wx_options_dialog, container, false);

            // Find view IDs
            _metarOrTaforSpinner = view.FindViewById<Spinner>(Resource.Id.wx_options_metarORtafor_spinner);
            _configurationText = view.FindViewById<TextView>(Resource.Id.wx_options_configuration_text);
            _metarOrTaforText = view.FindViewById<TextView>(Resource.Id.wx_options_metarORtafor_text);
            _metarHoursText = view.FindViewById<TextView>(Resource.Id.wx_options_metarHours);
            _metarHoursSeekBar = view.FindViewById<SeekBar>(Resource.Id.wx_options_metarHours_seekbar);
            _metarHoursSeekBarText = view.FindViewById<TextView>(Resource.Id.wx_option_metarHours_seekbarText);
            _dismissBialogButton = view.FindViewById<Button>(Resource.Id.wx_option_closeButton);
            _saveDataText = view.FindViewById<TextView>(Resource.Id.wx_options_saveDataText);
            _saveDataSwitch = view.FindViewById<Switch>(Resource.Id.wx_options_saveDataSwitch);


            // Assign text fields
            _configurationText.Text = Resources.GetString(Resource.String.Option_ConfigurationText);
            _metarOrTaforText.Text = Resources.GetString(Resource.String.Option_ChooseMetarOrTaforText);
            _metarHoursText.Text = Resources.GetString(Resource.String.Option_MetarHoursText);
            _saveDataText.Text = Resources.GetString(Resource.String.Option_SaveDataText);



            // SPINNER ADAPTER CONFIG
            var adapter = ArrayAdapter.CreateFromResource
                (this.Activity, Resource.Array.wx_option, Resource.Layout.wx_options_spinner);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _metarOrTaforSpinner.Adapter = adapter;

            _metarOrTaforSpinner.SetSelection(_spinnerSelection);

            _metarOrTaforSpinner.ItemSelected += delegate
            {
                // Call event raiser
                OnSpinnerChanged(_metarOrTaforSpinner.SelectedItemPosition);

                // Save ISharedPreference
                SetWeatherOrTaforPreferences(_metarOrTaforSpinner.SelectedItemPosition);
            };



            // SEEKBAR CONFIG

            // Set max hours
            _metarHoursSeekBar.Max = _maxSpinnerHours;

            // Set actual value to the one passed by main activity
            if (_mostRecent)
            {
                _metarHoursSeekBar.Progress = 0;
            }
            else
            {
                _metarHoursSeekBar.Progress = _hoursBefore;
            }
            

            // Set initial value for seekbar text
            if (_metarHoursSeekBar.Progress == 0)
            {
                _metarHoursSeekBarText.Text = GetString(Resource.String.Option_JustGetLast);
            }
            else if (_metarHoursSeekBar.Progress == 1)
            {
                _metarHoursSeekBarText.Text = _metarHoursSeekBar.Progress.ToString() 
                    + " " + GetString(Resource.String.Option_Hour);
            }
            else
            {
                _metarHoursSeekBarText.Text = _metarHoursSeekBar.Progress.ToString()
                    + " " + GetString(Resource.String.Option_Hours);
            }

            _metarHoursSeekBar.ProgressChanged += delegate
            {
                // We want to write "Last" instead of 0
                if (_metarHoursSeekBar.Progress == 0)
                {
                    _metarHoursSeekBarText.Text = GetString(Resource.String.Option_JustGetLast);
                }
                else if (_metarHoursSeekBar.Progress == 1)
                {
                    _metarHoursSeekBarText.Text = _metarHoursSeekBar.Progress.ToString()
                        + " " + GetString(Resource.String.Option_Hour);
                }
                else
                {
                    _metarHoursSeekBarText.Text = _metarHoursSeekBar.Progress.ToString()
                        + " " + GetString(Resource.String.Option_Hours);
                }


                // Call event raiser
                OnSeekbarChanged(_metarHoursSeekBar.Progress);

                // Save ISharedPreferences
                SetHoursBeforePreferences(_metarHoursSeekBar.Progress);

            };



            // SWITCH CONFIG

            if (_saveData)
            {
                _saveDataSwitch.Checked = true;
            }
            else
            {
                _saveDataSwitch.Checked = false;
            }

            _saveDataSwitch.CheckedChange += delegate
            {
                
                // Call event raiser with parameters
                if (_saveDataSwitch.Checked)
                {
                    OnSwitchChanged(true);

                    SetSaveDataPreferences(true);
                }
                else
                {
                    OnSwitchChanged(false);

                    SetSaveDataPreferences(false);
                }
                

            };


            // CLOSE BUTTON (dismiss dialog)
            _dismissBialogButton.Click += delegate
            {
                this.Dismiss();
            };


            return view;
        }



        private void SetSaveDataPreferences(bool saveData)
        {
            ISharedPreferences wxprefs = Application.Context.GetSharedPreferences("WX_Preferences", FileCreationMode.Private);

            if (saveData)
            {
                wxprefs.Edit().PutString("saveDataPREF", "true").Apply();
            }
            else
            {
                wxprefs.Edit().PutString("saveDataPREF", "false").Apply();
            }
            
        }


        private void SetHoursBeforePreferences(int progress)
        {
            ISharedPreferences wxprefs = Application.Context.GetSharedPreferences("WX_Preferences", FileCreationMode.Private);
            wxprefs.Edit().PutString("hoursBeforePREF", progress.ToString()).Apply();
        }


        private void SetWeatherOrTaforPreferences(int position)
        {

            string preference = String.Empty;

            switch (position)
            {
                case 0:
                    preference = "metar_and_tafor";
                    break;
                case 1:
                    preference = "only_metar";
                    break;
                case 2:
                    preference = "only_tafor";
                    break;
            }

            ISharedPreferences wxprefs = Application.Context.GetSharedPreferences("WX_Preferences", FileCreationMode.Private);
            wxprefs.Edit().PutString("metarOrTaforPREF", preference).Apply();
        }


        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            // Sets the title bar to invisible
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            base.OnActivityCreated(savedInstanceState);

            // Sets the animation
            Dialog.Window.Attributes.WindowAnimations = Resource.Style.dialog_animation;

        }


        // Event raiser
        protected virtual void OnSpinnerChanged(int position)
        {
            SpinnerChanged?.Invoke(this, new WXOptionsDialogEventArgs(position));
        }


        // Event raiser
        protected virtual void OnSeekbarChanged(int position)
        {
            SeekbarChanged?.Invoke(this, new WXOptionsDialogEventArgs() { HoursBefore = position } );
        }


        // Event raiser
        protected virtual void OnSwitchChanged(bool toggled)
        {
            SwitchChanged?.Invoke(this, new WXOptionsDialogEventArgs() { SaveData = toggled });
        }

    }


    public class WXOptionsDialogEventArgs : EventArgs
    {

        public string MetarOrTafor { get; private set; }
        public int HoursBefore { get; set; }
        public bool SaveData { get; set; }

        public WXOptionsDialogEventArgs() { }

        public WXOptionsDialogEventArgs(int position)
        {
            switch (position)
            {
                case 0:
                    MetarOrTafor = "metar_and_tafor";
                    break;
                case 1:
                    MetarOrTafor = "only_metar";
                    break;
                case 2:
                    MetarOrTafor = "only_tafor";
                    break;
            }
        }

    }


}