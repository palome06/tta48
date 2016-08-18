using System;

namespace Trench.Utils
{
	public class Execute
	{
        public static void SafeExecute(Action action, Action<Exception> handler)
        {
            try { action(); }
            catch (System.Threading.ThreadAbortException) { }
            catch (System.OperationCanceledException) { throw; }
            catch (Exception ex) { handler(ex); Console.WriteLine(ex.ToString()); }
        }

        public static void StartTask(Action action, Log log)
        {
            
        }
    }
}
