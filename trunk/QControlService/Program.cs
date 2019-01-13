using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QControlService
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
            var createNew = true;
            ms_SingltonMutex = new Mutex(true, "QControlService-5FBA3B3E-A79C-444A-8F2F-D80AF4038DCA", out createNew);

            if (createNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            else
            {
                MessageBox.Show("程序只能运行一个实例！", "奇境森林远程服务端", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }
    }
}
