namespace Humper
{
	using System.Collections.Generic;
	using Base;

	public interface IMovement
	{
		IEnumerable<IHit> Hits { get; }

		bool HasCollided { get; }

		Rect Origin { get; }

		Rect Goal { get; }

		Rect Destination { get; }
	}
}

