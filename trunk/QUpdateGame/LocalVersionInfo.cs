using QConnection;
using System.Collections.Generic;
using System.IO;

namespace QUpdateGame
{
    public class LocalVersionInfo
    {
        private List<QProtocols.FileInfo> m_ClientFilesVersionInfo;
        private List<QProtocols.FileInfo> m_OtherFilesVersionInfo;

        public LocalVersionInfo()
        {}

        public string ToLocalPath(string path)
        {
            return $"ClientFiles\\{path}";
        }

        public string ToClientPath(string path , bool withExt)
        {
            var result = path.Replace("ClientFiles\\", "");
            if (!withExt)
            {
                result = result.Replace(".zip", "");
            }
            
            return result;
        }

        public void Load(string zipPath, List<string> checkPaths)
        {
            var ignorePa = new List<string> { "Log" };
            ignorePa.AddRange(checkPaths);

            if (File.Exists(zipPath))
            {
                m_ClientFilesVersionInfo = Utils.GetFileInfoFromZip(zipPath, ignorePa);
            }
            else
            {
                m_ClientFilesVersionInfo = new List<QProtocols.FileInfo>();
            }

            m_OtherFilesVersionInfo = new List<QProtocols.FileInfo>();
            foreach (var path in checkPaths)
            {
                var p = ToLocalPath(path);
                if (Directory.Exists(p))
                {
                    var infos = Utils.GetVersionInfoFromZip(p);                    
                    m_OtherFilesVersionInfo.AddRange(infos);
                }
            }
        }

        public bool CheckClientFileChange(List<QProtocols.FileInfo> infos)
        {
            if(m_ClientFilesVersionInfo == null )
            {
                return false;
            }

            if (infos == null)
            {
                return true;
            }

            for(int i = 0; i < m_ClientFilesVersionInfo.Count; i++)
            {
                var target = m_ClientFilesVersionInfo[i];
                var find = infos.Find((item) => item.Path == target.Path);
                if (find == null)
                {
                    return true;
                }
                
                if(find.Info != target.Info)
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetNeedUpdatePath(List<QProtocols.FileInfo> infos, bool forceUpdate)
        {
            var result = new List<string>();
            if(infos == null)
            {
                return result;
            }

            for (int i = 0; i < m_OtherFilesVersionInfo.Count; i++)
            {
                var target = m_OtherFilesVersionInfo[i];
                string target_path = ToClientPath(target.Path,false);        
                if (forceUpdate)
                {
                    //对比的时候使用的是没有拓展名的，现在需要加回来
                    result.Add(target.Path + ".zip");
                }
                else
                {                
                    var find = infos.Find((item) => item.Path == target_path);
                    if (find == null ||
                        find.Info != target.Info)
                    {                      
                        result.Add(target.Path + ".zip");
                    }                    
                }
            }
            return result;
        }

        public bool CheckClientVersion(string left, string right)
        {
            left = left.Replace("QClient-", "");
            left = left.Replace(".zip", "");

            right = right.Replace("QClient-", "");
            right = right.Replace(".zip", "");

            try
            {
                float l = float.Parse(left);
                float r = float.Parse(right);

                if(l > r)
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }
    }
}
