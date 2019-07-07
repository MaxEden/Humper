namespace Humper
{
	using System;
	using Base;
	using Responses;

	/// <summary>
	/// Represents a physical body in the world.
	/// </summary>
	public interface IBox
	{
		#region Properties
		/// <summary>
		/// Gets the bounds of the box.
		/// </summary>
		/// <value>The bounds.</value>
		RectangleF Bounds { get; }

		/// <summary>
		/// Gets or sets custom user data attached to this box.
		/// </summary>
		/// <value>The data.</value>
		object Data { get; set; }

		#endregion

		#region Movements

		/// <summary>
		/// Tries to move the box to specified coordinates with collisition detection.
		/// </summary>
		IMovement Move(Vector2 destination, Func<ICollision, ICollisionResponse> filter);
		
		/// <summary>
		/// Tries to move the box to specified coordinates with collisition detection.
		/// </summary>
		IMovement Move(Vector2 destination, Func<ICollision, CollisionResponses> filter);

		/// <summary>
		/// Simulate the move of the box to specified coordinates with collisition detection. The boxe's position isn't
		/// altered.
		/// </summary>
		IMovement Simulate(Vector2 destination, Func<ICollision, ICollisionResponse> filter);

		/// <summary>
		/// Simulate the move of the box to specified coordinates with collisition detection. The boxe's position isn't
		/// altered.
		/// </summary>
		IMovement Simulate(Vector2 destination, Func<ICollision, CollisionResponses> filter);

		#endregion

		#region Tags

		/// <summary>
		/// Adds the tags to the box.
		/// </summary>
		/// <returns>The tags.</returns>
		/// <param name="newTags">New tags.</param>
		IBox AddTags(params Enum[] newTags);

		/// <summary>
		/// Removes the tags from the box.
		/// </summary>
		/// <returns>The tags.</returns>
		/// <param name="newTags">New tags.</param>
		IBox RemoveTags(params Enum[] newTags);

		/// <summary>
		/// Indicates whether the box has at least one of the given tags.
		/// </summary>
		/// <returns><c>true</c>, if tag was hased, <c>false</c> otherwise.</returns>
		/// <param name="values">Values.</param>
		bool HasTag(params Enum[] values);

		/// <summary>
		/// Indicates whether the box has all of the given tags.
		/// </summary>
		/// <returns><c>true</c>, if tags was hased, <c>false</c> otherwise.</returns>
		/// <param name="values">Values.</param>
		bool HasTags(params Enum[] values);

		#endregion
	}
}

