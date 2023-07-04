using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameHelper 
{
    public class SQLiteHelper
    {
        public static Dictionary<string, SQLiteHelper> DataBaseSqLiteHelpers = new Dictionary<string, SQLiteHelper>();
        private SQLiteConnection _sqLiteConnection;
        private string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNamePath">数据文件路径</param>
        public SQLiteHelper(string fileNamePath = null)
        {
            if (string.IsNullOrEmpty(fileNamePath))
            {
                fileNamePath = @"./db/RunSportsDB.db";
            }

            DataSource = fileNamePath;
            GetConnectionSQLite();
        }
        /// <summary>
        /// 连接数据库
        /// </summary>
        private void GetConnectionSQLite()
        {
            try
            {
                string connStr = string.Format("Data Source={0};Version=3;Max Pool Size=10;Journal Mode=Off;",
                    DataSource);
                _sqLiteConnection = new SQLiteConnection(connStr);
            }
            catch (Exception exception)
            {
                LoggerHelper.Debug(exception);
                return;
            }
        }
        /// <summary>
        /// 创建数据库文件
        /// </summary>
        public void CreateDataBase()
        {
            try
            {
                string path = Path.GetDirectoryName(DataSource);
                if ((!string.IsNullOrWhiteSpace(path)) && (!Directory.Exists(path))) Directory.CreateDirectory(path);
                if (!File.Exists(DataSource)) SQLiteConnection.CreateFile(DataSource);
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return;
            }

        }
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public SQLiteTransaction BeginTransaction()
        {
            SQLiteTransaction transaction = null;
            try
            {
                transaction = _sqLiteConnection.BeginTransaction();
            }
            catch (Exception ec)
            {
                LoggerHelper.Debug(ec);
                transaction = null;
            }
            return transaction;
        }
        /// <summary>
        ///提交事务
        /// </summary>
        /// <param name="sQLiteTransaction"></param>
        public void CommitTransaction(ref SQLiteTransaction sQLiteTransaction)
        {
            try
            {
                if (sQLiteTransaction != null)
                {
                    sQLiteTransaction.Commit();
                    sQLiteTransaction = null;
                }

            }
            catch (Exception exception)
            {
                sQLiteTransaction = null;
                LoggerHelper.Debug(exception);
            }
        }
        /// <summary>
        /// 准备操作命令参数
        /// </summary>
        /// <param name="cmd">SQLiteCommand</param>
        /// <param name="conn">SQLiteConnection</param>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string cmdText, Dictionary<String, String> data)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Parameters.Clear();
                cmd.Connection = conn;
                cmd.CommandText = cmdText;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 30;
                if (data != null && data.Count >= 1)
                {
                    foreach (KeyValuePair<String, String> val in data)
                    {
                        cmd.Parameters.AddWithValue(val.Key, val.Value);
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.Debug(exception);
            }
        }
        /// <summary>
        /// 查询，返回DataSet
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataset(string cmdText, Dictionary<string, string> data = null)
        {
            try
            {
                DataSet ds = new DataSet();
                SQLiteCommand command = new SQLiteCommand();
                PrepareCommand(command, _sqLiteConnection, cmdText, data);
                var da = new SQLiteDataAdapter(command);
                da.Fill(ds);
                return ds;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

        }
        /// <summary>
        /// 查询，返回DataTable
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteDataTable(string cmdText, Dictionary<string, string> data = null)
        {
            try
            {
                DataTable dt = new DataTable();
                SQLiteCommand command = new SQLiteCommand();
                PrepareCommand(command, _sqLiteConnection, cmdText, data);
                SQLiteDataReader reader = command.ExecuteReader();
                dt.Load(reader);
                return dt;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

        }
        // <summary>
        /// 返回一行数据
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataRow</returns>
        public DataRow ExecuteDataRow(string cmdText, Dictionary<string, string> data = null)
        {
            try
            {
                DataSet ds = ExecuteDataset(cmdText, data);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    return ds.Tables[0].Rows[0];
                return null;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string cmdText, Dictionary<string, string> data = null)
        {
            try
            {
                File.AppendAllText(@"./Log/db/" + DateTime.Now.ToString("yyyy-MM-dd-HH") + ".log", $"数据库操作:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                File.AppendAllText(@"./Log/db/" + DateTime.Now.ToString("yyyy-MM-dd-HH") + ".log", cmdText + "\n");
                int result = 0;
                var command = new SQLiteCommand();
                PrepareCommand(command, _sqLiteConnection, cmdText, data);
                result = command.ExecuteNonQuery();

                return result;

            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return -1;
            }

        }

        /// <summary>
        /// 返回SqlDataReader对象
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>SQLiteDataReader</returns>
        public SQLiteDataReader ExecuteReader(string cmdText, Dictionary<string, string> data = null)
        {
            var command = new SQLiteCommand();

            try
            {
                PrepareCommand(command, _sqLiteConnection, cmdText, data);
                SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
            catch (Exception e)
            {
                command.Dispose();
                LoggerHelper.Debug(e);
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> ExecuteReaderList(string cmdText)
        {
            try
            {
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                var ds = ExecuteReader(cmdText);
                int columcount = ds.FieldCount;
                while (ds.Read())
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    for (int i = 0; i < columcount; i++)
                    {
                        object obj = ds.GetValue(i);
                        if (obj == null)
                        {
                            dic.Add(ds.GetName(i), "");
                        }
                        else
                        {
                            dic.Add(ds.GetName(i), obj.ToString());
                        }
                    }
                    list.Add(dic);
                }

                return list;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public Dictionary<string, string> ExecuteReaderOne(string cmdText)
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var ds = ExecuteReader(cmdText);
                int columcount = ds.FieldCount;
                while (ds.Read())
                {
                    for (int i = 0; i < columcount; i++)
                    {
                        object obj = ds.GetValue(i);
                        if (obj == null)
                        {
                            dic.Add(ds.GetName(i), "");
                        }
                        else
                        {
                            dic.Add(ds.GetName(i), obj.ToString());
                        }
                    }
                    break;
                }

                return dic;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

        }


        /// <summary>
        /// 返回结果集中的第一行第一列，忽略其他行或列
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>object</returns>
        public object ExecuteScalar(string cmdText, Dictionary<string, string> data = null)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand(cmd, _sqLiteConnection, cmdText, data);
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="recordCount">总记录数</param>
        /// <param name="pageIndex">页牵引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="countText">查询总记录数的Sql文本</param>
        /// <param name="data">命令参数</param>
        /// <returns>DataSet</returns>
        public DataSet ExecutePager(ref int recordCount, int pageIndex, int pageSize, string cmdText, string countText, Dictionary<string, string> data = null)
        {
            try
            {
                if (recordCount < 0)
                    recordCount = int.Parse(ExecuteScalar(countText, data).ToString());
                DataSet ds = new DataSet();
                SQLiteCommand command = new SQLiteCommand();
                PrepareCommand(command, _sqLiteConnection, cmdText, data);
                var da = new SQLiteDataAdapter(command);
                da.Fill(ds, (pageIndex - 1) * pageSize, pageSize, "result");
                return ds;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

        }

        /// <summary>
        /// 重新组织数据库：VACUUM 将会从头重新组织数据库
        /// </summary>
        public void ResetDataBass(SQLiteConnection conn)
        {
            try
            {
                SQLiteCommand cmd = new SQLiteCommand();
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Parameters.Clear();
                cmd.Connection = conn;
                cmd.CommandText = "vacuum";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 30;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return;
            }

        }
        /// <summary>
        /// 关闭数据库的连接
        /// </summary>
        public void CloseDataBaseConnection()
        {
            if (_sqLiteConnection.State == ConnectionState.Open)
                _sqLiteConnection.Close();
            _sqLiteConnection = null;
        }

        /// <summary>
        /// 初始化清空数据库
        /// </summary>
        public bool InitDataBase()
        {
            try
            {
                var dss = ExecuteReaderList("SELECT name,seq FROM sqlite_sequence");
                var transaction = BeginTransaction();
                foreach (var ds in dss)
                {
                    string tableName = ds["name"];

                    if (tableName == "localInfos")
                    {
                        continue;
                    }
                    ExecuteNonQuery($"DELETE FROM {tableName}");
                    ExecuteNonQuery($"UPDATE sqlite_sequence SET seq=0 where name='{tableName}'");
                }
                CommitTransaction(ref transaction);
                return true;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return false;
            }

        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        public bool BackDataBase()
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(DataSource);
                File.Copy(DataSource, $"./db/backup/{fileNameWithoutExtension}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.db");
                return true;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return false;
            }

        }
    }
}

