namespace Humper
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Base;
	using Responses;

	public class World 
	{
		public World(IBroadPhase broadPhase)
		{
			this._broadPhase = broadPhase;
		}

		public RectangleF Bounds => _broadPhase.Bounds;

		#region Boxes

		private IBroadPhase _broadPhase;

		public int Boxes {get;private set;}

		public Box Create(RectangleF area)
		{
			var box = new Box(this, area);
			this._broadPhase.Add(box);
			Boxes++;
			return box;
		}

		public IEnumerable<Box> Find(RectangleF area)
		{
			return this._broadPhase.QueryBoxes(area);
		}

		public bool Remove(Box box)
		{
			Boxes--;
			return this._broadPhase.Remove(box);
		}

		public void Update(Box box, RectangleF from)
		{
			this._broadPhase.Update(box, from);
		}

		#endregion

		#region Hits

		public IHit Hit(Vector2 point, IEnumerable<Box> ignoring = null)
		{
			var boxes = this._broadPhase.QueryBoxes(new RectangleF(point,Vector2.Zero));

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(point, other);

				if (hit != null)
				{
					return hit;
				}
			}

			return null;
		}

		public IHit Hit(Vector2 origin, Vector2 destination, IEnumerable<Box> ignoring = null)
		{
			var min = Vector2.Min(origin, destination);
			var max = Vector2.Max(origin, destination);

			var wrap = RectangleF.FromPoints(min, max);
			var boxes = this.Find(wrap);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			IHit nearest = null;

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(origin, destination, other);

				if (hit != null && (nearest == null || hit.IsNearest(nearest,origin)))
				{
					nearest = hit;
				}
			}

			return nearest;
		}

		public IHit Hit(RectangleF origin, RectangleF destination, IEnumerable<Box> ignoring = null)
		{
			var wrap = new RectangleF(origin, destination);
			var boxes = this.Find(wrap);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			IHit nearest = null;

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(origin, destination, other);

				if (hit != null && (nearest == null || hit.IsNearest(nearest, origin.Location)))
				{
					nearest = hit;
				}
			}

			return nearest;
		}

		#endregion

		#region Movements

		public IMovement Simulate(Box box, Vector2 destination, Func<ICollision, ICollisionResponse> filter)
		{
			var origin = box.Bounds;
			var goal = new RectangleF(destination, box.Bounds.Size);

			var hits = new List<IHit>();

			var result = new Movement()
			{
				Origin = origin,
				Goal = goal,
				Destination = this.Simulate(hits, new List<Box>() { box }, box, origin, goal, filter),
				Hits = hits,
			};

			return result;
		}

		private RectangleF Simulate(List<IHit> hits, List<Box> ignoring, Box box, RectangleF origin, RectangleF destination, Func<ICollision, ICollisionResponse> filter)
		{
			var nearest = this.Hit(origin, destination, ignoring);
				
			if (nearest != null)
			{
				hits.Add(nearest);

				var impact = new RectangleF(nearest.Position, origin.Size);
				var collision = new Collision() { Box = box, Hit = nearest, Goal = destination, Origin = origin };
				var response = filter(collision);

				if (response != null && destination != response.Destination)
				{
					ignoring.Add(nearest.Box);
					return this.Simulate(hits, ignoring, box, impact, response.Destination, filter);
				}
			}

			return destination;
		}

		#endregion

		#region Diagnostics

		public void DrawDebug(RectangleF area, Action<int,int,int,int,float> drawCell, Action<Box> drawBox, Action<string,int,int, float> drawString)
		{
			// Drawing boxes
			var boxes = _broadPhase.QueryBoxes(area);
			foreach (var box in boxes)
			{
				drawBox(box);
			}

			_broadPhase.DrawDebug(area, drawCell, drawBox, drawString);
		}

		#endregion
	}
}

