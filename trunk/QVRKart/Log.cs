using System;
using System.IO;

public class Log
{
    static FileStream ostrm;
    static StreamWriter writer;
    static string file = "QLog-";
    public static void SetLogToFile()
    {
        try
        {
            string filename = file + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            string path = Path.GetFullPath(".\\Log\\");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if(writer != null)
            {
                Console.SetOut(TextWriter.Null);
                writer.Close();
            }

            if(ostrm != null)
            {
                ostrm.Close();
            }

            ostrm = new FileStream(".\\Log\\" + filename, FileMode.Append, FileAccess.Write);
            writer = new StreamWriter(ostrm);
            writer.AutoFlush = true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[QServerBase] Cannot open QLog.txt for writing.");
            Console.WriteLine(e.Message);
            return;
        }
        Console.SetOut(writer);
    }

    public static void SetLogFile(string name)
    {
        file = name;
    }

    internal static void WriteLog(string level, string content)
    {
        Console.WriteLine("[{0}] {1} {2}", level, DateTime.Now.ToString("hh.mm.ss"), content);
    }

    public static void Warning(string content)
    {
        WriteLog("Warning", content);
    }

    public static void Debug(string content)
    {
        WriteLog("Debug", content);
    }

    public static void Error(string content)
    {
        WriteLog("Error", content);
    }
}