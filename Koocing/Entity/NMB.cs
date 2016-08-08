using Koocing.SKBranch;
using Trench.Card;

namespace Koocing.Entity
{
    public interface NMB : Bard
    {
        void RegisterSKB(SKB skb);
        // Get the SKB
        SKB GetSKB(int consume, int inType);
    }
}
