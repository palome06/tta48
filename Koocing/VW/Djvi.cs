using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Zero.VW;
using Algo = Trench.Utils.Algo;

namespace Koocing.VW
{
    public class Djvi : Trench.VW.IVI
    {
        private Ayvi[] ayvis;

        private Trench.Log.SvLog Log { set; get; }

        public string CinSentinel { get { return "\\"; } }

        private CancellationTokenSource ctoken;

        public Djvi(int playerCount, Trench.Log.SvLog log)
        {
            ayvis = new Ayvi[playerCount];
            for (int i = 0; i < ayvis.Length; ++i)
                ayvis[i] = new Ayvi();
            Log = log;
            ctoken = new CancellationTokenSource();
        }

        public void Init() { StartListenTask(ListenToUpstream); }

        private void ListenToUpstream()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                line = line.Trim().ToUpper();
                Match match = Regex.Match(line, @"<\d*>.*", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int idx = line.IndexOf('>', match.Index);
                    ushort usr = ushort.Parse(Algo.Substring(line, match.Index + 1, idx));
                    string content = Algo.Substring(line, idx + 1, -1);
                    if (usr >= 0 && usr <= ayvis.Length)
                        ayvis[usr - 1].Offer(content);
                }
            }
        }

        public void SetInGame(bool value)
        {
            foreach (Ayvi ayvi in ayvis)
                ayvi.SetInGame(value);
        }

        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            Console.WriteLine("{" + me + "}" + string.Format(msgFormat, args));
        }

        public string Cin(ushort me, string hintFormat, params object[] args)
        {
            if (!string.IsNullOrEmpty(hintFormat))
                hintFormat = "{{" + me + "}}" + hintFormat;
            return ayvis[me - 1].Cin(me, hintFormat, args);
        }

        public void CloseCinTunnel(ushort me)
        {
            ayvis[me - 1].CloseCinTunnel(me);
        }

        public string RequestHelp(ushort me)
        {
            return ayvis[me - 1].RequestHelp(me);
        }

        // do not support pure chat here
        public string RequestTalk(ushort me) { return ""; }
        public void Chat(string msg, string nick) { }

        public void OpenCinTunnel(ushort me) { }
        public void TerminCinTunnel(ushort me) { }

        public void Close()
        {
            ctoken.Cancel(); ctoken.Dispose();
            foreach (Ayvi ayvi in ayvis)
                ayvi.Close();
        }
        /// <summary>
        /// start an async Listening task
        /// </summary>
        /// <param name="action">the acutal listen action</param>
        private void StartListenTask(Action action)
        {
            Action<Exception> ae = (e) => { if (Log != null) Log.Logger(e.ToString()); };
            Task.Factory.StartNew(() => Trench.Utils.Execute.SafeExecute(action, ae),
                ctoken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
