using Koocing.VW;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Trench;
using Trench.Log;
using Trench.Utils;

namespace Koocing.VW
{
    // Server In it
    public class Aywi : Trench.VW.IWISV
    {
        public const int MSG_SIZE = 4096;
        // token to terminate all running thread when Aywi is closed
        private CancellationTokenSource ctoken;

        private TcpListener listener;
        private int port; // actual port number taken room int consideration

        // network stream to center, to replace Pipe
        private NetworkStream cns;

        private IDictionary<ushort, Neayer> neayers;
        private IDictionary<ushort, Netcher> netchers;
        private Trench.VW.IVI vi;
        // alive before C2AS confirmation
        private IDictionary<ushort, Neayer> n1;

        private bool isRecvBlocked = false;
        // indicate whether the room is hanged up, thus blocking all recv action
        public bool IsHangedUp
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set { isRecvBlocked = value; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return isRecvBlocked; }
        }
        // indicate whether the room has exited gracefully, won't report lose connection
        private bool IsLegecy { set; get; }

        private Trench.Log.SvLog Log { set; get; }

        #region Uid assignee
        // userid counter, only used in direct mode
        private ushort curCount = 1;
        private ushort watchCount = 1001;
        public bool IsPlayer(ushort gid) { return gid > 0 && gid < 1000; }
        public bool IsWatcher(ushort gid) { return gid > 1000 && gid < 2000; }
        public bool IsGod(ushort gid) { return gid == 0 || gid == 1000; }
        #endregion Uid assignee

