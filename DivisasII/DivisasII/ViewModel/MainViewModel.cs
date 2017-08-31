using DivisasII.Classes;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Windows.Input;

namespace DivisasII.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Attributes
        private decimal amount;
        private double sourceRate;
        private double targetRate;
        private bool isEnabled;
        private bool isRunning;
        private string message;
        private ExchangeRates exchangeRates;


        #endregion

        #region Properties
        public ObservableCollection<Rate> Rates { get; set; }

        public decimal Amount
        {

            set
            {
                if (amount != value)
                {
                    amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
                }
            }
            get
            {
                return amount;
            }
        }

        public double SourceRate
        {

            set
            {
                if (sourceRate != value)
                {
                    sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceRate"));
                }

            }
            get
            {
                return sourceRate;
            }
        }

        public double TargetRate
        {

            set
            {
                if (targetRate != value)
                {
                    targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetRate"));
                }
            }
            get
            {
                return targetRate;
            }
        }


        public bool IsEnabled
        {

            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get
            {
                return isEnabled;
            }
        }


        public bool IsRunning
        {

            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get
            {
                return isRunning;
            }
        }

        public string Message
        {
            set
            {
                if (message != value)
                {
                    message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
                }
            }
            get
            {
                return message;
            }

        }


        #endregion
        
        #region Constructors

        public MainViewModel()
        {
            Rates = new ObservableCollection<Rate>();
            Message = "Enter an a amount, select a source currency, select a target currency and press Convert button";
            LoadRates();
        }

        #endregion
        
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private async void LoadRates()
        {
            IsRunning = true;
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://openexchangerates.org/");
                var url = "/api/latest.json?app_id=f9997abe52f6483faf52f05bcc5cc10e";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Message = response.StatusCode.ToString();
                    return;
                }
                var result = await response.Content.ReadAsStringAsync();
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                isRunning = false;
                return;
            }
            ConvertRates();
            IsRunning = false;
            IsEnabled = true;


        }

        private void ConvertRates()
        {
            Rates.Clear();
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);
                Rates.Add(new Rate
                {
                    Code = code,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),
                });

            }
        }
        #endregion

        #region Commands

        public ICommand ConvertMoneyCommand { get { return new RelayCommand(ConvertMoney); } }

        private async void ConvertMoney()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a positive value in amount","Accept");
                return;
            }

            if (SourceRate == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a source rate", "Accept");
                return;
            }

            if (TargetRate == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a target rate", "Accept");
                return;
            }


            var converted = Amount /(decimal)SourceRate * (decimal)TargetRate;
            Message = string.Format("{0:C2} = {1:C2}", Amount, converted);
        }

       

        #endregion

    }
}
