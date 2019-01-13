using System;
using System.Diagnostics;
using System.Management;
using System.Windows;

namespace QSetIP
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var config = IPInfoConfig.LoadData("Configs/IPInfoConfig.xml");
            if(config == null)
            {
                MessageBox.Show("无法加载IPInfoConfig配置文件");
                this.Close();
                Environment.Exit(0);
                return;
            }

            IPComboBox.ItemsSource = config.IPInfos;
            IPComboBox.SelectedIndex = 0;
        }

        private void OnOKClick(object sender, RoutedEventArgs e)
        {
            var info = IPComboBox.SelectedItem as IPInfo;

            if (info.IPAddress == "自动获取")
            {
                setDHCP();
            }
            else
            {
                SetNetworkAdapter(info);
            }
        }
        
        private void setDHCP()
        {
            string _doscmd = "netsh interface ip set address 本地连接 DHCP";
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine(_doscmd.ToString());
            _doscmd = "netsh interface ip set dns 本地连接 DHCP";
            p.StandardInput.WriteLine(_doscmd.ToString());
            p.StandardInput.WriteLine("exit");
        }

        private void SetNetworkAdapter(IPInfo info)
        {

            //var regeIP = new Regex(@"^(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5])$");

            //if(!regeIP.IsMatch(info.IPAddress))
            //{
            //    MessageBox.Show("请配置正确的IPAddress地址");
            //    return;
            //}
            
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (!(bool)mo["IPEnabled"])
                    continue;

                //设置ip地址和子网掩码 
                inPar = mo.GetMethodParameters("EnableStatic");
                inPar["IPAddress"] = new string[] { info.IPAddress, "192.168.10.9" };
                inPar["SubnetMask"] = new string[] { info.SubnetMask, "255.255.255.0" };
                outPar = mo.InvokeMethod("EnableStatic", inPar, null);

                //设置网关地址 
                inPar = mo.GetMethodParameters("SetGateways");
                inPar["DefaultIPGateway"] = new string[] { info.DefaultIPGateway, "192.168.10.1" };
                outPar = mo.InvokeMethod("SetGateways", inPar, null);

                //设置DNS 
                inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                inPar["DNSServerSearchOrder"] = new string[] { "114.114.114.114", "179.32.42.5" };
                outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                break;
            }
        }

    }
}
