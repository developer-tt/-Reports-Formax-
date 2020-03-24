using SQLite;

namespace TimeTrackerBIXF.Interfaces
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
