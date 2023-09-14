using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCapture.tools {
    class WriteLog {

        static string CurrentRootPath = Directory.GetCurrentDirectory();//获取当前根目录

        #region 写日志
        public static void WriteLogFile(string input) {
            ///指定日志文件的目录
            string fDirectory = CurrentRootPath + "\\log\\";
            string fname = fDirectory+DateTime.Now.ToString("yyyyMMdd")+".txt";
            DirectoryInfo dInfo = new DirectoryInfo(fDirectory);
            if(!dInfo.Exists) {
                dInfo.Create();
            }

            FileInfo finfo = new FileInfo(fname);
            if(!finfo.Exists) {
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.ReadWrite);
                //using (StreamWriter writer1 = new StreamWriter(_file))

                //FileStream fs = File.Create(fname);
                fs.Close();
                finfo = new FileInfo(fname);
            }



            /**/

            ///判断文件是否存在以及是否大于2K

            if(finfo.Length > 1024 * 1024 * 10) {

                /**/

                ///文件超过10MB则重命名

                File.Move(Directory.GetCurrentDirectory() + "\\LogFile.txt",Directory.GetCurrentDirectory() + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "LogFile.txt");

                /**/

                ///删除该文件

                //finfo.Delete();

            }

            //finfo.AppendText();

            /**/

            ///创建只写文件流


            //FileStream _file = new FileStream(fname,FileMode.Create, FileAccess.ReadWrite);
            FileStream _file = new FileStream(fname,FileMode.Append,FileAccess.Write);
            using(StreamWriter w = new StreamWriter(_file))
            // using (FileStream fs = finfo.OpenWrite())

            {

                /**/

                ///根据上面创建的文件流创建写数据流

                //StreamWriter w = new StreamWriter(fs);



                /**/

                ///设置写数据流的起始位置为文件流的末尾

                w.BaseStream.Seek(0,SeekOrigin.End);



                /**/

                ///写入“Log Entry : ”

                w.Write("\n\r ");



                /**/

                ///写入当前系统时间并换行

                w.Write("{0} {1} \n\r",DateTime.Now.ToLongDateString(),DateTime.Now.ToLongTimeString());

                ///写入日志内容并换行

                w.Write(input + "\n\r");
                ///清空缓冲区内容，并把缓冲区内容写入基础流

                w.Flush();

                ///关闭写数据流

                w.Close();

            }



        }

        public static bool WriteStr(string fullName,string input) {
            try {
                FileInfo finfo = new FileInfo(fullName);
                if(!finfo.Exists) {
                    FileStream fs = new FileStream(fullName,FileMode.Create,FileAccess.ReadWrite);
                    fs.Close();
                    finfo = new FileInfo(fullName);
                }

                FileStream _file = new FileStream(fullName,FileMode.OpenOrCreate,FileAccess.Write);
                using(StreamWriter w = new StreamWriter(_file)) {
                    w.Write(input + "\n\r");
                }
                return true;
            } catch(Exception e) {
                WriteLogFile(e.Message + ":" + input);
                return false;
            }
        }
        #endregion
    }
}
