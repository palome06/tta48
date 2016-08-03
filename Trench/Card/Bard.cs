using System.Collections.Generic;

namespace Trench.Card
{
	public interface BardGenre { }

	public abstract class Bard
	{
		// card code printed in the body, e.g. XJ101
		public string OfCode { set; get; }
		// internal card rank, integer, e.g. 10001
		public int Avatar { set; get; }
		// name, loaded from locale database
		public string Name { set; get; }
		// card genre, unsure whether necessary
		// public BardGenre Genre { set; get; }
		// package count, e.g. { 0, 1; 1, 2}
		public IDictionary<int, int> PackageCount { set; get; }
	}
}