using System;

namespace Koocing.VW
{
    public class Ajvi : Trench.VW.IVI
    {
        public void Chat(string msg, string nick) { }

        public string Cin(ushort me, string hintFormat, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Close() { }

        public void CloseCinTunnel(ushort me) { }

        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            Console.WriteLine(msgFormat, args);
        }

        public void Init() { }

        public string RequestHelp(ushort me)
        {
            throw new NotImplementedException();
        }

        public string RequestTalk(ushort me)
        {
            throw new NotImplementedException();
        }

        public void SetInGame(bool value) { }
    }
}
