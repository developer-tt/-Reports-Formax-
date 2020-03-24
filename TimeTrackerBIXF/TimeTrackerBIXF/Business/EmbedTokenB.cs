using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTrackerBIXF.Data.Models;

namespace TimeTrackerBIXF.Business
{
    public class EmbedTokenB
    {
        static object locker = new object();

        public SQLiteConnection database;


        public EmbedTokenB(SQLiteConnection conn)
        {
            database = conn;
            database.CreateTable<EmbedTokenDTO>();
        }


        public int Create(EmbedTokenDTO user)
        {
            lock (locker)
            {
                return database.Insert(user);
            }
        }


        public int Update(EmbedTokenDTO user)
        {
            lock (locker)
            {
                return database.Update(user);
            }
        }


        public int DeleteAll()
        {
            lock (locker)
            {
                return database.DeleteAll<EmbedTokenDTO>();
            }
        }


        public EmbedTokenDTO Get()
        {
            lock (locker)
            {
                return database.Table<EmbedTokenDTO>().FirstOrDefault();
            }
        }


        public EmbedTokenDTO GetByGroupIDAndReportID(string GroupId, string ReportId)
        {
            lock (locker)
            {
                return database.Table<EmbedTokenDTO>().Where(x => x.GroupID.Equals(GroupId) && x.Id.Equals(ReportId)).ToList().Where(y => y.MinutesToExpiration > 0).FirstOrDefault();
            }
        }
    }
}
