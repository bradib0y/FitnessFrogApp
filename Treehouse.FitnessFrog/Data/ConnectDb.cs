using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Data
{
    /// <summary>
    /// Any object of this class is be capable of connecting to the database.
    /// 
    /// The various functions can: 
    /// -create a database and fill with mock data, 
    /// -drop database and delete mock data, 
    /// -get the list of entries, 
    /// -and CRUD individual entries.
    /// </summary>
    public class ConnectDb
    {

        //public List<Activity> Activities { get; set; }

        ///// <summary>
        ///// The collection of entries.
        ///// </summary>
        //public List<Entry> Entries { get; set; }


        /// <summary>
        /// Reading all entries from the database.
        /// </summary>
        public List<Entry> GetListForIndex()
        {
            List<Entry> received = new List<Entry>();

            using (System.Data.SQLite.SQLiteConnection connectDb = new System.Data.SQLite.SQLiteConnection("Data Source=C:\\sqlite\\something.db"))
            {
                using (System.Data.SQLite.SQLiteCommand cmdGetList = new System.Data.SQLite.SQLiteCommand(connectDb))
                {
                    // 1. open connection
                    connectDb.Open();

                    // 2. set select command
                    try
                    {
                        cmdGetList.CommandText = "select * from entries;";

                        // 3. using SQLiteDataReader                    
                        using (System.Data.SQLite.SQLiteDataReader readData = cmdGetList.ExecuteReader())
                        {
                            while (readData.Read())
                            {
                                Entry e = new Entry();
                                e.Id = Convert.ToInt32(readData[0]);
                                e.Date = Convert.ToDateTime(readData[1]);
                                e.ActivityId = Convert.ToInt32(readData[2]);
                                e.Duration = Convert.ToDouble(readData[3]);
                                switch (Convert.ToInt32(readData[4]))
                                {
                                    case 1:
                                        e.Intensity = Entry.IntensityLevel.Low;
                                        break;
                                    case 2:
                                        e.Intensity = Entry.IntensityLevel.Medium;
                                        break;
                                    case 3:
                                        e.Intensity = Entry.IntensityLevel.High;
                                        break;
                                }
                                try
                                {
                                    e.Exclude = Convert.ToBoolean(readData[5]);
                                }
                                catch (Exception ex)
                                {
                                    e.Exclude = false;
                                }
                                try
                                {
                                    e.Notes = Convert.ToString(readData[6]);
                                }
                                catch (Exception ex)
                                {
                                    e.Notes = "";
                                }

                                received.Add(e);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // file not found so we return the empty list
                    }
                    // 4. close connection
                    connectDb.Close();
                }
            }

            return received;
        }

        /// <summary>
        /// Create database and fill with mock data. 
        /// 
        /// If it exists, it will overwrite.
        /// </summary>
        public bool CreateDb()
        {
            // CreateFile either creates a file or overwrites it with an empty one if existent and deletes everything
            System.Data.SQLite.SQLiteConnection.CreateFile(@"C:\sqlite\something.db");

            using (System.Data.SQLite.SQLiteConnection connectDb = new System.Data.SQLite.SQLiteConnection("Data Source=C:\\sqlite\\something.db"))
            {
                using (System.Data.SQLite.SQLiteCommand cmdCreate = new System.Data.SQLite.SQLiteCommand(connectDb))
                {
                    // 1. open connection
                    connectDb.Open();

                    // 2. execute table creation commands
                    cmdCreate.CommandText = "create table if not exists activities(" +
                                "id integer primary key autoincrement," +
                                "activity text not null);";
                    cmdCreate.ExecuteNonQuery();
                    cmdCreate.CommandText = "create table if not exists intensities(" +
                                "id integer primary key autoincrement," +
                                "intensity text not null);";
                    cmdCreate.ExecuteNonQuery();
                    cmdCreate.CommandText = "create table if not exists entries(" +
                                "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                "date text not null, " +
                                "activityid integer not null references activities(id), " +
                                "duration real not null, " +
                                "intensityid integer not null references intensities(id), " +
                                "exclude boolean, " +
                                "notes text);";
                    cmdCreate.ExecuteNonQuery();

                    // 3. execute data insertion commands into the already existing tables
                        // 3.1 intensity levels
                        cmdCreate.CommandText = "insert into intensities (" +
                                    "id, intensity) values (" +
                                    "1, 'Low');";
                        cmdCreate.ExecuteNonQuery();
                        cmdCreate.CommandText = "insert into intensities (" +
                                    "id, intensity) values (" +
                                    "2, 'Medium');";
                        cmdCreate.ExecuteNonQuery();
                        cmdCreate.CommandText = "insert into intensities (" +
                                    "id, intensity) values (" +
                                    "3, 'High');";
                        cmdCreate.ExecuteNonQuery();

                        // 3.2 activities
                        foreach (Activity a in Data.Activities)
                        {
                            cmdCreate.CommandText = "insert into activities (" +
                                        "id, activity) values (" +
                                        a.Id.ToString() + ", '" +
                                        a.Name + "');";
                            cmdCreate.ExecuteNonQuery();
                        }

                        // 3.3 entries
                        foreach (Entry e in Data.Entries)
                        {
                            cmdCreate.CommandText = "insert into entries (" +
                                        "id, date, activityid, duration, intensityid, exclude, notes) values (" +
                                        e.Id.ToString() + ", '" +
                                        e.Date.ToString() + "', " +
                                        e.ActivityId.ToString() + ", " +
                                        e.Duration.ToString() + ", " +
                                        "2, null, null);";
                            cmdCreate.ExecuteNonQuery();
                        }

                    // 4. closing connection
                    connectDb.Close();
                }
                return true; // if success
            }
        }
    }
}