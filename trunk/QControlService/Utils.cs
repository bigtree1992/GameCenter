
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace QControlService
{
    internal class Utils
    {
        /// <summary>
        /// 返回第一个被找到的IPV4地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var len = interfaces.Length;
            
            //如果是本地连接 优先使用本地连接
            for (int i = 0; i < len; i++)
            {
                var ni = interfaces[i];
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (!ni.Name.Contains("本地"))
                    {
                        continue;
                    }

                    var property = ni.GetIPProperties();
                    foreach (var ip in property.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            //过滤169.开头的ip   过滤  192.168.10.开头的ip
                            if (ip.Address.ToString().StartsWith("169."))
                            {
                                continue;
                            }
                            return ip.Address.ToString();
                        }
                    }
                }
            }

            return "";
        }
    }
}
