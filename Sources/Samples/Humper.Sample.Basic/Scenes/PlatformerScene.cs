using Humper.Base;
using Mandarin.Common.Misc;
using Microsoft.Xna.Framework;
using Vector2 = Mandarin.Common.Misc.Vector2;

namespace Humper.Sample.Basic
{
	using System;
	using Responses;
	using Microsoft.Xna.Framework.Input;
	using System.Linq;

	public class PlatformerScene : WorldScene
	{
		public class Crate
		{
			public Crate(Box box)
			{
				this.box = box.AddTags(Tags.Group5);
				this.box.Data = this;
			}

			private Box box;

			public Vector2 velocity;

			private bool inWater;

			public void Update(float delta)
			{
				velocity.Y += delta * 0.001f;


				if (inWater)
					velocity.Y *= 0.5f;

				var move = box.Move(box.Bounds.Position + delta * velocity, (collision) =>
				{
					if (collision.Other.HasTag(Tags.Group3))
					{
						return Response.Cross(collision);
					}

					return Response.Slide(collision);
				});

				inWater = (move.Hits.Any((c) => c.Box.HasTag(Tags.Group3)));


				velocity.X *= 0.85f;

				// Testing if on ground
				if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group2) && (c.Normal.Y < 0)))
				{
					velocity.Y = 0;
					velocity.X *= 0.85f;
				}

			}
		}

		public PlatformerScene()
		{
		}

		private Box player1, platform;

		private Crate[] crates;

		private Vector2 platformVelocity = Vector2.right * 0.05f;

		public override void Initialize()
		{
			//World = new World(new Grid(1024, 700));
			World = new World(new DynamicTree());

			SpawnPlayer();

			platform = World.Create(new Rect(0, 200, 100, 20)).AddTags(Tags.Group4);

			crates = new[]
			{
				new Crate(World.Create(new Rect(150, 220, 40, 40))),
				new Crate(World.Create(new Rect(210, 220, 40, 40))),
			};
			
			// Map
			World.Create(new Rect(0, 300, 400, 20)).AddTags(Tags.Group2);
			World.Create(new Rect(380, 320, 20, 80)).AddTags(Tags.Group2);
			World.Create(new Rect(380, 400, 300, 20)).AddTags(Tags.Group2);
			World.Create(new Rect(420, 200, 200, 20)).AddTags(Tags.Group2);
			World.Create(new Rect(680, 220, 20, 200)).AddTags(Tags.Group2);
			World.Create(new Rect(680, 200, 200, 20)).AddTags(Tags.Group2);

			World.Create(new Rect(400, 300, 280, 100)).AddTags(Tags.Group3);
		}

		private void SpawnPlayer()
		{
			if(player1 != null)
				World.Remove(player1);

			player1 = World.Create(new Rect(50, 50, 10, 24)).AddTags(Tags.Group1);
			velocity = Vector2.Zero;
		}

		public override void Update(GameTime time)
		{
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;

			UpdatePlatform(platform, delta);
			 
			foreach (var crate in crates)
			{
				crate.Update(delta);
			}

			UpdatePlayer(player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);
		}

		private Vector2 velocity = Vector2.Zero;
		private KeyboardState state;
		private float timeInRed;
		private bool onPlatform;

		private void UpdatePlatform(Box platform, float delta)
		{
			if ((platform.Bounds.xMin < 50 && platformVelocity.X < 0) || (platform.Bounds.xMin > 300 && platformVelocity.X > 0))
			{
				platformVelocity.X *= -1;
			}

			platform.Move(new Vector2(platform.Bounds.xMin + platformVelocity.X * delta, platform.Bounds.Y), Response.Cross);
		}

		private void UpdatePlayer(Box player, float delta, Keys left, Keys up, Keys right, Keys down)
		{
			velocity.Y += delta * 0.001f;
			velocity.X = 0;

			var k = Keyboard.GetState();
			if (k.IsKeyDown(right))
				velocity.X += 0.1f;
			if (k.IsKeyDown(left))
				velocity.X -= 0.1f;
			if (state.IsKeyUp(up) && k.IsKeyDown(up))
				velocity.Y -= 0.5f;

			if (onPlatform)
				velocity += platformVelocity;

			if (timeInRed > 0)
				velocity.Y *= 0.75f;

			// Moving player
			var move = player.Move(player.Bounds.Position + delta * velocity, (collision) =>
			{
				if (collision.Other.HasTag(Tags.Group3))
				{
					return Response.Cross(collision);
				}

				return Response.Slide(collision);
			});

			// Testing if on moving platform
			onPlatform = move.Hits.Any((c) => c.Box.HasTag(Tags.Group4));

			// Testing if on ground
			if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group4, Tags.Group2, Tags.Group5) && (c.Normal.Y < 0)))
			{
				velocity.Y = 0;
			}

			var pushedCrateCollision = move.Hits.FirstOrDefault((c) => c.Box.HasTag(Tags.Group5));
			if (pushedCrateCollision != null)
			{
				var pushedCrate = pushedCrateCollision.Box.Data as Crate;
				var n = pushedCrateCollision.Normal;
				pushedCrate.velocity = new Vector2(n.X * n.X,n.Y * n.Y) * velocity;
			}

			// Testing if in red water
			if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group3)))
			{
				timeInRed += delta;
				if (timeInRed > 3000)
					SpawnPlayer();
			}
			else
			{
				timeInRed = 0;
			}

			player.Data = velocity;

			state = k;
		}
	}
}

