using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Trench.Log
{
    public class SvLog
    {
        private string fileName;
        private string gameName;

        private BlockingCollection<string> queue;

        public SvLog(string gameName) { this.gameName = gameName; }
        // whether the writing to Log stops or not
        public bool Stop { set; get; }

        public void Start()
        {
            DateTime dt = System.DateTime.Now;
            bool exists = Directory.Exists("./log");
            if (!exists)
                Directory.CreateDirectory("./log");
            fileName = string.Format("./log/{0}{1:D4}{2:D2}{3:D2}-{4:D2}{5:D2}{6:D2}.log",
                gameName, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            int version = ass.Version.Revision;

            queue = new BlockingCollection<string>(new ConcurrentQueue<string>());
            Task.Factory.StartNew(() =>
            {
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    sw.WriteLine("VERSION={0} ISSV=1", version);
                    sw.Flush();
                    Stop = false;
                    while (!Stop)
                    {
                        string line = queue.Take();
                        if (!string.IsNullOrEmpty(line))
                        {
                            string eline = LogES.DESEncrypt(line, "AKB48Show!",
                                (version * version).ToString());
                            sw.WriteLine(eline);
                            sw.Flush();
                        }
                    }
                }
            });
        }

        public void Logger(string line) { queue.Add(line); }
    }
}
