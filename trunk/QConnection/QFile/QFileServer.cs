using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace QConnection
{
    public class QFileServer
    {
        private const string ServerName = "FileZilla Server.exe";
        private const string ConfigPath = "FileZilla Server.xml";
        
        /// <summary>
        /// 启动服务，设置防火墙例外，重新加载配置文件
        /// 需要传入一个绝对路径
        /// </summary>
        public void Start(string dir)
        {            
            bool exist = File.Exists(ServerName);
            if (exist)
            {
                
                var ps = Process.GetProcessesByName(ServerName.Replace(".exe",""));
                if (ps.Length == 0)
                {
                    ReSetConfig(ConfigPath, "FantasyForest", dir);
                    ServerCtrl("/install auto");
                    ServerCtrl("/start");
                }
                else
                {
                    var path = Path.GetDirectoryName(ps[0].MainModule.FileName);
                    ReSetConfig($"{path}/{ConfigPath}", "FantasyForest", dir);                   
                }
                
                ServerCtrl("/reload-config");             
            }
            else
            {
                Log.Error("[QFileServer] " + ServerName + " Not Exist.");
            }
        }

        private void ServerCtrl(string args)
        {
            try
            {
                var startInfo = new ProcessStartInfo();
                startInfo.FileName = ServerName;
                startInfo.Arguments = args;
                Process.Start(startInfo);
            }
            catch(Exception e)
            {
                Log.Error("[QFileServer] Server Ctrl Failed : " + e.Message);
            }
            
        }

/*      
 *      FTP服务器为永久开启，没有停止方法  
        private void Stop()
        {
            bool exist = File.Exists(ServerName);
            if (exist)
            {
                var ps = Process.GetProcessesByName(ServerName);
                if (ps.Length > 0)
                {
                    ServerCtrl("/stop");
                }
            }
            else
            {
                Log.Error("[QFileServer] " + ServerName + "Not Exist.");
            }
        }
*/
        private void ReSetConfig(string filepath,string username, string dir)
        {
            var content = ConfigContent.Replace("{UserName}", username);
            content = content.Replace("{Dir}", dir);

            FileStream file = null;
            try
            {
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }

                file = File.Open(filepath, FileMode.Create, FileAccess.Write);
                var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                file.Write(buffer, 0, buffer.Length);
                file.Close();              
            }
            catch(Exception e)
            {
                Log.Error("[QFileServer] ReloadConfig Failed : " + e.Message);
            }
            finally
            {
                if(file != null)
                {
                    file.Close();
                }
            }            
        }

        private const string ConfigContent = 
 "<FileZillaServer>"
+   "<Settings> <Item name=\"Admin port\" type=\"numeric\">14147</Item></Settings>"
+   "<Groups />"
+   "<Users>"
+       "<User Name=\"{UserName}\">"
+           "<Option Name=\"Pass\"></Option> <Option Name=\"Salt\"></Option> <Option Name=\"Group\"></Option>" 
+           "<Option Name=\"Bypass server userlimit\">0</Option><Option Name=\"User Limit\">0</Option>" 
+           "<Option Name=\"IP Limit\">0</Option><Option Name=\"Enabled\">1</Option>"
+           "<Option Name=\"Comments\"></Option><Option Name=\"ForceSsl\">0</Option>"
+           "<IpFilter> <Disallowed /> <Allowed /> </IpFilter>"
+           "<Permissions> <Permission Dir=\"{Dir}\" >"
+                   "<Option Name=\"FileRead\">1</Option> <Option Name=\"FileWrite\">0</Option>"
+                   "<Option Name=\"FileDelete\">0</Option> <Option Name=\"FileAppend\">0</Option>"
+                   "<Option Name=\"DirCreate\">0</Option> <Option Name=\"DirDelete\">0</Option>"
+                   "<Option Name=\"DirList\" > 1</Option> <Option Name=\"DirSubdirs\">1</Option>"
+                    "<Option Name=\"IsHome\">1</Option> <Option Name=\"AutoCreate\">0</Option>"
+                "</Permission> </Permissions>"
+                "<SpeedLimits DlType=\"0\" DlLimit=\"10\" ServerDlLimitBypass=\"0\" UlType=\"0\" UlLimit=\"10\" ServerUlLimitBypass=\"0\" > "
+                "<Download /><Upload /> </SpeedLimits>"
+        "</User></Users>"
+  "</FileZillaServer>";
    }
}
