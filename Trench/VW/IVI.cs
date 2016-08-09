using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trench.VW
{
    public interface IVI
    {
        // Initial listening receive
        void Init();
        // Set whether the game is started or still in preparation
        // thus whether operations can be accepted or not
        void SetInGame(bool value);
        // display $msg on $me screen
        void Cout(ushort me, string msgFormat, params object[] args);
        // require input from $me, with hint as $hint
        string Cin(ushort me, string hintFormat, params object[] args);
        // Close Cin Tunnel
        void CloseCinTunnel(ushort me);
        // Request in Client
        string RequestHelp(ushort me);
        // Talk in Client
        string RequestTalk(ushort me);
        // Display the chat message send from the others before room
        void Chat(string msg, string nick);
        // close and recycle resources
        void Close();
    }
}