        #region Network of Player
        // n1 is a temporary version of neayers (uid not rearranged), netchers is usable
        private void ConnectDo(Socket socket, List<ushort> valids, IDictionary<ushort, Neayer> n1)
        {
            NetworkStream ns = new NetworkStream(socket);
            string data = Trench.VW.WHelper.ReadByteLine(ns);
            //string addr = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            if (data == null) { return; }
            else if (data.StartsWith("C2CO,"))
            {
                string[] blocks = data.Split(',');
                ushort ut = ushort.Parse(blocks[1]);
                string uname = blocks[2];
                ushort uava = ushort.Parse(blocks[3]);
                int uHope = int.Parse(blocks[4]);

                if (n1 == null)
                    Trench.VW.WHelper.SentByteLine(ns, "C2CN,0");

                if (valids != null)
                {
                    if (valids.Contains(ut) && GetAliveNeayersCount() < playerCapacity)
                    {
                        Neayer ny = new Neayer(uname, uava)
                        {
                            Uid = ut, // actual Uid isn't set yet
                            AUid = ut,
                            HopeTeam = uHope,
                            Tunnel = socket
                        };
                        n1.Add(ut, ny);
                        vi.Cout(0, "[{0}]{1} joined.", ny.AUid, uname);
                        Trench.VW.WHelper.SentByteLine(ns, "C2CN," + ny.AUid);
                    }
                    else
                        Trench.VW.WHelper.SentByteLine(ns, "C2CN,0");
                }
                else // In Direct mode, exit isn't allowed, AUid isn't useful.
                {
                    ushort allocUid = (ushort)(curCount++);
                    Neayer ny = new Neayer(uname, uava)
                    {
                        Uid = allocUid,
                        AUid = allocUid,
                        HopeTeam = uHope,
                        Tunnel = socket
                    };
                    string c2rm = "";
                    string c2nw = "C2NW," + ny.Uid + "," + ny.Name + "," + ny.Avatar;
                    foreach (Neayer nyr in n1.Values)
                    {
                        c2rm += "," + nyr.Uid + "," + nyr.Name + "," + nyr.Avatar;
                        Trench.VW.WHelper.SentByteLine(new NetworkStream(nyr.Tunnel), c2nw);
                    }
                    n1.Add(ny.Uid, ny);
                    vi.Cout(0, "[{0}]{1} joined.", ny.Uid, uname);
                    Trench.VW.WHelper.SentByteLine(ns, "C2CN," + ny.Uid);
                    if (c2rm.Length > 0)
                        Trench.VW.WHelper.SentByteLine(ns, "C2RM" + c2rm);
                }
            }
            else if (data.StartsWith("C2QI,"))
            {
                string[] blocks = data.Split(',');
                ushort ut = (valids != null) ? ushort.Parse(blocks[1]) : watchCount++;
                string uname = blocks[2];
                while (netchers.ContainsKey(ut))
                    ++ut;
                Netcher nc = new Netcher(uname, ut) { Tunnel = socket }; 
                netchers.Add(ut, nc);
                Trench.VW.WHelper.SentByteLine(ns, "C2QJ," + ut);
            }
        }
        public void TcpListenerStart()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }
        // Hall path of construct Aywi, successors is the list player to join, null when indirect
        public IDictionary<ushort, Gamer> Connect(Trench.VW.IVI vi, bool selTeam, List<ushort> valids)
        {
            n1 = new Dictionary<ushort, Neayer>();
            neayers = new Dictionary<ushort, Neayer>();
            netchers = new Dictionary<ushort, Netcher>();
            this.vi = vi;

            while (n1.Count < playerCapacity)
            {
                Socket socket = listener.AcceptSocket();
                // TODO-tbl60: set neayer.HopeTeam but do not handle with it
                try { ConnectDo(socket, valids, n1); } // no leave allowed.
                catch (SocketException) { }
            }
            // TODO-tbl60: set rearrange as abstract
            IDictionary<ushort, ushort> cgmap = Rearrange(selTeam, n1);
            IDictionary<ushort, Gamer> newGarden = new Dictionary<ushort, Gamer>();
            neayers = new Dictionary<ushort, Neayer>();
            foreach (var pair in n1)
            {
                Neayer ny = pair.Value;
                ushort ut = cgmap[pair.Key];
                Gamer player = new Gamer(ny.Name, ny.Avatar, pair.Key, ut);
                ny.Uid = ut;
                newGarden.Add(ut, player);
                neayers.Add(ut, ny);
            }
            foreach (var pair in neayers)
            {
                StartListenTask(() => KeepOnListenRecv(pair.Value));
                Trench.VW.WHelper.SentByteLine(new NetworkStream(pair.Value.Tunnel), "C2SA,0");
            }
            foreach (var pair in netchers.ToList())
            {
                StartListenTask(() => KeepOnListenRecv(pair.Value));
                Trench.VW.WHelper.SentByteLine(new NetworkStream(pair.Value.Tunnel), "C2SA,0");
            }
            StartListenTask(() => KeepOnListenSend());
            return newGarden;
        }
        private IDictionary<ushort, ushort> Rearrange(bool selTeam, IDictionary<ushort, Neayer> n1)
        {
            IDictionary<ushort, ushort> uidMap = new Dictionary<ushort, ushort>();
            if (selTeam)
            {
                List<int> range = new List<int>(Enumerable.Range(1, playerCapacity));
                List<int> team1 = range.Where(p => p % 2 == 1).ToList();
                team1.Shuffle();
                List<int> team2 = range.Where(p => p % 2 == 0).ToList();
                team2.Shuffle();
                Random random = new Random();
                if (random.Next() % 2 == 0)
                {
                    List<int> tmp = team1; team1 = team2; team2 = tmp;
                }

                List<ushort> hope1 = new List<ushort>();
                List<ushort> hope2 = new List<ushort>();
                List<ushort> hope0 = new List<ushort>();

                IDictionary<string, List<ushort>> dictCount = new Dictionary<string, List<ushort>>();
                foreach (var pair in n1)
                {
                    Neayer ny = pair.Value;
                    string name = "";
                    if (ny.HopeTeam == Trench.Rules.RuleCode.HOPE_AKA)
                        name = "AKA";
                    else if (ny.HopeTeam == Trench.Rules.RuleCode.HOPE_AO)
                        name = "AO";
                    else if (ny.HopeTeam == Trench.Rules.RuleCode.HOPE_IP)
                        name = (ny.Tunnel.RemoteEndPoint as IPEndPoint).Address.ToString();
                    else
                        hope0.Add(ny.Uid);
                    if (name != "")
                    {
                        if (!dictCount.ContainsKey(name))
                            dictCount.Add(name, new List<ushort>());
                        dictCount[name].Add(ny.Uid);
                    }
                }
                //List<List<ushort>> rests = dictCount.Values.Where(p => p.Count > 1).ToList();
                var pq = new PriorityQueue<List<ushort>>(new ListSizeComparer<ushort>());
                foreach (var list in dictCount.Values)
                {
                    if (list.Count > 1)
                        pq.Push(list);
                    else if (list.Count == 1)
                        hope0.Add(list[0]);
                }

                while (pq.Count > 0)
                {
                    List<ushort> list = pq.Pop();
                    int a = hope1.Count, b = hope2.Count;
                    if (a + list.Count <= b)
                        hope1.AddRange(list);
                    else if (b + list.Count <= a)
                        hope2.AddRange(list);
                    else if (a <= b)
                        hope1.AddRange(list);
                    else
                        hope2.AddRange(list);
                }
                //foreach (var pair in n1)
                //{
                //    Neayer ny = pair.Value;
                //    if (ny.HopeTeam == 1)
                //        hope1.Add(ny.Uid);
                //    else if (ny.HopeTeam == 2)
                //        hope2.Add(ny.Uid);
                //    else
                //        hope0.Add(ny.Uid);
                //}
                hope1.Shuffle();
                hope2.Shuffle();
                hope0.Shuffle();

                int dif1 = hope1.Count - team1.Count;
                if (dif1 > 0)
                {
                    var removes = hope1.Take(dif1).ToList();
                    hope1.RemoveAll(p => removes.Contains(p));
                    hope0.AddRange(removes);
                }
                else if (dif1 < 0)
                {
                    var removes = hope0.Take(-dif1).ToList();
                    hope0.RemoveAll(p => removes.Contains(p));
                    hope1.AddRange(removes);
                }
                for (int i = 0; i < hope1.Count; ++i)
                    uidMap.Add(hope1[i], (ushort)team1[i]);
                int dif2 = hope2.Count - team2.Count;
                if (dif2 > 0)
                {
                    var removes = hope2.Take(dif2).ToList();
                    hope2.RemoveAll(p => removes.Contains(p));
                    hope0.AddRange(removes);
                }
                else if (dif2 < 0)
                {
                    var removes = hope0.Take(-dif2).ToList();
                    hope0.RemoveAll(p => removes.Contains(p));
                    hope2.AddRange(removes);
                }
                for (int i = 0; i < hope2.Count; ++i)
                    uidMap.Add(hope2[i], (ushort)team2[i]);
            }
            else
            {
                List<int> uidList = new List<int>(Enumerable.Range(1, playerCapacity));
                uidList.Shuffle();
                int i = 0;
                foreach (var pair in n1)
                    uidMap.Add(pair.Key, (ushort)uidList[i++]);
            }
            return uidMap;
        }
		// Catch new room comer, containing watcher and reconnector
		public ushort CatchNewRoomComer()
        {
            if (listener == null) { return 0; }
            Socket socket;
            try { socket = listener.AcceptSocket(); }
            catch (SocketException) { return 0; }

            NetworkStream ns = new NetworkStream(socket);
            string data = Trench.VW.WHelper.ReadByteLine(ns);
            if (data == null) { return 0; }
            else if (data.StartsWith("C2QI,")) // Watcher case
            {
                string[] blocks = data.Split(',');
                ushort ut = ushort.Parse(blocks[1]);
                if (ut == 0)
                    ut = watchCount++;
                string uname = blocks[2];
                while (netchers.ContainsKey(ut))
                    ++ut;
                Netcher nc = new Netcher(uname, ut) { Tunnel = socket };
                netchers.Add(ut, nc);
                Trench.VW.WHelper.SentByteLine(ns, "C2QJ," + ut);
                Trench.VW.WHelper.SentByteLine(ns, "C2SA,0");
                return ut;
            }
            else if (data.StartsWith("C4CR,")) // Reconnect case
            {
                // C4CR,u,i,A,cd
                string[] blocks = data.Split(',');
                ushort newUt = ushort.Parse(blocks[1]);
                ushort oldUt = ushort.Parse(blocks[2]);
                string uname = blocks[3];
                string roomPwd = blocks[4];
                ushort gameUt = neayers.Values.First(p => p.AUid == oldUt).Uid;

                Neayer ny = new Neayer(uname, 0) // set avatar = 0, not care
                {
                    AUid = newUt,
                    Uid = gameUt,
                    HopeTeam = 0,
                    Tunnel = socket,
                    Alive = false
                };
                neayers[ny.Uid] = ny;
                StartListenTask(() => KeepOnListenRecv(ny));
                Trench.VW.WHelper.SentByteLine(ns, "C4CS," + ny.Uid);
                ny.Alive = true;
                WakeTunnelInWaiting(ny.AUid, ny.Uid);
                return ny.Uid;
            }
            else
            {
                Trench.VW.WHelper.SentByteLine(ns, "C2CN,0");
                return 0;
            }
        }
        #endregion Network of Player
        #region Communication and Tunnel

        public bool IsTalkSilence { set; get; }
        // $paruru is of type Neayer, register to the socket
        // only support neayer. Watcher -> Indirect Live Message only
        private void KeepOnListenRecv(object paruru)
        {
            Neayer ny = (Neayer)paruru;
            while (true)
            {
                string line = "";
                try
                {
                    line = Trench.VW.WHelper.ReadByteLine(new NetworkStream(ny.Tunnel));
                    if (string.IsNullOrEmpty(line)) { ny.Tunnel.Close(); OnLoseConnection(ny.Uid); break; }
                }
                catch (IOException) { OnLoseConnection(ny.Uid); break; }
                // Always return, Keep on find survivors if game not end, otherwise...
                // and stop searching for survivors after a time limit (300s)
                if (line.StartsWith("Y")) // word
                {
                    if (yMsgHandler != null)
                        yMsgHandler(line, ny.Uid);
                }
                else if (!string.IsNullOrEmpty(line) && !IsHangedUp)
                {
                    inf0Msgs.Add(new Trench.VW.Msgs(line, ny.Uid, 0), ctoken.Token);
                    //Log.Logger(0 + "<" + ny.Uid + ":" + line);
                }
                else
                    Thread.Sleep(80);
            }
        }
        private void KeepOnListenSend()
        {
            while (true)
            {
                Trench.VW.Msgs msg = infNMsgs.Take(ctoken.Token);
                if (msg.From == 0) // send won't be blocked
                {
                    if (neayers.ContainsKey(msg.To) && neayers[msg.To].Alive)
                    {
                        Log.Logger(0 + ">" + msg.To + ";" + msg.Msg);
                        try { Trench.VW.WHelper.SentByteLine(new NetworkStream(neayers[msg.To].Tunnel), msg.Msg); }
                        catch (IOException) { OnLoseConnection(msg.To); break; }
                    }
                    else
                        KeepOnListenSendWatcher(msg);
                }
                else
                    Thread.Sleep(80);
            }
        }
        // Watcher case, won't cause exception to notify leaves
        private bool KeepOnListenSendWatcher(Trench.VW.Msgs msg)
        {
            try
            {
                if (netchers.ContainsKey(msg.To))
                    Trench.VW.WHelper.SentByteLine(new NetworkStream(netchers[msg.To].Tunnel), msg.Msg);
                return true;
            }
            catch (IOException)
            {
                // On Leave of watcher, don't care.
                Log.Logger("%%Watcher(" + msg.To + ") Leaves.");
                netchers.Remove(msg.To);
                //if (GetAliveNeayersCount() == 0 && netchers.Count == 0)
                //    Environment.Exit(0);
                return false;
            }
        }
        // Start a new thread to listen to waiting stage
        private bool StartWaitingStage()
        {
            int timeout = 0;
            while ((GetAliveNeayersCount() < playerCapacity))
            {
                if (GetAliveNeayersCount() == 0 && timeout < 1800)
                {
                    if (vi != null) vi.Cout(0, "Run for Escape - 全员离线中.");
                    timeout = 1800;
                }
                Thread.Sleep(100);
                ++timeout;
                if (timeout == 3000 || timeout == 4200)
                {
                    int left = (4800 - timeout) / 10;
                    Send("H0WD," + left, neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
                    Live("H0WD," + left);
                }
                else if (timeout == 4800) { OnBrokenConnection(); return false; }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void WakeTunnelInWaiting(ushort auid, ushort suid)
        {
            Send("H0BK," + suid, neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
            Live("H0BK," + suid);
            // Awake the neayer
            if (neayers.ContainsKey(suid))
                neayers[suid].Alive = true;
            Trench.VW.WHelper.SentByteLine(cns, "C3RA," + auid);
            // Check whether all members has gathered.
            if (GetAliveNeayersCount() == playerCapacity)
            {
                // OK, all gathered.
                BCast("H0RK,0");
                Trench.VW.WHelper.SentByteLine(cns, "C3RV,0");
                IsHangedUp = false;
            }
        }
        // lose the connection with $who, hoping to get echo and resume game
        private void OnLoseConnection(ushort who)
        {
            if (neayers.ContainsKey(who) && neayers[who].Alive)
            {
                neayers[who].Alive = false;
                if (!IsLegecy)
                {
                    if (vi != null) vi.Cout(0, "玩家{0}掉线，房间等待重连中.", who);
                    Send("H0WT," + who, neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
                    Live("H0WT," + who);

                    Report("C3LS," + neayers[who].AUid);
                }
                //if (GetAliveNeayersCount() == 0 && netchers.Count == 0)
                //    Bye();
                if (!IsLegecy && !IsHangedUp)
                {
                    // Start Waiting thread and init news signal queue
                    IsHangedUp = true;
                    StartListenTask(() => StartWaitingStage());
                }
            }
        }
        // completetly lose connection, then terminate the room
        private void OnBrokenConnection()
        {
            if (vi != null) vi.Cout(0, "房间严重损坏，本场游戏终结.");
            Send("H0LT,0", neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
            Live("H0LT,0");
            Thread.Sleep(1000); // Wait for sending out H0LT before Bye()
            Bye();
        }
        // report to fake pipe
        public void Report(string message)
        {
            if (cns != null && !IsLegecy)
                Trench.VW.WHelper.SentByteLine(cns, message);
        }
        // terminate the room
        public void RoomGameEnd()
        {
            Report("C3TM,0");
            IsLegecy = true;
        }
        // bury the room
        public void RoomBury()
        {
            // Report("C3BR,0");
            ctoken.Cancel(); ctoken.Dispose();
            listener.Stop();
            inf0Msgs.Dispose(); infNMsgs.Dispose();
        }
        #endregion Communication and Tunnel

        #region Fake Pipe
        public void StartFakePipe(int roomNum)
        {
            TcpClient client = new TcpClient("127.0.0.1", Trench.Rules.NetworkCode.HALL_PORT);
            NetworkStream tcpStream = client.GetStream();
            Trench.VW.WHelper.SentByteLine(tcpStream, "C3HI," + roomNum);
            cns = tcpStream;
        }
        /// <summary>
        /// shutdown the room and the pipe, happens when C2ST not gathered ready
        /// </summary>
        /// <param name="roomNum">the room number/param>
        public void ShutdownFakePipe(int roomNum)
        {
            Trench.VW.WHelper.SentByteLine(cns, "C3HX," + roomNum);
            // TODO: notify users that has joined the room
            // QUESTION: how to know who has died yet
            if (n1 != null)
            {
                foreach (Neayer ny in n1.Values)
                {
                    try { Trench.VW.WHelper.SentByteLine(new NetworkStream(ny.Tunnel), "C2SB,0"); }
                    catch (SocketException) { }
                    catch (ObjectDisposedException) { }
                }
            }
            Environment.Exit(0);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private int GetAliveNeayersCount()
        {
            lock (neayers) { return neayers.Values.Count(p => p.Alive); }
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
        #endregion Fake Pipe
        // player capacity, won't change during the game (volitate number would be solved in Cente)
        private readonly int playerCapacity;
        // message queue of handling inf
        private BlockingCollection<Trench.VW.Msgs> inf0Msgs;
        // only for send
        private BlockingCollection<Trench.VW.Msgs> infNMsgs;
        // handler of Y message, inits from XI Instance.
        private Action<string, ushort> yMsgHandler;

        public Aywi(int port, int playerCapacity, SvLog log, Action<string, ushort> yHandler)
        {
            this.port = port;
            this.playerCapacity = playerCapacity;
            inf0Msgs = new BlockingCollection<Trench.VW.Msgs>(new ConcurrentQueue<Trench.VW.Msgs>());
            infNMsgs = new BlockingCollection<Trench.VW.Msgs>(new ConcurrentQueue<Trench.VW.Msgs>());

            IsTalkSilence = false;
            IsLegecy = false;
            this.yMsgHandler = yHandler;
            this.Log = log;
            ctoken = new CancellationTokenSource();
        }
        #region Implemetation
        // Get input result from $from to $me (require reply from $side to $me)
        public string Recv(ushort me, ushort from)
        {
            if (me == 0)
            {
                Trench.VW.Msgs rvDeq = inf0Msgs.Take();
                if (rvDeq.From == from && !string.IsNullOrEmpty(rvDeq.Msg))
                {
                    Log.Logger("=" + from + ":" + rvDeq.Msg);
                    return rvDeq.Msg;
                }
            }
            return null;
        }
        // receive each message during the process
        public Trench.VW.Msgs RecvInfRecv()
        {
            return inf0Msgs.Take();
        }
        // Send raw message from $me to $to
        public void Send(string msg, ushort me, ushort to)
        {
            if (me == 0)
                infNMsgs.Add(new Trench.VW.Msgs(msg, me, to));
        }
        // Send raw message to multiple $to
        public void Send(string msg, IEnumerable<ushort> tos)
        {
            tos.Intersect(neayers.Keys).ToList().ForEach(p => Send(msg, 0, p));
        }
        // send in general, might get combined results
        public void Send(IDictionary<ushort, string> table, string live)
        {
            table.ToList().ForEach(p => Send(p.Value, 0, p.Key));
            Live(live);
        }
        // send $msg to who and nofify the others with live
        public void Focus(ushort who, string msg, string live)
        {
            Send(msg, 0, who);
            Send(live, neayers.Keys.Except(new ushort[] { who }));
            Live(live);
        }
        public void Live(string msg)
        {
            List<ushort> nets = netchers.Keys.ToList();
            nets.ForEach(p => Send(msg, 0, p));
        }
        // Send raw message to the whole
        public void BCast(string msg)
        {
            Send(msg, neayers.Keys.ToArray());
            Live(msg);
        }
        public void Shutdown()
        {
            ctoken.Cancel(); ctoken.Dispose();
            listener.Stop();
        }
        private void Bye()
        {
            if (vi != null) vi.Cout(0, "房间已回收.");
            Report("C3LV,0");
            ctoken.Cancel(); ctoken.Dispose();
            listener.Stop();
            inf0Msgs.Dispose(); infNMsgs.Dispose();
            Environment.Exit(0);
        }

        public void Dispose() { }
        #endregion Implementation
    }
}
