using System;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Humper.Sample.Basic
{
	public class PreviewScene : IScene
	{
		public PreviewScene()
		{
		}

		private RectangleF origin;

		private RectangleF goal;

		private RectangleF destination;

		private RectangleF collision;

		private RectangleF other;

		private RectangleF normal;

		private bool moveDestination, isMoving;
		private RectangleF cursor, selected;

		private CollisionResponses[] values = Enum.GetValues(typeof(CollisionResponses)) as CollisionResponses[];
		private int response = 0;

		private KeyboardState previous;

		public string Message 
		{ 
			get 
			{
				var moving = moveDestination ? nameof(goal) : nameof(origin);
				var changed = !moveDestination ? nameof(goal) : nameof(origin);
				var r = values[response];
				return $"[N]: select {changed} box\n[Space]: move selected {moving} box\n[R]: Change collision mode ({r})";
			}
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(origin, color: Color.Green, fillOpacity: 0.3f);
			sb.Draw(goal, color: Color.Red, fillOpacity: 0.3f);
			sb.Draw(other, color: new Color(165, 155, 250), fillOpacity: 0.3f);
			sb.Draw(collision, color: Color.Orange, fillOpacity: 0.0f);
			sb.Draw(normal, color: Color.Orange, fillOpacity: 0.3f);
			sb.Draw(destination, color: Color.Green, fillOpacity: 0.0f);
			var s = destination.Size / 10;
			sb.Draw(new RectangleF(destination.Center - (s / 2), s), color: Color.Green, fillOpacity: 0.0f);
			sb.Draw(cursor, color: Color.White, fillOpacity: 0.0f);
			sb.Draw(selected, color: Color.White, fillOpacity: 0.5f);
		}

		public void Initialize()
		{
			origin = new RectangleF(0, 0, 100, 100);
			goal = new RectangleF(400, 300, 100, 100);
			selected = new RectangleF(-3, -3 , 6, 6);

			other = new RectangleF(200, 200, 500, 120);
		}


		public void Update(GameTime time)
		{
			var state = Keyboard.GetState();
			if (previous.IsKeyUp(Keys.N) && state.IsKeyDown(Keys.N))
			{
				moveDestination = !moveDestination;
				selected.Location = (moveDestination ? goal.Location : origin.Location) - selected.Size / 2;
			}

			if (previous.IsKeyUp(Keys.R) && state.IsKeyDown(Keys.R))
			{
				
				response = (response + 1) % values.Length;
			}

			previous = state;

			isMoving = state.IsKeyDown(Keys.Space);
			var m = Mouse.GetState().Position;
			var pos = new Base.Vector2(m.X, m.Y);
			var size = isMoving ? 18 : 6;
			cursor = new RectangleF(m.X - size/2, m.Y - size/2,size, size);


			if (isMoving)
			{
				selected.Location = pos - selected.Size / 2;
				
				if (moveDestination)
				{
					goal.Location = pos;
				}
				else
				{
					origin.Location = pos;
				}
			}

			// Calculate collision
			var hit = Hit.Resolve(origin, goal, other);
			var r = values[response];

			if (hit != null && r != CollisionResponses.None)
			{
				collision = new RectangleF(hit.Position, origin.Size);
				normal = new RectangleF(collision.Center + hit.Normal * 50, new Base.Vector2(5, 5));

				// Destination
				var collisionPoint = new Collision()
				{
					Origin = origin,
					Goal = goal,
					Hit = hit,
				};

				destination = CollisionResponse.Create(collisionPoint,r)?.Destination ?? goal;
			}
			else
			{
				collision = new RectangleF();
				normal = new RectangleF();
				destination = goal;
			}
				
		}

		public void LoadContent(ContentManager content)
		{

		}
	}
}

