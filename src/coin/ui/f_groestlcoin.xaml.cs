/*######   Copyright (c) 2011-2019 Ufasoft  http://ufasoft.com  mailto:support@ufasoft.com,  Sergey Pavlov  mailto:dev@ufasoft.com ####
#                                                                                                                                     #
#       See LICENSE for licensing information                                                                                         #
#####################################################################################################################################*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Microsoft.Win32;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;

using GuiComp;

using Interop.coineng;

using Hardcodet.Wpf.TaskbarNotification;

using Coin.Properties;

namespace Coin {
    public partial class FormMain : Window {
        public static FormMain I;
        CultureInfo CultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();

        public FormMain() {
            InitializeComponent();

            I = this;
            CultureInfo.NumberFormat = (NumberFormatInfo)CultureInfo.NumberFormat.Clone();
            CultureInfo.NumberFormat.CurrencySymbol = "";
            Thread.CurrentThread.CurrentCulture = CultureInfo;
        }

        public ICoinEng Eng = CoinEng.CoinCreateObj();

        Dictionary<ITransaction, object> AllTxes = new Dictionary<ITransaction, object>();
        Dictionary<IWallet, WalletForms> m_wallet2forms = new Dictionary<IWallet, WalletForms>();
        WalletForms TheWallet;
        Dictionary<string, int> AllAlerts = new Dictionary<string, int>();

        public static RegistryKey UserAppRegistryKey {
            get {
                return Registry.CurrentUser.CreateSubKey(@"Software\Groestlcoin-WPF");
            }
        }

        public class WalletEvent {
            public DateTime Timestamp { get; set; }
            public string Comment { get; set; }
        }

        ObservableCollection<WalletForms> ActiveWalletForms = new ObservableCollection<WalletForms>();
        ObservableCollection<WalletEvent> WalletEvents = new ObservableCollection<WalletEvent>();


        void UpdateData(WalletForms wf) {
            var w = wf.Wallet;

            int i = 0;
            foreach (var alert in w.Alerts) {
                string comment = alert.Comment;
                if (!AllAlerts.ContainsKey(comment)) {
                    AllAlerts.Add(comment, 1);
                    WalletEvents.Add(new WalletEvent() { Timestamp = alert.UntilTimestamp, Comment = comment });
                }
            }

            foreach (var tx in w.Transactions) {
                if (!AllTxes.ContainsKey(tx)) {
                    AllTxes.Add(tx, null);
                    var addr = tx.Address;
                    try {
                        decimal amount = tx.Amount;
                        string s;
                        if (amount > 0)
                            s = $"{amount} {w.CurrencySymbol} received to our ";
                        else
                            s = $"{-amount} {w.CurrencySymbol} sent to";
                        s += $" address {addr.Value} {addr.Comment} {tx.Comment}";
                        WalletEvents.Add(new WalletEvent() { Timestamp = tx.Timestamp, Comment = s });
                        if (++i > 10)
                            break;
                    }
                    finally {
                        Marshal.ReleaseComObject(addr);
                    }
                }
            }
            CtlTxes.UpdateTransactions();
        }


        void UpdateView() {
            WalletEvents.Clear();
            AllAlerts.Clear();
            foreach (var wf in ActiveWalletForms)
                UpdateData(wf);
            SetMenuDBMode();
        }

        void AddWalletToList(WalletForms wf) {
            wf.Wallet.Enabled = true;
            if (App.CancelPendingTxes)
                wf.Wallet.CancelPendingTxes();
//          LvWallet.ItemsSource = ActiveWalletForms;
        }


        DispatcherTimer timer1 = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        void RegisterUriHandler() {
            try {
                if ((int)UserAppRegistryKey.GetValue("UrlRegAsked", 0) == 0) {
                    UserAppRegistryKey.SetValue("UrlRegAsked", 1);
                    RegistryKey keyClasses = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", true);
                    string toMe = $"\"{Assembly.GetCallingAssembly().Location}\" %1";
                    var subKey = keyClasses.OpenSubKey("groestlcoin");
                    if (subKey == null) {
                        keyClasses.CreateSubKey("groestlcoin").CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue(null, toMe);
                    }  else {
                        var keyCommand = subKey.OpenSubKey("shell").OpenSubKey("open").OpenSubKey("command");
                        string path = (string)keyCommand.GetValue(null, "");
                        if (path != toMe) {
                            if (MessageBox.Show("Register Groestlcoin URI for this Application", "Coin", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                keyCommand.SetValue(null, toMe);
                        }
                    }
                }
            } catch (Exception) {
            }
        }

        static string ToProxyString(string proxyType, string proxyEndPoint) {
            switch (proxyType) {
                case "None": return "";
                case "TOR": return "TOR";
                case "SOCKS5": return "socks5://" + proxyEndPoint;
                case "CONNECT": return "connect://" + proxyEndPoint;
            }
            return "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            MenuDBMode.IsEnabled = false;       //!!! Until testing of Lite mode
            MenuModeFull.Tag = EEngMode.Bootstrap;
            MenuModeLite.Tag = EEngMode.Lite;

            LvWallet.ItemsSource = ActiveWalletForms;
            LvEvent.ItemsSource = WalletEvents;

            var port = UserAppRegistryKey.GetValue("LocalPort");
            if (port != null) {
                try {
                    Eng.LocalPort = Convert.ToUInt16(port);
                } catch (Exception) {
                }
            }

            var proxyType = UserAppRegistryKey.GetValue("ProxyType");
            if (proxyType != null) {
                try {
                    Eng.ProxyString = ToProxyString(Convert.ToString(proxyType), Convert.ToString(UserAppRegistryKey.GetValue("ProxyEndPoint")));
                } catch (Exception) {
                }
            }

            if (App.Testnet)
                Eng.Testnet = App.Testnet;

            Wallet[] wallets = null;
            try {
                wallets = Eng.Wallets;
            } catch (Exception x) {
                MessageBox.Show(x.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            foreach (var wallet in wallets) {
                string currencyName = wallet.CurrencyName;

                var wf = new WalletForms();
                wf.Wallet = wallet;
                /*
                MenuItem mi = new MenuItem();
                wf.MenuItem = mi;
                mi.Header = $"{wallet.CurrencySymbol}  {currencyName}";
                mi.Icon = new Image() { Source = new BitmapImage(new Uri($"images/{Regex.Replace(currencyName, @"-testnet\d?", "")}.ico", UriKind.Relative)) };
                menuCurrency.Items.Add(mi);
                mi.Template = menuTemplate;
                mi.IsCheckable = true;
                mi.Tag = wf;
                mi.Checked += Currency_CheckChanged;
                mi.Unchecked += Currency_CheckChanged;
                */
                m_wallet2forms[wallet] = wf;
                TheWallet = wf;

                EEngMode mode = EEngMode.Bootstrap;
                var sk = UserAppRegistryKey.OpenSubKey(wf.Wallet.CurrencySymbol);
                bool bMiningEnabled = false;
                if (sk != null) {
                    switch ((string)sk.GetValue("DBMode")) {
                        case "Bootstrap": mode = EEngMode.Bootstrap; break;
                        case "Lite": mode = EEngMode.Lite; break;
                    }
                    try {
//                      bMiningEnabled = (int)sk.GetValue("Mining", 0) != 0;
                    } catch (Exception) { }
                }
                mode = EEngMode.Bootstrap;  //!!! Lite Mode not supported temporary
                wallet.Mode = mode;
                if (wallet.MiningAllowed)
                    wallet.MiningEnabled = bMiningEnabled;
            }
            AddWalletToList(TheWallet);

            CtlTxes.WalletForms = TheWallet;
            CtlTxes.InitLoaded();

            CtlSendMoney.Wallet = TheWallet.Wallet;


            CtlMyAddresses.Wallet = TheWallet.Wallet;
            CtlMyAddresses.UpdateMyAddresses();

            CtlRecipients.Wallet = TheWallet.Wallet;
            CtlRecipients.UpdateRecipients();

            UpdateData(TheWallet);
            ActiveWalletForms.Add(TheWallet);


            timer1.Tick += (s, e1) => {
                bool b = false;
                foreach (var wf in ActiveWalletForms)
                    b |= wf.CheckForChanges();
                if (b)
                    UpdateView();
            };
            timer1.Start();

            UpdateView();
            CheckForCommands();
            //          RegisterUriHandler();

            MenuTop.DataContext = TheWallet;

            //MenuMining.IsEnabled = TheWallet.MiningAllowed;
            if (LvWallet.SelectedItem == null && LvWallet.Items.Count == 1)
                LvWallet.SelectedIndex = 0;
        }

        public bool EnsurePassphraseUnlock() {
            if (Eng.NeedPassword) {
                var dlg = new FormPassphrase();
                dlg.labelRetype.Visibility = Visibility.Hidden;
                dlg.textRetype.Visibility = Visibility.Hidden;
                dlg.Title = "Wallet Password";
                if (Dialog.ShowDialog(dlg, this)) {
                    Eng.SetPassword(dlg.textPassword.Password);
                } else
                    return false;
            }
            return true;
        }

        void ReleaseComObjects() {
            foreach (var pp in AllTxes)             //!!!?
                Marshal.FinalReleaseComObject(pp.Key);
            AllTxes.Clear();
            CtlSendMoney.Wallet = null;
            while (m_wallet2forms.Count > 0) {
                IWallet w = null;
                foreach (var de in m_wallet2forms) {
                    w = de.Key;
                    de.Value.Free();
                    break;
                }
                m_wallet2forms.Remove(w);
                Marshal.FinalReleaseComObject(w);
            }
            TheWallet = null;
            Marshal.FinalReleaseComObject(Eng);
        }

        void OnFileExit(object sender, RoutedEventArgs argg) {
            Close();
        }

        void OnTrayExit(object sender, RoutedEventArgs argg) {
//!!!?            TrayIcon.Dispose();
            Close();
        }

        void OnToolsOptions(object sender, RoutedEventArgs argg) {
            var d = new FormOptions(this);
            d.textListeningPort.Text = Eng.LocalPort.ToString();

            var proxyEp = UserAppRegistryKey.GetValue("ProxyEndPoint");
            if (proxyEp != null)
                try {
                    d.editProxy.Text = Convert.ToString(proxyEp);
                } catch (Exception) {
                }
            var proxyType = UserAppRegistryKey.GetValue("ProxyType");
            if (proxyType != null)
                try {
                    d.cbProxy.Text = Convert.ToString(proxyType);
                } catch (Exception) {
                }
            if (Dialog.ShowDialog(d, this)) {
                UInt16 port = Convert.ToUInt16(d.textListeningPort.Text);
                Eng.LocalPort = port;
                Eng.ProxyString = ToProxyString(d.cbProxy.Text, d.editProxy.Text);
                UserAppRegistryKey.SetValue("LocalPort", port, RegistryValueKind.DWord);
                UserAppRegistryKey.SetValue("ProxyType", d.cbProxy.Text);
                UserAppRegistryKey.SetValue("ProxyEndPoint", d.editProxy.Text);
            }
        }

        WalletForms SelectedWallet() {
            return TheWallet;
        }

        WalletForms SelectedWalletNotNull() {
            return TheWallet;
        }

        WalletForms FindWallet(string netName) {
            var r = ActiveWalletForms.FirstOrDefault(w => w.Wallet.CurrencyName.ToUpper() == netName.ToUpper());
            if (r == null)
                throw new ApplicationException($"No active Wallet with name {netName}");
            return r;
        }

        public void SendMoney(string netName, string address, decimal amount, string label, string comment) {
            CtlSendMoney.Wallet = FindWallet(netName).Wallet;
            try {
                CtlSendMoney.Wallet.AddRecipient(address, label);
            } catch (Exception) {
            }
            if (comment == "")
                comment = label;
            CtlSendMoney.textAddress.Text = address;
            CtlSendMoney.textAmount.Text = amount.ToString();
            CtlSendMoney.textComment.Text = comment;
            CtlSendMoney.OnSend(this, null);
        }

        public void CheckForCommands() {
            if (App.SendUri != null) {
                Uri uri = App.SendUri;
                string address = uri.AbsolutePath;
                string label = "", message = "";
                decimal amount = 0;
                Regex s_reParam = new Regex(@"\??([^=]+)=(.*)");
                foreach (var pp in uri.Query.Split('&')) {
                    Match m = s_reParam.Match(pp);
                    if (m.Success) {
                        string name = m.Groups[1].Value,
                            val = Uri.UnescapeDataString(m.Groups[2].Value);
                        switch (name) {
                            case "amount":
                                amount = decimal.Parse(val);
                                break;
                            case "label":
                                label = val;
                                break;
                            case "message":
                                message = val;
                                break;
                        }
                    }
                }
                SendMoney(uri.Scheme, address, amount, label, message);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
//            Settings.Default.MainWindowPlacement = this.GetPlacement();
//!!!?            Settings.Default.Save();
        }

        private void Window_Closed(object sender, EventArgs e) {
            Eng.Stop();
            ReleaseComObjects();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
//            this.SetPlacement(Settings.Default.MainWindowPlacement);
        }



        void SetMenuDBMode() {
            var wf = SelectedWallet();
            var mode = wf.Wallet.Mode;
            MenuModeFull.IsChecked = mode == EEngMode.Bootstrap;
            MenuModeLite.IsChecked = mode == EEngMode.Lite;
            MenuModeLite.IsEnabled = wf.Wallet.LiteModeAllowed;
        }

        private void MenuDBMode_Click(object sender, RoutedEventArgs e) {
            MenuItem mi = (MenuItem)sender;
            if (!mi.IsChecked)
                mi.IsChecked = true;
            else {
                WalletForms wf = SelectedWallet();
                wf.Wallet.Mode = (EEngMode)mi.Tag;
                UserAppRegistryKey.CreateSubKey(wf.Wallet.CurrencySymbol).SetValue("DBMode", ((EEngMode)mi.Tag).ToString());
                SetMenuDBMode();
            }
        }

        private void OnHelpAbout(object sender, RoutedEventArgs e) {
            var d = new GuiComp.DialogAbout();
            d.SourceCodeUri = new Uri("https://github.com/Groestlcoin/Groestlcoin-WPF");
            d.Image.Source = new BitmapImage(new Uri($"/{ System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/groestlcoin256.png", UriKind.Relative));
            Dialog.ShowDialog(d, this);
        }

        private void OnToolsRescan(object sender, RoutedEventArgs e) {
            SelectedWalletNotNull().Wallet.Rescan();
        }

        void OnFileImport(object sender, RoutedEventArgs e) {
            if (!EnsurePassphraseUnlock())
                return;
            var wf = SelectedWalletNotNull();
            var d = new OpenFileDialog();
            d.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), wf.Wallet.CurrencyName);
            d.Filter = wf.Wallet.CurrencyName + " Wallet format|wallet.dat|All Files|*.*";
            d.FileName = "wallet.dat";
            if (Dialog.ShowDialog(d, this)) {
                string password = "";
                while (true) {
                    try {
                        switch (d.FilterIndex) {
                            case 1:
                                wf.Wallet.ImportWallet(d.FileName, password);
                                CtlMyAddresses.UpdateMyAddresses();
                                CtlRecipients.UpdateRecipients();
                                wf.Wallet.Rescan();

                                break;
                            case 2:
                            case 3:
                                foreach (var key in File.ReadAllLines(d.FileName))
                                    wf.Wallet.ImportPrivateKey(key.Trim(), password);
                                break;
                        }
                        break;
                    } catch (Exception ex) {
                        if (ex.HResult != (int)Err.InvalidPassword)
                            throw;
                        var dlg = new FormPassphrase();
                        dlg.labelRetype.Visibility = Visibility.Hidden;
                        dlg.textRetype.Visibility = Visibility.Hidden;
                        dlg.Title = "Enter Passphrase for imported wallet.dat";
                        if (!Dialog.ShowDialog(dlg, this))
                            break;
                        password = dlg.textPassword.Password;
                    }
                }
            }
        }

        bool AskUnencryptedExportWarning() {
            return MessageBox.Show("Exported file will contain unencrypted keys. You can lose all your Groestlcoins if this file will be stolen!\nPlease save it to secure place and remove from the Working computer.\nAre you sure to export?",
                    "Coin Security Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;
        }


        void OnFileExport(object sender, RoutedEventArgs e) {
            if (!AskUnencryptedExportWarning())
                return;

            if (!EnsurePassphraseUnlock())
                return;
            var wf = SelectedWalletNotNull();
            var d = new SaveFileDialog();
            d.InitialDirectory = Eng.AppDataDirectory;
            d.Filter = wf.Wallet.CurrencyName + " Wallet format|wallet.dat";
            d.FileName = "wallet-backup";
            if (Dialog.ShowDialog(d, this)) {
                if (File.Exists(d.FileName))
                    File.Delete(d.FileName);
                switch (d.FilterIndex) {
                    case 1:
                        SelectedWalletNotNull().Wallet.ExportWalletToBdb(d.FileName);
                        break;
                }
            }
        }

        void OnFileImportBlockchain(object sender, RoutedEventArgs e) {
            var wf = SelectedWalletNotNull();
            var d = new OpenFileDialog();
            d.FileName = "bootstrap.dat";
            d.Filter = "Bootstrap.dat format|bootstrap.dat";
            if (Dialog.ShowDialog(d, this))
                wf.Wallet.ImportFromBootstrapDat(d.FileName);
        }

        void OnFileExportBlockchain(object sender, RoutedEventArgs e) {
            var wf = SelectedWalletNotNull();
            var d = new SaveFileDialog();
            d.FileName = "bootstrap.dat";
            d.Filter = "Bootstrap.dat format|bootstrap.dat";
            if (Dialog.ShowDialog(d, this))
                wf.Wallet.ExportToBootstrapDat(d.FileName);
        }

        private void OnHelp(object sender, RoutedEventArgs e) {
            string exe = Assembly.GetExecutingAssembly().Location;
            Process.Start(Path.Combine(Path.GetDirectoryName(exe), Path.GetFileNameWithoutExtension(exe) + ".htm"));
        }

        private void OnFileCompact(object sender, RoutedEventArgs e) {
            foreach (var wf in ActiveWalletForms)
                wf.Wallet.CompactDatabase();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {
            ContextMenu menu = (ContextMenu)sender;
            var wf = SelectedWallet();
            menu.DataContext = wf;
/*
            MenuDBMode.IsEnabled = wf != null;
            if (MenuDBMode.IsEnabled)
                SetMenuDBMode();
*/
            }

        private void menuMining_Checked(object sender, RoutedEventArgs e) {
//            SaveActiveCurrencies();
        }

        private void OnChangeWalletPassword(object sender, RoutedEventArgs e) {
            var dlg = new FormPassphrase();
            dlg.labelOldPassword.Visibility = Visibility.Visible;
            dlg.textOldPassword.Visibility = Visibility.Visible;
            dlg.textOldPassword.Focus();
            if (Dialog.ShowDialog(dlg, this)) {
                Eng.ChangePassword(dlg.textOldPassword.Text, dlg.textPassword.Password);
            }
        }

        private void TrayIcon_TrayMouseClick(object sender, RoutedEventArgs e) {
            Activate();
        }

        private void OnSendToRecipient(object sender, IWallet wallet, string address, decimal? amount, string comment) {
            CtlSendMoney.textAddress.Text = address;
            CtlSendMoney.textComment.Text = comment;
            tabSend.Focus();
        }

        /*!!!?
        private void menuMining_Checked(object sender, RoutedEventArgs e) {
            var sk = UserAppRegistryKey.OpenSubKey(TheWallet.CurrencySymbol, true);
            if (sk != null)
                sk.SetValue("Mining", TheWallet.MiningEnabled ? 1 : 0);
        }*/

    }

    public class WalletForms : INotifyPropertyChanged {
        public IWallet Wallet;
        public MenuItem MenuItem;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Free() {
            Wallet = null;
            /*!!!T
            if (FormTransactions != null) {
                FormTransactions.Close();
                FormTransactions = null;
            }
            if (FormAddressBook != null) {
                FormAddressBook.Close();
                FormAddressBook = null;
            }*/
        }



        public Uri IconUri { get { return new Uri("images/" + Regex.Replace(Wallet.CurrencyName, @"-testnet\d?", "") + ".ico", UriKind.Relative); } }
        public string CurrencySymbol { get { return Wallet.CurrencySymbol; } }
        public string Balance { get { return Wallet.Balance.ToString("0.########"); } }
        public string BlockHeight =>  Wallet.LastBlock.ToString("n0", CultureInfo.InvariantCulture);
        public string State { get { return Wallet.State; } }
        public int Peers { get { return Wallet.Peers; } }

        public bool MiningAllowed { get { return Wallet.MiningAllowed; } }
        public bool MiningEnabled { get { return Wallet.MiningEnabled; } set { Wallet.MiningEnabled = value; } }

        public bool LiteModeEnabled { get { return Wallet.Mode == EEngMode.Lite; } set { Wallet.Mode = value ? EEngMode.Lite : (Wallet.CurrencySymbol=="BTC" ? EEngMode.Bootstrap : EEngMode.Normal); } }
        public bool LiteModeAllowed { get { return Wallet.LiteModeAllowed; } }

        public bool CheckForChanges() {
            if (Wallet == null)
                return false;
            bool r = Wallet.GetAndResetStateChangedFlag();
            if (r) {
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Balance"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BlockHeight"));
                    PropertyChanged(this, new PropertyChangedEventArgs("State"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Peers"));
                }
            }
            return r;
        }
    }

    public class Win32WindowHandle : System.Windows.Forms.IWin32Window {
        private IntPtr _handle;

        public Win32WindowHandle(IntPtr handle) {
            _handle = handle;
        }

        public IntPtr Handle {
            get { return _handle; }
        }
    }

    public class MyImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string imageName = (string)value;
            return new BitmapImage(new Uri("images/" + imageName + ".ico", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
