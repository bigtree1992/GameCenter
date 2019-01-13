using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Globalization;

namespace QConnection
{
    public class Utils
    {
        public static void ExecuteCmd(string cmd)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            process.Start();//启动程序

            //向cmd窗口发送输入信息
            process.StandardInput.WriteLine(cmd + "&exit");
            process.StandardInput.AutoFlush = true;
            //获取cmd窗口的输出信息
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();//等待程序执行完退出进程
            process.Close();
        }

        /// <summary>
        /// 使用进程名做唯一标识
        /// </summary>
        /// <param name="show"></param>
        public static void SingleProgramTest(Action show)
        {
            var file = Process.GetCurrentProcess().MainModule.FileName;
            var path = Path.GetDirectoryName(file);
            Directory.SetCurrentDirectory(path);
            var name = Path.GetFileNameWithoutExtension(file);

            var ps = Process.GetProcessesByName(name);
            if (ps.Length > 1)
            {
                show();
                Environment.Exit(0);
            }
            else
            {
                Log.Debug("[QClient] " + file + " " + Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion);
            }
        }
        /// <summary>
        /// 使用窗口名做唯一标识
        /// </summary>
        /// <param name="show"></param>
        /// <param name="centername"></param>
        public static void SingleProgramTest(Action show, string centername)
        {
            var file = Process.GetCurrentProcess().MainModule.FileName;
            var path = Path.GetDirectoryName(file);
            Directory.SetCurrentDirectory(path);
            var name = Path.GetFileNameWithoutExtension(file);

            var ps = Process.GetProcessesByName(name);

            if (ps[0].MainWindowTitle.Contains(centername))
            {
                show();
                Environment.Exit(0);
            }
            else
            {
                Log.Debug("[QClient] " + file + " " + Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion);
            }
        }

        /// <summary>
        /// 返回第一个被找到的IPV4地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var len = interfaces.Length;
            var count = 0;
            while (count < 10)
            {
                //如果是本地连接 优先使用本地连接
                for (int i = 0; i < len; i++)
                {
                    NetworkInterface ni = interfaces[i];
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        if (ni.Name.Contains("本地"))
                        {
                            IPInterfaceProperties property = ni.GetIPProperties();
                            foreach (UnicastIPAddressInformation ip in
                                property.UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    //过滤169.开头的ip   过滤  192.168.10.开头的ip
                                    if (ip.Address.ToString().StartsWith("169.") || ip.Address.ToString().StartsWith("192.168.10."))
                                    {
                                        continue;
                                    }
                                    return ip.Address.ToString();
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                }
                Thread.Sleep(100);
                count++;
            }

            return "";
        }

        //先获取本地的所有IP
        static List<string> GetIP()
        {
            List<string> local = new List<string>();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            local.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            return local;
        }

        //获取正在使用的所有ip，与本地IP比较，得出目标IP
        internal static string GetTargetIP(List<string> localIP)
        {
            var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

            string target = null;

            for (int i = 0; i < addressList.Length; i++)
            {
                IPAddress iPAddress = addressList[i];

                if (iPAddress.AddressFamily == AddressFamily.InterNetwork && iPAddress.ToString().StartsWith("192.168."))
                {
                    string ip = iPAddress.ToString();

                    foreach (var v in localIP)
                    {
                        if (ip == v)
                        {
                            target = ip;

                            return target;
                        }
                    }
                }
            }

            return target;
        }

        public static string Base64ToString(string source)
        {
            if (source == null)
            {
                return null;
            }

            string target = null;
            byte[] bpath = Convert.FromBase64String(source);
            target = System.Text.Encoding.UTF8.GetString(bpath);

            return target;
        }

        public static string StringToBase64(string source)
        {
            if (source == null)
            {
                return null;
            }

            string target = null;
            System.Text.Encoding encode = System.Text.Encoding.UTF8;
            byte[] bytedata = encode.GetBytes(source);
            target = Convert.ToBase64String(bytedata, 0, bytedata.Length);

            return target;
        }
        private static byte[] __IV = { 0x14, 0xFC, 0x29, 0xC8, 0x9A, 0xCB, 0xCA, 0xE7 };//加密IV向量
        /// <summary>
        /// DES算法描述简介：
        /// DES是Data Encryption Standard（数据加密标准）的缩写。它是由IBM公司研制的一种加密算法，
        /// 美国国家标准局于1977年公布把它作为非机要部门使用的数据加密标准；
        /// 它是一个分组加密算法，他以64位为分组对数据加密。
        /// 同时DES也是一个对称算法：加密和解密用的是同一个算法。
        /// 它的密匙长度是56位（因为每个第8位都用作奇偶校验），
        /// 密匙可以是任意的56位的数，而且可以任意时候改变．
        /// </summary>
        public static String Encrypt(String Key, String str)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(Key.Substring(0, 8));
            byte[] bIV = __IV;
            byte[] bStr = Encoding.UTF8.GetBytes(str);
            try
            {
                var desc = new DESCryptoServiceProvider();
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, desc.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write);
                cStream.Write(bStr, 0, bStr.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error("[Encrypt] error:" + ex.Message);
                return string.Empty;
            }
        }

