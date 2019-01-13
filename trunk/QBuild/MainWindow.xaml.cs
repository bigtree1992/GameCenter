using QConnection;
using QGameCenterLogic;
using System;
using System.IO;
using System.Threading;
using System.Windows;



namespace QBuild
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string m_dotNETTool = @"..\..\Tools\.NET Reactor\dotNET_Reactor.exe";

        private string m_dotCmd = " -necrobit [1] -suppressildasm [1] -antitamp [1] -stringencryption[1]  -resourceencryption [1]  -flow_level [9]";

        public MainWindow()
        {
            InitializeComponent();
            Log.SetLogToFile();
        }

        /// <summary>
        /// 1.移动中控 -> 加密中控 -> 删除不必要文件 -> 修改名称  
        /// 2.移动各种工具  ->  加密各种工具  -> 删除不必要文件  -> 修改名称  -> 然后将exe以及exe,config移动到中控
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HorseWarriorClick(object sender, RoutedEventArgs e)
        {
            CloseButton();

            if (!PackACenter("QHorseGame", "HorseCenter", "HorseCenter", @"HorseCenter\QHorseCenter.exe"))
            {
                MessageBox.Show("马战打包中控失败.");
            }
            else if (!ChangeCenterName("HorseCenter", "游戏启动器"))
            {
                MessageBox.Show("马战中控修改名称失败.");
            }
            else if (!PackTools("HorseCenter"))
            {

            }
            else
            {
                try
                {
                    var dataDir = Path.GetFullPath(@"..\HorseCenter\Data");

                    if (!Directory.Exists(dataDir))
                    {
                        //移动gamebasedb
                        Directory.CreateDirectory(dataDir);
                        File.Copy(Path.GetFullPath(@"..\..\trunk\DateBase\GameCenterDB.db"), dataDir + @"\GameCenterDB.db", true);

                    }
                    var resourcesDir = Path.GetFullPath(@"..\HorseCenter\Resources");
                    if (!Directory.Exists(resourcesDir))
                    {
                        //移动gamebasedb
                        Directory.CreateDirectory(resourcesDir);
                    }

                    var configDir = Path.GetFullPath(@"..\HorseCenter\Configs");
                    if (!Directory.Exists(configDir))
                    {
                        //创建Configs
                        Directory.CreateDirectory(configDir);
                        var configSourcesDir = Path.GetFullPath(@"..\..\trunk\Configs\Horse");
                        var dir = new DirectoryInfo(configSourcesDir);
                        foreach (var file in dir.GetFiles())
                        {
                            File.Copy(file.FullName, configDir + @"\" + file.Name, true);
                        }

                    }

                    //删除QClinet -> MachineTools下面的多余的文件夹
                    var machineToolsDir = Path.GetFullPath(@"..\QClient\MachineTools");
                    if (Directory.Exists(machineToolsDir))
                    {
                        var dir = new DirectoryInfo(machineToolsDir);
                        foreach (var d in dir.GetDirectories())
                        {
                            if (d.FullName.Contains("CloseGa") || d.FullName.Contains("SuperHorseFull"))
                            {

                            }
                            else
                            {
                                var di = new DirectoryInfo(d.FullName);
                                di.Delete(true);
                            }

                        }
                    }
                    else
                    {
                        MessageBox.Show("QClient文件件下面不存在MachineTools文件夹，出现错误.打包失败.");
                        return;
                    }


                    MessageBox.Show("马战中控打包成功.");
                }
                catch (Exception ex)
                {
                    Log.Error("[QBuild] Errro : " + ex.ToString());
                }
            }

            OpenButton();
        }

        private void OnVRKartButtonClick(object sender, RoutedEventArgs e)
        {
            CloseButton();

            if (!PackACenter("QVRKart", "VRKart", "VRKart", @"VRKart\QVRKart.exe"))
            {
                MessageBox.Show("VRKart打包中控失败.");
            }
            else if (!ChangeCenterName("VRKart", "游戏启动器"))
            {
                MessageBox.Show("VRKart中控修改名称失败.");
            }
            else if (!PackTools("VRKart", false))
            {

            }
            else
            {
                try
                {
                    MessageBox.Show("VRKart中控打包成功.");
                }
                catch (Exception ex)
                {
                    Log.Error("[QBuild] Errro : " + ex.ToString());
                }
            }

            OpenButton();
        }

        private bool PackTools(string centerName, bool isBuildClient = true)
        {
            //HorseCenter
            if (!PackEevntStatistics())
            {
                MessageBox.Show("打包事件管理器失败.");
            }
            else if (!MoveToolToCenter("QEventStatistics", centerName))
            {
                MessageBox.Show(centerName + "移动事件管理器失败.");
            }
            else if (!PackUpdateGame())
            {
                MessageBox.Show("打包游戏升级系统失败.");
            }
            //else if (!MoveToolToCenter("QGameManager", centerName))
            //{
            //    MessageBox.Show(centerName + "移动游戏管理器失败.");
            //}
            else if (!MoveToolToCenter("QUpdateGame", centerName))
            {
                MessageBox.Show(centerName + "移动游戏升级系统失败.");
            }
            else if (isBuildClient && !PackXmlView())
            {
                MessageBox.Show("打包XmlViwe管理器失败.");
            }
            else if (isBuildClient && !MoveToolToCenter("QXMLFileView", centerName))
            {
                MessageBox.Show(centerName + "移XmlView失败.");
            }
            else if (!PackSetIP())
            {
                MessageBox.Show("打包SetIP设置器器失败.");
            }
            else if (!MoveToolToCenter("QSetIP", centerName))
            {
                MessageBox.Show("SetIP设置器移动失败.");
            }
            else if (isBuildClient && !PackClient())
            {
                MessageBox.Show("打包客户端失败.");
            }
            else if (isBuildClient && !MoveToolToCenter("QSetIP", "QClient")) //将SetIP移动到Client
            {
                MessageBox.Show("SetIP设置器移动失败.");
            }
            else if (!PackControlManager())
            {
                MessageBox.Show("打包远程操作系统（服务端）失败.");
            }
            else if (!MoveToolToCenter("QControlManager", centerName))
            {
                MessageBox.Show(centerName + "移动远程操作系统（服务端）失败.");
            }
            else if (isBuildClient && !PackControlService())
            {
                MessageBox.Show("打包远程操作系统（客户端）失败.");
            }
            else if (isBuildClient && !MoveToolToCenter("QControlService", "QClient")) //将ControlService移动到Client
            {
                MessageBox.Show("移动远程操作系统（客户端）失败.");
            }

            return true;
        }

        private void OnHTankButtonClick(object sender, RoutedEventArgs e)
        {
            CloseButton();

            if (!PackACenter("QHTankGame", "HTankCenter", "HTankCenter", @"HTankCenter\HorizontalTankCenterControl.exe"))
            {
                MessageBox.Show("横版坦克打包中控失败.");
            }
            else if (!ChangeCenterName("HorseCenter", "游戏启动器"))
            {
                MessageBox.Show("横版坦克修改名称失败.");
            }
            else if (!PackTools("HTankCenter"))
            {
                return;
            }
            else
            {
                try
                {
                    var dataDir = Path.GetFullPath(@"..\HTankCenter\Data");

                    if (!Directory.Exists(dataDir))
                    {
                        //移动gamebasedb
                        Directory.CreateDirectory(dataDir);
                        File.Copy(Path.GetFullPath(@"..\..\trunk\DateBase\GameCenterDB.db"), dataDir + @"\GameCenterDB.db", true);

                    }
                    var resourcesDir = Path.GetFullPath(@"..\HTankCenter\Resources");
                    if (!Directory.Exists(resourcesDir))
                    {
                        //移动gamebasedb
                        Directory.CreateDirectory(resourcesDir);
                    }
                    var configDir = Path.GetFullPath(@"..\HTankCenter\Configs");
                    if (!Directory.Exists(configDir))
                    {
                        //创建Configs
                        Directory.CreateDirectory(configDir);
                        var configSourcesDir = Path.GetFullPath(@"..\..\trunk\Configs\Tank");
                        var dir = new DirectoryInfo(configSourcesDir);
                        foreach (var file in dir.GetFiles())
                        {
                            File.Copy(file.FullName, configDir + @"\" + file.Name, true);
                        }

                    }

                    //删除QClinet -> MachineTools下面的多余的文件夹
                    var machineToolsDir = Path.GetFullPath(@"..\QClient\MachineTools");
                    if (!Directory.Exists(machineToolsDir))
                    {
                        var dir = new DirectoryInfo(machineToolsDir);
                        foreach (var d in dir.GetDirectories())
                        {
                            if (d.FullName.Contains("ResetHeadset") || d.FullName.Contains("ResetMachine"))
                            {

                            }
                            else
                            {
                                var di = new DirectoryInfo(d.FullName);
                                di.Delete(true);
                            }

                        }
                    }
                    else
                    {
                        MessageBox.Show("QClient文件件下面不存在MachineTools文件夹，出现错误.打包失败.");
                        return;
                    }

                    MessageBox.Show("横版坦克打包成功.");
                }
                catch (Exception ex)
                {
                    Log.Error("[QBuild] Errro : " + ex.ToString());
                }
            }
            OpenButton();
        }

        private void OnVTankButtonClick(object sender, RoutedEventArgs e)
        {
            CloseButton();
            if (!PackACenter("QVTankGame", "VTankCenter", "VTankCenter", @"VTankCenter\VerticalTankCenterControl.exe"))
            {
                MessageBox.Show("竖版坦克打包中控失败.");
            }
            else if (!ChangeCenterName("VTankCenter", "游戏启动器"))
            {
                MessageBox.Show("竖版坦克修改名称失败.");
            }
            else if (!PackTools("VTankCenter"))
            {

            }
            else
            {
                try
                {
                    var dataDir = Path.GetFullPath(@"..\VTankCenter\Data");

                    if (!Directory.Exists(dataDir))
                    {
                        //移动gamebasedb
                        Directory.CreateDirectory(dataDir);
                        File.Copy(Path.GetFullPath(@"..\..\trunk\DateBase\GameCenterDB.db"), dataDir + @"\GameCenterDB.db", true);

                    }
                    var resourcesDir = Path.GetFullPath(@"..\VTankCenter\Resources");
                    if (!Directory.Exists(resourcesDir))
                    {
                        //移动gamebasedb
                        Directory.CreateDirectory(resourcesDir);
                    }
                    var configDir = Path.GetFullPath(@"..\VTankCenter\Configs");
                    if (!Directory.Exists(configDir))
                    {
                        //创建Configs
                        Directory.CreateDirectory(configDir);
                        var configSourcesDir = Path.GetFullPath(@"..\..\trunk\Configs\Tank");
                        var dir = new DirectoryInfo(configSourcesDir);
                        foreach (var file in dir.GetFiles())
                        {
                            File.Copy(file.FullName, configDir + @"\" + file.Name, true);
                        }
                    }

                    //删除QClinet -> MachineTools下面的多余的文件夹
                    //var machineToolsDir = Path.GetFullPath(@"..\QClient\MachineTools");
                    //if (!Directory.Exists(machineToolsDir))
                    //{
                    //    var dir = new DirectoryInfo(machineToolsDir);
                    //    foreach (var d in dir.GetDirectories())
                    //    {
                    //        //if (d.FullName.Contains("ResetHeadset") || d.FullName.Contains("ResetMachine"))
                    //        //{

                    //        //}
                    //        //else
                    //        //{
                    //        //    var di = new DirectoryInfo(d.FullName);
                    //        //    di.Delete(true);
                    //        //}

                    //    }
                    //}
                    //else
                    //{
                    //    MessageBox.Show("QClient文件件下面不存在MachineTools文件夹，出现错误.打包失败.");
                    //    return;
                    //}

                    MessageBox.Show("竖版坦克打包成功.");
                }
                catch (Exception ex)
                {
                    Log.Error("[QBuild] Errro : " + ex.ToString());
                }

            }
            OpenButton();
        }


        private bool PackGameManager()
        {
            if (!PackACenter("QGameManager", "QGameManager", "QGameManager", @"QGameManager\QGameManager.exe"))
            {
                MessageBox.Show("竖版坦克打包游戏管理器失败.");
                return false;
            }
            else if (!ChangeCenterName("QGameManager", "游戏管理器"))
            {
                MessageBox.Show("事件管理器修改名称失败.");
                return false;
            }
            else
            {
                //MessageBox.Show("游戏管理器成功.");

            }
            return true;
        }

        private bool PackUpdateGame()
        {
            if (!PackACenter("QUpdateGame", "QUpdateGame", "QUpdateGame", @"QUpdateGame\QUpdateGame.exe"))
            {
                MessageBox.Show("游戏升级系统打包失败.");
                return false;
            }
            else if (!ChangeCenterName("QGameManager", "游戏升级系统"))
            {
                MessageBox.Show("游戏升级系统修改名称失败.");
                return false;
            }
            else
            {
                
            }
            return true;
        }


        private bool PackControlManager()
        {
            if (!PackACenter("QControlManager", "QControlManager", "QControlManager", @"QControlManager\QControlManager.exe"))
            {
                MessageBox.Show("远程操作系统（服务端）打包失败.");
                return false;
            }
            else if (!ChangeCenterName("QControlManager", "远程操作系统"))
            {
                MessageBox.Show("远程操作系统（服务端）修改名称失败.");
                return false;
            }
            else
            {

            }
            return true;
        }
        
        private bool PackControlService()
        {
            if (!PackACenter("QControlService", "QControlService", "QControlService", @"QControlService\QControlService.exe"))
            {
                MessageBox.Show("远程操作系统（客户端）打包失败.");
                return false;
            }
            else if (!ChangeCenterName("QControlService", "远程操作系统"))
            {
                MessageBox.Show("远程操作系统（客户端）修改名称失败.");
                return false;
            }
            else
            {

            }
            return true;
        }

        private bool PackEevntStatistics()
        {
            //LogTxt.Text = "打包事件管理器中...";
            if (!PackACenter("QEventStatistics", "QEventStatistics", "QEventStatistics", @"QEventStatistics\QEventStatistics.exe"))
            {
                MessageBox.Show("事件管理器失败.");
                return false;
            }
            else if (!ChangeCenterName("QEventStatistics", "事件统计管理器"))
            {
                MessageBox.Show("事件管理器修改名称失败.");
                return false;
            }
            else
            {
                //MessageBox.Show("事件管理器成功.");
            }
            return true;
        }

        private bool PackClient()
        {
            if (!PackACenter("QClient", "QClient", "QClient", @"QClient\奇境森林中控客户端.exe"))
            {
                MessageBox.Show("客户端打包失败.");
                return false;
            }
            else
            {
                //将MachineTools移动到Client下面
                var sorDir = @"..\..\trunk\MachineTools";
                var desDir = @"..\QClient\MachineTools";

                if (!Directory.Exists(Path.GetFullPath(desDir)))
                {
                    Directory.CreateDirectory(Path.GetFullPath(desDir));
                }

                //文件移动
                try
                {
                    FileOperation.CopyDirFile(Path.GetFullPath(sorDir), Path.GetFullPath(desDir));
                }
                catch (Exception e)
                {
                    Log.Debug("[Build] PackClient MoveMachineTools Error : " + e.Message);
                    return false;
                }
            }
            return true;
        }

        private bool PackXmlView()
        {
            if (!PackACenter("QXMLFileView", "QXMLFileView", "QXMLFileView", @"QXMLFileView\QXMLFileView.exe"))
            {
                MessageBox.Show("XmlView打包失败.");
                return false;
            }
            else if (!ChangeCenterName("QXMLFileView", "客户端配置工具"))
            {
                MessageBox.Show("Xml查看器修改名称失败.");
                return false;
            }
            else
            {
                //MessageBox.Show("Xml查看器打包成功.");
            }
            return true;
        }

        private bool PackSetIP()
        {
            if (!PackACenter("QSetIP", "QSetIP", "QSetIP", @"QSetIP\QSetIP.exe"))
            {
                MessageBox.Show("QSetIP打包失败.");
                return false;
            }
            else if (!ChangeCenterName("QSetIP", "IP修改工具"))
            {
                MessageBox.Show("IP修改工具修改名称失败.");
                return false;
            }
            else
            {
                //MessageBox.Show("Xml查看器打包成功.");
            }
            return true;
        }

        private void CloseButton()
        {
            horse.IsEnabled = false;
            tank.IsEnabled = false;
            bte.IsEnabled = false;
            vrkart.IsEnabled = false;
        }
        private void OpenButton()
        {
            horse.IsEnabled = true;
            tank.IsEnabled = true;
            bte.IsEnabled = true;
            vrkart.IsEnabled = true;
        }

        private void OnWindowClose(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sorDirParam">trunk下面的工程名</param>
        /// <param name="desDirParam">Publish下面的名称</param>
        /// <param name="dllPathParam">Publish下面的文件名称</param>
        /// <param name="exePathParam">Publish下面的文件+exe名称</param>
        /// <returns></returns>
        private bool PackACenter(string sorDirParam, string desDirParam, string dllPathParam, string exePathParam)
        {
            var reslut = true;

            //清空目标文件的内容
            try
            {
                //var desDir = @"..\" + desDirParam;
                var path = Path.GetFullPath(@"..\" + desDirParam);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var dir = new DirectoryInfo(path);
                var fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (var i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error("[QBuild] PackACenter Error : " + e.ToString());
                return false;
            }

            var t = new Thread(() =>
            {
                lock (this)
                {
                    if (!MoveAndEncryption(sorDirParam, desDirParam, dllPathParam, exePathParam))
                    {
                        reslut = false;
                    }
                    else if (!DeleteUsenessFile(desDirParam))
                    {
                        reslut = false;
                    }
                }

            });

            t.Start();
            t.Join();
            t.Abort();
            t = null;
            return reslut;
        }

        private bool MoveAndEncryption(string sorDirParam, string desDirParam, string dllPathParam, string exePathParam)
        {

            var sorDir = @"..\..\trunk\" + sorDirParam + @"\bin\Debug";
            var desDir = @"..\" + desDirParam;

            //Log.Debug("sorDir : " + Path.GetFullPath(sorDir) + "  desDir : " + Path.GetFullPath(desDir));


            if (!Directory.Exists(Path.GetFullPath(desDir)))
            {
                Directory.CreateDirectory(Path.GetFullPath(desDir));
            }

            //文件移动
            try
            {
                FileOperation.CopyDirFile(Path.GetFullPath(sorDir), Path.GetFullPath(desDir));
            }
            catch (Exception e)
            {
                Log.Debug("[Build] MoveAndEncryption Error : " + e.Message);
                return false;
            }

            //文件加密
            try
            {
                //加密dll  @"..\HorseCenter\QGameCenterLogic.dll",
                var cmd = "-file " + Path.GetFullPath(@"..\" + dllPathParam + @"\QGameCenterLogic.dll") + m_dotCmd;
                //Log.Debug(" 加密 dll : " + Path.GetFullPath(@"..\" + dllPathParam + @"\QGameCenterLogic.dll"));
                if (File.Exists(Path.GetFullPath(@"..\" + dllPathParam + @"\QGameCenterLogic.dll")))
                {
                    if (Utils.RunCmd(Path.GetFullPath(m_dotNETTool), cmd) == false)
                    {
                        MessageBox.Show("加密DLL失败.");
                        return false;
                    }
                }


                //加密exe @"..\QGameManager\QGameManager.exe"
                cmd = "-file " + Path.GetFullPath(@"..\" + exePathParam) + m_dotCmd;
                //Log.Debug(" 加密 exe : " + Path.GetFullPath(@"..\" + exePathParam));

                if (Utils.RunCmd(Path.GetFullPath(m_dotNETTool), cmd) == false)
                {
                    MessageBox.Show("加密EXE失败.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Debug("[Build] MoveAndEncryption Error : " + e.Message);
                return false;
            }

            var array = exePathParam.Split('\\');
            var nameArray = array[array.Length - 1].Split('.');

            var sorDir_Secure_EXE = @"..\" + desDirParam + @"\" + nameArray[0] + "_Secure";
            var desDir_Secure_EXE = @"..\" + desDirParam;
            var sorDir_Secure_DLL = @"..\" + desDirParam + @"\QGameCenterLogic_Secure";
            var desDir_Secure_DLL = @"..\" + desDirParam;


            try
            {
                FileOperation.CopyDirFile(Path.GetFullPath(sorDir_Secure_EXE), Path.GetFullPath(desDir_Secure_EXE), true);
                if (Directory.Exists(sorDir_Secure_DLL))
                {
                    FileOperation.CopyDirFile(Path.GetFullPath(sorDir_Secure_DLL), Path.GetFullPath(desDir_Secure_DLL), true);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("移动文件夹失败，原因:" + ex.Message);
                Log.Error("移动文件夹失败，原因:" + ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 删除无用文件
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        private bool DeleteUsenessFile(string directoryName)
        {
            try
            {
                var path = Path.GetFullPath(@"..\" + directoryName);
                var dir = new DirectoryInfo(path);

                foreach (var info in dir.GetFiles())
                {
                    var fullName = info.FullName;
                    var extension = Path.GetExtension(fullName);

                    try
                    {
                        if (extension == ".exe" || extension == ".dll")
                        {
                            //保留.exe文件和.dll文件
                            if (fullName.Contains(".vshost"))
                            {
                                File.Delete(fullName);
                            }
                        }
                        else
                        {
                            //保留.exe.config
                            if (!fullName.Contains(".exe.config") || fullName.Contains(".vshost.exe.config"))
                            {
                                File.Delete(fullName);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("[QBild] DeleteUsenessFile Error : " + e.ToString());
                        return false;
                    }
                }

                foreach (var d in dir.GetDirectories())
                {
                    if (d.FullName.Contains("x64") || d.FullName.Contains("x86"))
                    {
                        foreach (var file in d.GetFiles())
                        {
                            File.Copy(file.FullName, path + @"\" + file.Name, true);
                        }
                        var di = new DirectoryInfo(d.FullName);
                        di.Delete(true);
                    }
                    //删除Log文件夹 保证在出包的时候Log文件夹不存在
                    if (d.FullName.Contains("Log"))
                    {
                        var di = new DirectoryInfo(d.FullName);
                        di.Delete(true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[QBild] DeleteUsenessFile Error : " + e.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// 移动工具到中控中
        /// </summary>
        /// <param name="toolsName"></param>
        /// <param name="centerName"></param>
        /// <returns></returns>
        private bool MoveToolToCenter(string toolsName, string centerName)
        {

            try
            {
                var toolDir = new DirectoryInfo(@"..\" + toolsName);
                var centerDir = Path.GetFullPath(@"..\" + centerName);

                if (toolDir.GetFiles().Length < 0)
                {
                    MessageBox.Show("请先打包 : " + toolsName);
                    return false;
                }

                foreach (var file in toolDir.GetFiles())
                {
                    var extension = Path.GetExtension(file.FullName);
                    if (file.Name.Contains(".exe"))
                    {
                        File.Copy(file.FullName, centerDir + @"\" + file.Name, true);
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error("[QBuild] MoveToolToCenter Error : " + e.ToString());
                //return false;
            }

            return true;
        }

        /// <summary>
        /// 将英文名称修改为中文名称
        /// </summary>
        /// <param name="dirParam"></param>
        /// <param name="name"></param>
        private bool ChangeCenterName(string dirParam, string name)
        {
            var path = Path.GetFullPath(@"..\" + dirParam);
            var dir = new DirectoryInfo(path);

            try
            {
                foreach (var file in dir.GetFiles())
                {
                    if (file.FullName.Contains(".exe"))
                    {
                        //.config 配置文件
                        if (file.FullName.Contains(".config"))
                        {
                            File.Copy(file.FullName, path + @"\" + name + ".exe.config", true);
                            File.Delete(file.FullName);
                        }
                        else
                        {
                            //exe文件
                            File.Copy(file.FullName, path + @"\" + name + ".exe", true);
                            File.Delete(file.FullName);
                        }

                    }

                }
            }
            catch (Exception e)
            {
                Log.Error("[QBuild] ChangeCenterName Error : " + e.ToString());
                //return false;
            }
            return true;

        }


    }
}
