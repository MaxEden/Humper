using System;
using Humper.Base;
using Humper.Responses;
using Mandarin.Common.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Mandarin.Common.Misc.Vector2;
using Humper.Sample.Basic;

namespace Humper.Sample.Basic
{
	public class PreviewScene : IScene
	{
		public PreviewScene()
		{
		}

		private Rect origin;

		private Rect goal;

		private Rect destination;

		private Rect collision;

		private Rect other;

		private Rect normal;

		private bool moveDestination, isMoving;
		private Rect cursor, selected;

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
			sb.Draw(new Rect(destination.Center - (s / 2), s), color: Color.Green, fillOpacity: 0.0f);
			sb.Draw(cursor, color: Color.White, fillOpacity: 0.0f);
			sb.Draw(selected, color: Color.White, fillOpacity: 0.5f);
		}

		public void Initialize()
		{
			origin = new Rect(0, 0, 100, 100);
			goal = new Rect(400, 300, 100, 100);
			selected = new Rect(-3, -3 , 6, 6);

			other = new Rect(200, 200, 500, 120);
		}


		public void Update(GameTime time)
		{
			var state = Keyboard.GetState();
			if (previous.IsKeyUp(Keys.N) && state.IsKeyDown(Keys.N))
			{
				moveDestination = !moveDestination;
				selected.Position = (moveDestination ? goal.Position : origin.Position) - selected.Size / 2;
			}

			if (previous.IsKeyUp(Keys.R) && state.IsKeyDown(Keys.R))
			{
				
				response = (response + 1) % values.Length;
			}

			previous = state;

			isMoving = state.IsKeyDown(Keys.Space);
			var m = Mouse.GetState().Position;
			var pos = new Vector2(m.X, m.Y);
			var size = isMoving ? 18 : 6;
			cursor = new Rect(m.X - size/2, m.Y - size/2,size, size);


			if (isMoving)
			{
				selected.Position = pos - selected.Size / 2;
				
				if (moveDestination)
				{
					goal.Position = pos;
				}
				else
				{
					origin.Position = pos;
				}
			}

			// Calculate collision
			var hit = Hit.Resolve(origin, goal, other);
			var r = values[response];

			if (hit != null && r != CollisionResponses.None)
			{
				collision = new Rect(hit.Position, origin.Size);
				normal = new Rect(collision.Center + hit.Normal * 50, new Vector2(5, 5));

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
				collision = new Rect();
				normal = new Rect();
				destination = goal;
			}
				
		}

		public void LoadContent(ContentManager content)
		{

		}
	}
}

