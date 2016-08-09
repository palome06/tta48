using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Trench.Log
{
    // Client log
    public class ClLog
    {
        private string rName, lName;
        //private Queue<string> rq, lq;
        private BlockingCollection<string> rq, lq;
        //private List<string> recentList;

        // record literature results 
        private bool record;
        // record log in code
        private bool msglog;
        // whether the writing to Log stops or not
        public bool Stop { set; get; }

        public void Start(int playerId, bool record, bool msglog, int nouse)
        {
            rq = new BlockingCollection<string>(new ConcurrentQueue<string>());
            lq = new BlockingCollection<string>(new ConcurrentQueue<string>());
            //rq = new Queue<string>();
            //lq = new Queue<string>();
            this.record = record; this.msglog = msglog;
            Stop = false;

            DateTime dt = System.DateTime.Now;
            if (record)
            {
                if (!Directory.Exists("./rec"))
                    Directory.CreateDirectory("./rec");
                // TODO: reset the log title name
                rName = string.Format("./rec/逍遥游游戏记录{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}({6}).txt",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, playerId);
                Task.Factory.StartNew(() =>
                {
                    using (StreamWriter sw = new StreamWriter(rName, true))
                    {
                        while (!Stop)
                        {
                            string line = rq.Take();
                            if (!string.IsNullOrEmpty(line))
                            {
                                sw.WriteLine(line);
                                sw.Flush();
                            }
                        }
                    }
                });
            }
            if (msglog)
            {
                if (!Directory.Exists("./log"))
                    Directory.CreateDirectory("./log");
                lName = string.Format("./log/psd{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}({6}).psg",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, playerId);
                var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                int version = ass.Version.Revision;

                Task.Factory.StartNew(() =>
                {
                    using (StreamWriter sw = new StreamWriter(lName, true))
                    {
                        sw.WriteLine("VERSION={0} UID={1}", version, playerId);
                        sw.Flush();
                        while (!Stop)
                        {
                            string line = lq.Take();
                            if (!string.IsNullOrEmpty(line))
                            {
                                sw.WriteLine(LogES.DESEncrypt(line, "AKB48Show!",
                                    (version * version).ToString()));
                                //char[] chs = line.ToCharArray();
                                //sw.Write(chs.Length);
                                //sw.Write(chs);
                                sw.Flush();
                            }
                        }
                    }
                });
            }
        }

        public void Logg(string line) { if (msglog) lq.Add(line); }

        public void Record(string line) { if (record) rq.Add(line); }
    }
}
