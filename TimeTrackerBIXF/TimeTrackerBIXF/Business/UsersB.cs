using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using TimeTrackerBIXF.Data.Models;

namespace TimeTrackerBIXF.Business
{
    public class UsersB
    {
        static object locker = new object();

        public SQLiteConnection database;

        //This method receives the connection to the database and creates the table if necessary
        public UsersB(SQLiteConnection conn)
        {
            database = conn;
            database.CreateTable<tblUsersDTO>();
        }

        //Create a new record "User" on the database
        public int Create(tblUsersDTO user)
        {
            lock (locker)
            {
                return database.Insert(user);
            }
        }

        //Update the received record "User"  on the database
        public int Update(tblUsersDTO user)
        {
            lock (locker)
            {
                return database.Update(user);
            }
        }

        //Removes all the "user" entries on the database
        public int DeleteAll()
        {
            lock (locker)
            {
                return database.DeleteAll<tblUsersDTO>();
            }
        }

        //Returns the first "User" element of the database
        public tblUsersDTO Get()
        {
            lock (locker)
            {
                return database.Table<tblUsersDTO>().FirstOrDefault();
            }
        }

        //Returns the first "User" element of the database where the User ID equals to the parameter received
        public tblUsersDTO Get(int iUserID)
        {
            lock (locker)
            {
                return database.Table<tblUsersDTO>().FirstOrDefault(x => x.UserID == iUserID);
            }
        }
    }
}
