using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace QConnection
{
    //同屏服务端，放在QClient程序里面
    public class QScreenServer
    {
        private Thread m_ThreadAccped = null;
        private Thread m_ThreadSend = null;

        private Socket m_SocketAccept = null;
        private Socket m_ClientSocket = null;

        private string m_ScreenGame;

        private Rectangle m_ScreenRect = Screen.PrimaryScreen.Bounds;
        private int m_Count = 0;

        private bool m_Running;
        private bool m_Sending;

        private int m_Frequency;
        private int m_Frequency_Copy;


        public string ScreenGame
        {
            get
            {
                return m_ScreenGame;
            }

            set
            {
                m_ScreenGame = value;
            }
        }

        public void Start(int port,int frequency)
        {
            /*string ip = Utils.GetLocalIP();
            
            if (string.IsNullOrEmpty(ip))
            {
                Log.Error("[QScreenServer] Can't Start Server With Empty IP.");
                return;
            }
            */
            Stop();

            try
            {
                m_Frequency_Copy = frequency;
                //m_Frequency = frequency;
                
                m_SocketAccept = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Any, port);
                m_SocketAccept.Bind(endPoint);
                m_SocketAccept.Listen(10);

                m_ThreadAccped = new Thread(new ThreadStart(OnAcceptSocket));
                m_ThreadAccped.Start();
                
            }
            catch (Exception e)
            {
                Log.Error("[QScreenServer] Start Server Failed:" + e.ToString());
            }

            Log.Debug("[QScreenServer] Server is Starting...");
        }

        private bool SendRawData(MemoryStream mem)
        {
            try
            {
                var dataSize = (int)mem.Length;
                if (dataSize > (1024 * 1024 * 4))
                {
                    Log.Error("[QScreenServer] SendRawData Size > 4M. " + dataSize.ToString());
                    return true;
                }

                var dataSizeBuffer = BitConverter.GetBytes(dataSize);
               
                var sendedSize = 0;

                //首先发送头部4字节表示缓冲区长度
                while (4 - sendedSize > 0)
                {
                    try
                    {
                        sendedSize += m_ClientSocket.Send(dataSizeBuffer, sendedSize, 4 - sendedSize, SocketFlags.None);
                    }
                    catch (SocketException)
                    {
                        //Log.Error("[QScreenServer] sendSize : " + sendedSize + "   Error : " + e.ToString() + "  ip : " + ((IPEndPoint)m_ClientSocket.RemoteEndPoint).Address);
                        //continue;
                        break;
                    }
                }

                sendedSize = 0;
                var raw = mem.GetBuffer();
                while (dataSize - sendedSize != 0)
                {
                    try
                    {          
                        var s = m_ClientSocket.Send(raw, sendedSize, dataSize - sendedSize, SocketFlags.None); ;
                        sendedSize += s;
                        if(s <= 0)
                        {
                            Log.Debug("s : " + s);
                        }
                    }
                    catch (SocketException)
                    {
                        //Log.Error("[QScreenServer] SendRawData2 Error : " + e);
                        break;
                    }
                }
            }
            catch (SocketException)
            {
                return false;
                
            }
            catch (Exception)
            {
                m_Count++;
                //Log.Error("[QScreenServer] ProcessNotGameStart Error:" + ex);
            }
            finally
            {
                mem.Close();
            }

            Thread.Sleep(m_Frequency);
         
            return true;
        }
                
        private bool ProcessNotGameStart(Bitmap screen, Graphics grp)
        {
            var memoryStream = new MemoryStream();

            try
            {
                grp.CopyFromScreen(0, 0, 0, 0, m_ScreenRect.Size);
                var image = screen.GetThumbnailImage(m_ScreenRect.Width / 1, m_ScreenRect.Height / 1, null, IntPtr.Zero);
                image.Save(memoryStream, ImageFormat.Png);
            }
            catch(Exception e)
            {
                Log.Error("[QScreenServer] Get Screen Failed, Error : " + e.Message);
                Thread.Sleep(1000);
                //Thread.Sleep(m_Frequency);
                return true;
            }

            return SendRawData(memoryStream);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left; //最左坐标
            public int Top; //最上坐标
            public int Right; //最右坐标
            public int Bottom; //最下坐标

            public override string ToString()
            {
                return String.Format("[{0},{1},{2},{3}]", Left, Top, Right, Bottom);
            }
        }

        private bool ProcessGameStarted()
        {
            var memoryStream = new MemoryStream();

            try
            {
                IntPtr game = new IntPtr(0);
                foreach (var p in Process.GetProcesses())
                {
                    ////考虑到虚幻游戏
                    //if (p.ProcessName.Contains("Shipping"))
                    //{
                    //    game = p.MainWindowHandle;
                    //    break;
                    //}

                    if (p.ProcessName.Contains(ScreenGame))
                    {
                        game = p.MainWindowHandle;
                        break;
                    }
                }

                if (game == (IntPtr)0)
                {
                    Thread.Sleep(33);
                    return true;
                }

                var rect = new RECT();
                GetWindowRect(game, ref rect);
                //Log.Debug(rect.ToString());
                var screen = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top, PixelFormat.Format16bppRgb565);
                var grp = Graphics.FromImage(screen);

                grp.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(rect.Right - rect.Left, rect.Bottom - rect.Top));

                var image = screen.GetThumbnailImage(m_ScreenRect.Width, m_ScreenRect.Height, null, IntPtr.Zero);
                image.Save(memoryStream, ImageFormat.Jpeg);

                screen.Dispose();
                grp.Dispose();

            }
            catch
            {
                Log.Error("[QScreenServer] Get Game Screen Failed:" + ScreenGame);
                return true;
            }

            return SendRawData(memoryStream);
        }

        private void OnSendScreenData()
        {
            m_Sending = true;

            var screen = new Bitmap(m_ScreenRect.Width, m_ScreenRect.Height, PixelFormat.Format16bppRgb565);
            var grp = Graphics.FromImage(screen);

            while (m_Sending)
            { 
                if (m_ClientSocket != null && !m_ClientSocket.Connected)
                {
                    break ;
                }
               
                if (/*IsGameStart &&*/ Process.GetProcessesByName(ScreenGame).Length > 0)
                {
                     m_Frequency = m_Frequency_Copy;
                     m_Sending = ProcessGameStarted();
                }
                else
                {
                    //Log.Debug("游戏没有开始的同屏");
                    m_Frequency = 1000;
                    m_Sending = ProcessNotGameStart(screen, grp);
                }
            }

            screen.Dispose();
            grp.Dispose();

            //Log.Debug("[QScreenServer] End Sending.");
        }

        private void OnAcceptSocket()
        {
            m_Count = 0;
            m_Running = true;

            while (m_Running)
            {
                Socket socket = null;
                try
                {
                    socket = m_SocketAccept.Accept();
                }
                catch (Exception e)
                {
                    if (m_Running)
                    {
                        Log.Error("[QScreenServer] AcceptSocket Error:" + e.Message);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                StopSending();

                m_ClientSocket = socket;
                m_ClientSocket.SendBufferSize = 1024 * 1024;
                m_ClientSocket.ReceiveBufferSize = 1024 * 1024;
                m_Frequency = 1000;
                m_ThreadSend = new Thread(OnSendScreenData);
                m_ThreadSend.Start();
            }

            Log.Debug("[QScreenServer] Server Stop.");
        }

        private void StopSending()
        {
            m_Sending = false;

            //Log.Debug("[QScreenServer] StopSending : Stop Sending . ");

            if (m_ThreadSend != null && m_ThreadSend.IsAlive)
            {
                try
                {
                    m_ThreadSend.Abort();
                }
                catch { }
            }

            if (m_ClientSocket != null && m_ClientSocket.Connected)
            {
                try
                {
                    m_ClientSocket.Close();
                }
                catch { }
            }
        }

        public void Stop()
        {
            try
            {
                StopSending();
                m_Running = false;

                if (m_SocketAccept != null)
                {
                    m_SocketAccept.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error("[QScreenServer] Stop Error " + e.Message);
            }
            finally
            {
                m_ClientSocket = null;
                m_ThreadSend = null;
                m_ThreadAccped = null;
                m_SocketAccept = null;
            }
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        private static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);
    }
}
