using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace QControlManagerNS
{
    static class Program
    {
        private static Mutex ms_SingltonMutex;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomain_AssemblyResolve;

            var createNew = true;
            ms_SingltonMutex = new Mutex(true, "QControlManager-5FBA3B3E-A79C-444A-8F2F-D80AF4038DCA", out createNew);

            if (createNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            else
            {
                MessageBox.Show("程序只能运行一个实例！", "奇境森林远程管理端", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        private static Assembly OnCurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? 
                args.Name.Substring(0, args.Name.IndexOf(',')) : 
                args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources"))
                return null;
            var rm = new ResourceManager(
                "QControlManagerNS.Properties.Resources", 
                Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return Assembly.Load(bytes);
        }
    }
}