        public static String Decrypt(String Key, String DecryptStr)
        {
            if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(DecryptStr))
            {
                return string.Empty;
            }

            try
            {
                byte[] bKey = Encoding.UTF8.GetBytes(Key.Substring(0, 8));
                byte[] bIV = __IV;
                byte[] bStr = Convert.FromBase64String(DecryptStr);
                var desc = new DESCryptoServiceProvider();
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, desc.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write);
                cStream.Write(bStr, 0, bStr.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error("[Decrypt] error:" + ex.Message);
                return string.Empty;
            }
        }

        public static string ReadReg(string subkey)
        {
            string value = null;

            try
            {
                var key = Registry.LocalMachine;
                var read = key.OpenSubKey(@"software\game", true);

                if (read.GetValue(subkey) != null)
                {
                    value = read.GetValue(subkey).ToString();
                }
                else
                {
                    Log.Warning("[ReadReg] warning: 键不存在!");
                }

                read.Close();
                key.Close();

                return value;
            }
            catch (Exception ex)
            {
                Log.Error("[ReadReg] error:" + ex.Message);
                return value;
            }
        }

        public static void WriteReg(string subkey, string value)
        {
            try
            {
                var key = Registry.LocalMachine;
                var write = key.OpenSubKey(@"software\game", true);

                write.SetValue(subkey, value);

                write.Close();
                key.Close();
            }
            catch (Exception ex)
            {
                Log.Error("[WriteReg] error:" + ex.Message);
            }
        }

        /// <summary>
        /// 日期转换成unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return Convert.ToInt64((dateTime - start).TotalSeconds);
        }

        /// <summary>
        /// unix时间戳转换成日期
        /// </summary>
        /// <param name="unixTimeStamp">时间戳（秒）</param>
        /// <returns></returns>
        public static DateTime UnixTimestampToDateTime(long timestamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return start.AddSeconds(timestamp);
        }

        public static void DebugBuffer(byte[] buffer, int start, int count)
        {
            string content = "";
            for (int i = 0; i < count; i++)
            {
                content += (" " + buffer[start + i].ToString("x"));
            }
            Log.Debug("[DebugBuffer] " + content);
        }

        public static bool RunCmd(string cmdExe, string cmdStr)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                    myPro.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                    myPro.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                    myPro.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                    myPro.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                    myPro.Start();
                    //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                    string str = string.Format(@"""{0}"" {1} {2}", cmdExe, cmdStr, "&exit");

                    //向cmd窗口发送输入信息
                    myPro.StandardInput.WriteLine(str + "&exit");
                    myPro.StandardInput.AutoFlush = true;
                    myPro.WaitForExit();

                    //获取cmd窗口的输出信息
                    string output = myPro.StandardOutput.ReadToEnd();
                    myPro.WaitForExit();//等待程序执行完退出进程
                    myPro.Close();
                    Console.WriteLine(output);
                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }

        public static void DeleteBorder(Process process)
        {
            while (true)
            {
                var winhandler = process.MainWindowHandle;

                if (winhandler != IntPtr.Zero)
                {
                    Int32 wndStyle = GetWindowLong(winhandler, GWL_STYLE);
                    wndStyle &= ~WS_BORDER;
                    wndStyle &= ~WS_THICKFRAME;
                    SetWindowLong(winhandler, GWL_STYLE, wndStyle);
                    process.Refresh();
                    return;
                }
                Thread.Sleep(100);
            }
        }


        public static string MachineStatus(string strIpOrDName)
        {
            try
            {
                var objPingSender = new Ping();
                var objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                var data = "";
                var buffer = Encoding.UTF8.GetBytes(data);
                var intTimeout = 120;
                var objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                var strInfo = objPinReply.Status.ToString();

                return strInfo;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        //将Bitmap格式转换为ImageSource
        public static ImageSource ChangeBitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();

            var wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }

            return wpfBitmap;
        }

        public static Cursor CreateBmpCursor(string path)
        {
            var imagefullpath = Path.GetFullPath(path);

            if (File.Exists(imagefullpath) == false)
            {
                throw new Exception("[Utils] CreateBmpCursor Error : Not Exist File .");
            }
            var map = new Bitmap(imagefullpath);
            return BitmapCursor.CreateBmpCursor(map);
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int WS_THICKFRAME = 262144;   //具有可调边框
        const int WS_BORDER = 8388608;      //单边框窗口
        const int WS_CAPTION = 12582912;    //带标题栏的窗口
        const int WS_MAXIMIZEBOX = 65536;   //带最大化按钮的窗口
        const int WS_MINIMIZE = 536870912;  //窗口最小化
        const int GWL_STYLE = -16;
       
        /// <summary>
        /// 获取ZIP文件中的所有文件更新时间信息
        /// </summary>
        /// <param name="zipFile"></param>
        /// <param name="ignoreList"></param>
        /// <returns></returns>
        public static List<QProtocols.FileInfo> GetFileInfoFromZip(string zipFile, List<string> ignoreList)
        {
            var fileInfos = new List<QProtocols.FileInfo>();
            ZipInputStream zipfiles = null;
            try
            {
                zipfiles = new ZipInputStream(File.OpenRead(zipFile));
                ZipEntry theEntry = null;
                while ((theEntry = zipfiles.GetNextEntry()) != null)
                {
                    bool skip = false;
                    foreach (var pattern in ignoreList)
                    {
                        if (theEntry.Name.Contains(pattern))
                        {
                            skip = true;
                        }
                    }

                    if (skip)
                    {
                        continue;
                    }

                    theEntry.IsUnicodeText = true;
                    if (theEntry.IsFile)
                    {                        
                        fileInfos.Add(new QProtocols.FileInfo
                        {
                            Path = theEntry.Name.Replace("/","\\"),
                            Info = theEntry.DateTime.ToString("yyyy-MM-dd HH:mm")
                        });
                    }
                }
            }
            finally
            {
                if (zipfiles != null)
                {
                    zipfiles.Close();
                }
            }
            return fileInfos;
        }

        /// <summary>  
        /// 获取路径下所有文件以及子文件夹中文件更新时间信息 
        /// </summary>  
        /// <param name="path">根目录</param>  
        /// <param name="ignoreList">忽略列表</param>  
        /// <returns></returns>  
        public static List<QProtocols.FileInfo> GetFileInfoFromPath(string path, List<string> ignoreList)
        {
            var fileInfos = new List<QProtocols.FileInfo>();

            try
            {
                var dir = new DirectoryInfo(path);
                var files = dir.GetFiles();
                var subDir = dir.GetDirectories();

                foreach (var f in files)
                {
                    fileInfos.Add(new QProtocols.FileInfo {
                        Path = f.Name,
                        Info = f.LastWriteTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    });
                }

                //获取子文件夹内的文件列表，递归遍历  
                foreach (var d in subDir)
                {
                    if (ignoreList.Contains(d.Name))
                    {
                        continue;                       
                    }

                    var sub = GetFileInfoFromPath(d.FullName, ignoreList);
                    foreach (var info in sub)
                    {
                        fileInfos.Add(new QProtocols.FileInfo
                        {
                            Path = $"{d.Name}\\{info.Path}",
                            Info = info.Info
                        });
                    }
                }
            }
            catch
            {
                return fileInfos;
            }

            return fileInfos;
        }

        public static string GetFileFromZip(string zip, string pattern)
        {
            string versiong;
            ZipInputStream zipfiles = null;
            try
            {
                zipfiles = new ZipInputStream(File.OpenRead(zip));
                ZipEntry theEntry;
                while ((theEntry = zipfiles.GetNextEntry()) != null)
                {
                    if (theEntry.IsFile &&
                        theEntry.Name.Contains(pattern))
                    {
                        return versiong = theEntry.Name;
                    }
                }
            }
            finally
            {
                if (zipfiles != null)
                {
                    zipfiles.Close();
                }
            }

            return "";
        }

        public static List<QProtocols.FileInfo> GetVersionInfoFromZip(string zipsPath)
        {
            var infos = new List<QProtocols.FileInfo>();
            var dir = new DirectoryInfo(zipsPath);
            var files = dir.GetFiles();

            foreach (var file in files)
            {
                if (!file.Name.EndsWith(".zip"))
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(file.Name);
                var version = GetFileFromZip($"{zipsPath}\\{file.Name}", ".Version");
                version = version.Replace(".Version", "");

                infos.Add(new QProtocols.FileInfo
                {
                    Path = $"{zipsPath}\\{name}",
                    Info = version
                });
            }
            return infos;
        }

        public static string GetFileFromPath(string path, string pattern)
        {
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles();
            foreach (var f in files)
            {
                if (f.Name.Contains(pattern))
                {
                    return f.Name;
                }
            }
            return "";
        }

        public static List<QProtocols.FileInfo> GetVersionInfo(string path)
        {
            var infos = new List<QProtocols.FileInfo>();

            var dir = new DirectoryInfo(path);
            var dirs = dir.GetDirectories();

            foreach(var d in dirs)
            {
                var version = GetFileFromPath(d.FullName, ".Version");
                version = version.Replace(".Version", "");

                infos.Add(new QProtocols.FileInfo
                {
                    Path = $"{path}\\{d.Name}",
                    Info = version
                });
            }

            return infos;
        }
    }
}
