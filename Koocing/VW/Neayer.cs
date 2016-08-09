using System.Collections.Generic;
using System.Net.Sockets;

namespace Koocing.VW
{
    public class Neayer
    {
        public string Name { private set; get; }
        public ushort Avatar { private set; get; }
        // Uid used in the room (1~6)
        public ushort Uid { set; get; }
        // Login Uid/Register Uid, passed from Center
        public ushort AUid { set; get; }
        // TODO: hope team, unsure whether necessary
        public int HopeTeam { set; get; }

        public Socket Tunnel { set; get; }

        public bool Alive { set; get; }

        public Neayer(string name, ushort avatar)
        {
            Name = name; Avatar = avatar;
            Alive = true;
        }
    }

    public class Netcher
    {
        public string Name { private set; get; }
         // Netcher doesn't need auid field in centre
        public ushort Uid { private set; get; }

        public Socket Tunnel { set; get; }
        public Netcher(string name, ushort uid)
        {
            Name = name; Uid = uid;
        }
    }
}
