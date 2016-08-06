using System.Collections.Generic;
using Trench.Utils;

namespace Trench.Card
{
	public interface Bard
	{
		// card code printed in the body, e.g. XJ101
		string OfCode { get; }
		// internal card rank, integer, e.g. 10001
		int Avatar { get; }
		// nick, used only for debug/moniter, not name
		string Nick { get; }
		// card genre
		int Genre { get; }
		// package count, e.g. { 0, [0, 1]; 1, [2, 4]}
		IDictionary<ushort, Range> Package { get; }
	}
}