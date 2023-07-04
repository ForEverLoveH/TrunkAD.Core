using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace TrunkAD.Core.GameSystem.GameHelper 
{
    public class TextFileHelper
    {
         #region  初始化
        private static object logLock;

        private static TextFileHelper _instance;
        private TextFileHelper()
        {

        }
        public static TextFileHelper Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new TextFileHelper(); 
                    logLock = new object();
                }
                return _instance;
            }
        }
        #endregion
        /// <summary>
        /// 写入log
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="str"></param>
        public  void Write(string fileName,string str)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(fileName, false);
                streamWriter.Write(str);
                streamWriter.Close();
                streamWriter.Dispose();
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        public  void  SaveList(System.Windows.Forms.ListBox lst)
        {
            string st = "";
            foreach(string str in lst.Items)
            {
                st += str + "\r\n";

            }
            TextFileHelper.Instance.Write(lst.Name, st);

        }
        public System.Windows.Forms.ListBox LoadList(string fileName,ListBox ls)
        {
            string str = "";
            try{
                if (File.Exists(fileName))
                {
                    StreamReader streamReader = new StreamReader(fileName, false);
                    while (true)
                    {
                        str = streamReader.ReadLine();
                        if (!string.IsNullOrEmpty(str))
                        {
                             ls.Items.Add(str);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamReader.Close();
                }
            }
            catch(Exception ex)
            {
                LoggerHelper .Debug(ex);
            }
            return ls;
        }
        public  void    SaveInt(string fileName,int i)
        {
            string st = i + "";
            Write(fileName, st);

        }
        private  int  string2Int( string str)
        {
            int i = 0;
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            int.TryParse (str, out i);
            return i;
        }
        public  int  LoadInt(string filenAME)
        {
            string[] ST = Read(filenAME);
            if ( ST!=null   )
            {
                if( ST.Length > 0)
                {
                    return string2Int(ST[0]);
                }
            }
            return 0;
        }
        public string load1LineString(string filename)
        {
            string[] strg = Read(filename);
            if (null != strg)
            {
                if (strg.Length > 0)
                {
                    return strg[0];
                }
            }
            return "";
        }
        public string loadString(string filename)
        {
            ArrayList list = new ArrayList();
            string str = "";
            try
            {
                if (File.Exists(filename))
                {
                    StreamReader sr = new StreamReader(filename, false);
                    while (true)
                    {
                        str = sr.ReadLine();
                        if (null != str)
                        {
                            list.Add(str);
                        }
                        else
                        {
                            break;
                        }
                    }

                    sr.Close();
                }
            }
            catch (Exception) { }
            if (list.Count == 0)
                list.Add("");
            string str1 = "";
            for (int i = 0; i < list.Count; i++)
            {
                str1 += list[i];
                if (i < (list.Count - 1))
                    str1 += "\r\n";
            }
            return str1;// list.ToString();
            // return (string[])list.ToArray(typeof(string));
        }


        public string[] Read(string filename)
        {
            ArrayList list = new ArrayList();
            string str = "";
            try
            {
                if (File.Exists(filename))
                {
                    StreamReader sr = new StreamReader(filename, false);
                    while (true)
                    {
                        str = sr.ReadLine();
                        if (null != str)
                        {
                            list.Add(str);
                        }
                        else
                        {
                            break;
                        }
                    }

                    sr.Close();
                }
            }
            catch (Exception) { }
            if (list.Count == 0)
                list.Add("");
            return (string[])list.ToArray(typeof(string));
        }

    }
}