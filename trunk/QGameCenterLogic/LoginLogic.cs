using System;
using System.Collections.Generic;
using QData;

namespace QGameCenterLogic
{
    /// <summary>
    /// 进入主界面之前的登陆逻辑
    /// </summary>
    public class LoginLogic
    {
        public static bool Check(string username, string password)
        {
            try
            {
               
                var gamecenterdata = GameCenterConfig.LoadData("Configs/GameCenterConfig.xml") as GameCenterConfig;
                if(gamecenterdata == null)
                {
                    throw new Exception("无法加载中控配置文件");
                }
                if (username == gamecenterdata.UserName && password == gamecenterdata.Password)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                Log.Error("[LoginLogic] Check Error : " + e.Message);
            }
            return false;
        }

    }
}
