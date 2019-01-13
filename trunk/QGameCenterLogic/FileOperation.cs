using System;
using System.Collections.Generic;
using System.IO;


namespace QGameCenterLogic
{
    public class FileOperation
    {
 
        /// <summary>
        /// 移动文件夹到指定的位置
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        public static void CopyDirFile(string sourcePath, string destPath, bool isDeleSourpath = false)
        {
            if (Directory.Exists(sourcePath) == false)
            {
                Log.Error("[FileOperation] CopyDirFile Error : 源文件夹不存在 : " + sourcePath);
                throw new Exception("[FileOperation] CopyDirFile Error : 源文件夹不存在.");
            }
     
            if (!Directory.Exists(destPath))
            {
                //目标目录不存在则创建  
                try
                {
                    Directory.CreateDirectory(destPath);
                }
                catch (Exception ex)
                {
                    throw new Exception("创建目标目录失败：" + ex.Message);
                }
            }

            //获得源文件下所有文件  
            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            files.ForEach(c =>
            {
                string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                //覆盖模式  
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }
                File.Copy(c, destFile);
            });
            //获得源文件下所有目录文件  
            List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));

            folders.ForEach(c =>
            {
                string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                //Directory.Move必须要在同一个根目录下移动才有效，不能在不同卷中移动。  
                //Directory.Move(c, destDir);  
                //采用递归的方法实现  
                CopyDirFile(c, destDir, isDeleSourpath);
            });

            if (isDeleSourpath == false)
            {
                return;
            }

            try
            {
                //File.Delete(deletPath);
                Directory.Delete(sourcePath,true);
            }
            catch (Exception ex)
            {
                throw new Exception("删除源文件夹失败：" + ex.Message+",文件夹:"+ sourcePath);
            }

        }

    }
}
