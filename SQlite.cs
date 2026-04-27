using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace 馬達邏輯加上sqlite
{
    public class MotorLogEntry
    {
        public int Id { get; set; } // 資料庫自動生成的識別碼
        public string MotorName { get; set; }
        public DateTime LogTime { get; set; }
        public double CurrentPos { get; set; }
        public double NowSpeed { get; set; }
        public double TargetPos { get; set; }
        public string MotorState { get; set; }
        public string EventMsg { get; set; }
    }

    public class SQLiteManager
    {
        private readonly string _connectionString;

        public SQLiteManager(string fileName = "motor_records.db")
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(baseDir, fileName);
            _connectionString = $"Data Source={dbPath}";
        }

        public void InitDatabase()// 初始化資料庫和表格
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS MotorLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MotorName TEXT,
                    LogTime TEXT,
                    CurrentPos REAL,
                    NowSpeed REAL,
                    TargetPos REAL,
                    MotorState TEXT,
                    EventMsg TEXT
                )";
                conn.Execute(sql);
            }
        }

        // 寫入資料並回傳產生的 ID
        public int InsertLog(MotorLogEntry log)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO MotorLogs 
                    (MotorName, LogTime, CurrentPos, NowSpeed, TargetPos, MotorState, EventMsg) 
                    VALUES (@MotorName, @LogTime, @CurrentPos, @NowSpeed, @TargetPos, @MotorState, @EventMsg);
                    SELECT last_insert_rowid();"; // 獲取本次寫入產生的 ID

                return conn.ExecuteScalar<int>(sql, new
                {
                    log.MotorName,
                    LogTime = log.LogTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    log.CurrentPos,
                    log.NowSpeed,
                    log.TargetPos,
                    log.MotorState,
                    log.EventMsg
                });//
            }
        }
        public MotorLogEntry GetLatestData()// 新增方法：獲取最新一筆紀錄
        {
            using (var conn = new SqliteConnection(_connectionString)) // 修正類別名稱
            {
                string sql = "SELECT * FROM MotorLogs ORDER BY Id DESC LIMIT 1";
                return conn.QueryFirstOrDefault<MotorLogEntry>(sql);// 使用 QueryFirstOrDefault 以防止資料庫中沒有紀錄時拋出例外
            }
        }
        public List<MotorLogEntry> GetRecentAlarms()// 新增方法：獲取最近 10 筆紀錄
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                // 移除 WHERE 條件，改為抓取最後 10 筆不分型態的紀錄
                string sql = "SELECT * FROM MotorLogs ORDER BY Id DESC LIMIT 10";
                return conn.Query<MotorLogEntry>(sql).ToList();// 使用 Query 方法獲取多筆紀錄並轉換為 List
            }
        }
    }
}