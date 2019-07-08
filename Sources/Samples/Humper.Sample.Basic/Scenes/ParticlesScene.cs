using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Humper.Base.Vector2;

namespace Humper.Sample.Basic
{
	using System;
	using Responses;
	using Microsoft.Xna.Framework.Input;
	using System.Linq;
	using System.Collections.Generic;

	public class ParticlesScene : WorldScene
	{
		public class Particle
		{
			public static Random random = new Random();

			public Particle(IBox box)
			{
				this.Box = box;
				this.Velocity = new Vector2((float)random.NextDouble() * 0.1f, 0);
			}

			public IBox Box { get; set; }

			public Vector2 Velocity { get; set; }

			public void Update(float delta)
			{
				Velocity = Velocity + Vector2.UnitY * delta * 0.001f;

				var move = this.Box.Move(
					delta*Velocity + Box.Bounds.Location, (collision) =>
				{
					return CollisionResponses.Bounce;
				});

				// Testing if on ground
				if (move.Hits.Any((c) => (c.Normal.Y < 0)))
				{
					Velocity = Velocity * new Vector2(1,-1);
				}

				// Testing if on wall
				if (move.Hits.Any((c) => (Math.Abs(c.Normal.X) > Constants.Threshold)))
				{
					Velocity = Velocity * new Vector2(-1, 1);
				}
			}
		}

		public ParticlesScene()
		{
		}

		private IBox player1;

		private Vector2 platformVelocity = Vector2.UnitX * 0.05f;

		public override void Initialize()
		{
			this.World = new World(1024, 700);

			this.SpawnPlayer();

			// Map
			this.World.Create(new RectangleF(0, 0, 1024, 20)).AddTags(Tags.Group2);
			this.World.Create(new RectangleF(0, 20, 20, 660)).AddTags(Tags.Group2);
			this.World.Create(new RectangleF(1004, 20, 20, 660)).AddTags(Tags.Group2);
			this.World.Create(new RectangleF(0, 680, 1024, 20)).AddTags(Tags.Group2);

			int maxBoxes = 500;
			int width = 2;
			for (int x = 24; x < 1000; x+=width*2)
			{
				for(int y = 40; y < 500; y+= width*5)
				{
					var box = this.World.Create(new RectangleF(
						                            x,
						                            y,
						                            (float)(Particle.random.NextDouble()*width) + 0.001f,
						                            (float)(Particle.random.NextDouble()*width) + 0.001f
						)).AddTags(Tags.Group3);
					this.particles.Add(new Particle(box));
					if(World.Boxes>maxBoxes) return;
				}
			}

		}

		private void SpawnPlayer()
		{
			if (this.player1 != null)
				this.World.Remove(this.player1);

			this.player1 = this.World.Create(new RectangleF(50, 100, 50, 30)).AddTags(Tags.Group1);
			this.velocity = Vector2.Zero;
		}

		public override void Update(GameTime time)
		{
			
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;

			foreach (var p in this.particles)
			{
				p.Update(delta);
			}

			return;
			UpdatePlayer(this.player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);
		}

		private Vector2 velocity = Vector2.Zero;
		private KeyboardState state;
		private float timeInRed;
		private List<Particle> particles = new List<Particle>();

		private void UpdatePlayer(IBox player, float delta, Keys left, Keys up, Keys right, Keys down)
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

			// Moving player
			var move = player.Move(player.Bounds.Location + delta * velocity, (collision) =>
			{
				return CollisionResponses.Slide;
			});

			// Testing if on ground
			if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group2, Tags.Group3) && (c.Normal.Y < 0)))
			{
				velocity.Y = 0;
			}

			state = k;
		}
	}
}

