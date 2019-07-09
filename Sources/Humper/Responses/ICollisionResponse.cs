using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper.Responses
{
	/// <summary>
	///     The result of a collision reaction onto a box position.
	/// </summary>
	public interface ICollisionResponse
    {
	    /// <summary>
	    ///     Gets the new destination of the box after the collision.
	    /// </summary>
	    /// <value>The destination.</value>
	    Rect Destination { get; }
    }
}