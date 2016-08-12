namespace Trench
{
	// Account Info, player would derive from it
    public class Gamer
    {
    	public string Name { private set; get; }
    	public ushort Avatar { private set; get; }
    	// from given account, not game seat position
    	public ushort AUid { set; get; }
        // ok the game seat position
        public ushort Uid { set; get; }

    	public Gamer(string name, ushort avatar, ushort auid, ushort ut)
    	{
    		Name = name; Avatar = avatar;
            AUid = auid; Uid = ut;
    	}
    }
}
