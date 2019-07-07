namespace Humper
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Base;
	using Responses;

	public class World : IWorld
	{
		public World(float width, float height, float cellSize = 64)
		{
			var iwidth = (int)Math.Ceiling(width / cellSize);
			var iheight = (int)Math.Ceiling(height / cellSize);

			this.grid = new Grid(iwidth, iheight, cellSize);
		}

		public RectangleF Bounds => new RectangleF(0, 0, this.grid.Width , this.grid.Height);

		#region Boxes

		private Grid grid;

		public IBox Create(float x, float y, float width, float height)
		{
			var box = new Box(this, x, y, width, height);
			this.grid.Add(box);
			return box;
		}

		public IEnumerable<IBox> Find(RectangleF area)
		{
			return this.grid.QueryBoxes(area);
		}

		public bool Remove(IBox box)
		{
			return this.grid.Remove(box);
		}

		public void Update(IBox box, RectangleF from)
		{
			this.grid.Update(box, from);
		}

		#endregion

		#region Hits

		public IHit Hit(Vector2 point, IEnumerable<IBox> ignoring = null)
		{
			var boxes = this.grid.QueryBoxes(new RectangleF(point,Vector2.Zero));

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

		public IHit Hit(Vector2 origin, Vector2 destination, IEnumerable<IBox> ignoring = null)
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

		public IHit Hit(RectangleF origin, RectangleF destination, IEnumerable<IBox> ignoring = null)
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
				Destination = this.Simulate(hits, new List<IBox>() { box }, box, origin, goal, filter),
				Hits = hits,
			};

			return result;
		}

		private RectangleF Simulate(List<IHit> hits, List<IBox> ignoring, Box box, RectangleF origin, RectangleF destination, Func<ICollision, ICollisionResponse> filter)
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

		public void DrawDebug(RectangleF area, Action<int,int,int,int,float> drawCell, Action<IBox> drawBox, Action<string,int,int, float> drawString)
		{
			// Drawing boxes
			var boxes = this.grid.QueryBoxes(area);
			foreach (var box in boxes)
			{
				drawBox(box);
			}

			// Drawing cells
			var cells = this.grid.QueryCells(area);
			foreach (var cell in cells)
			{
				var count = cell.Count();
				var alpha = count > 0 ? 1f : 0.4f;
				drawCell((int)cell.Bounds.X, (int)cell.Bounds.Y, (int)cell.Bounds.Width, (int)cell.Bounds.Height, alpha);
				drawString(count.ToString(), (int)cell.Bounds.Center.X, (int)cell.Bounds.Center.Y,alpha);
			}
		}

		#endregion
	}
}

