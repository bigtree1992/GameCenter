using System;
using System.Collections.Generic;
using System.Linq;

namespace QData
{
    public partial class GameCenterDBEntities
    {
        public Action OnOperationScuess;

        public void PrintGameRecordInfos()
        {
            Log.Debug("数据有 ： ");
            foreach (var gr in GameRecords)
            {
                Log.Debug("Id : " + gr.Id + "  Name : " + gr.Name + " Count : " + gr.Count + " Amount : " + gr.Amount + "  RunTime : " + gr.RunTime);
            }
        }

        public void AddAGameReecordInfo(GameRecord info)
        {
            try
            {

                info.Id = GameRecords.Count() + 1;
                GameRecords.Add(info);

                this.SaveChangesAsync();

                if (OnOperationScuess != null)
                {
                    OnOperationScuess();
                }
            }
            catch (Exception e)
            {
                Log.Error("[GameCenterDBEntities] AddAGameReecordInfo Error : " + e.Message);
            }

        }

        public void AddAGameReecordInfo(List<GameRecord> infos)
        {
            try
            {
                var count = GameRecords.Count();
                for (var i = 0; i < infos.Count; i++)
                {
                    infos[i].Id = count + i + 1;
                }
                GameRecords.AddRange(infos);
                this.SaveChangesAsync();

                if (OnOperationScuess != null)
                {
                    OnOperationScuess();
                }
            }
            catch (Exception e)
            {
                Log.Error("[GameCenterDBEntities] AddAGameReecordInfo Error : " + e.Message);
            }

        }

        public List<GameRecord> CheckGameRecordInfo(DateTime t1, DateTime t2, string gameName)
        {
            
            t2 = t2.AddHours(23.9);

            try
            {
                var query = new List<GameRecord>();
                if (string.IsNullOrEmpty(gameName))
                {
                    var query1 = from gr in GameRecords
                                 where (gr.RunTime.CompareTo(t1.Date) >= 0 && gr.RunTime.CompareTo(t2.Date) <= 0)
                                 select gr;
                    query = query1.ToList();
                }
                else
                {
                    var query1 = from gr in GameRecords
                                 where (gr.RunTime.CompareTo(t1) >= 0 && gr.RunTime.CompareTo(t2) <= 0) && (gr.Name == gameName)
                                 select gr;
                    query = query1.ToList();
                }

                if (OnOperationScuess != null)
                {
                    OnOperationScuess();
                }
                return query;
            }
            catch (Exception e)
            {
                Log.Error("[GameCenterDBEntities] CheckGameRecordInfo Error : " + e.Message);
                return null;
            }

        }

        public void DeleteAllGameRecord()
        {
            try
            {
                //GameRecords.
                //GameRecords.ToList().Clear();
                GameRecords.RemoveRange(GameRecords);
                this.SaveChangesAsync();
                if (OnOperationScuess != null)
                {
                    OnOperationScuess();
                }
            }
            catch (Exception e)
            {
                Log.Error("[GameCenterDBEntities] DeleteAllGameRecord Error : " + e.Message);
            }
        }
    }


}
