using Humper.Base;
using Microsoft.Xna.Framework;
using System;
using Humper.Responses;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Collections.Generic;
using Mandarin.Common.Misc;
using Vector2 = Mandarin.Common.Misc.Vector2;

namespace Humper.Sample.Basic
{

    public class ParticlesScene : WorldScene
	{
		public class Particle
		{
			public static Random Random = new Random();

			public Particle(Box box)
			{
				Box = box;
				Velocity = new Vector2((float)Random.NextDouble() * 0.1f, 0);
			}

			public Box Box { get; set; }

			public Vector2 Velocity { get; set; }

			public void Update(float delta)
			{
				Velocity = Velocity + Vector2.down * delta * 0.001f;

				var move = Box.Move(delta*Velocity + Box.Bounds.Position, Response.Slide);

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

		private Box _player1;

		private Vector2 _platformVelocity = Vector2.right * 0.05f;

		public override void Initialize()
		{
			World = new World(new Grid(1024, 700, 20));
			//World = new World(new DynamicTree());

			SpawnPlayer();

			// Map
			World.Create(new Rect(0, 0, 1024, 20)).AddTags(Tags.Group2);
			World.Create(new Rect(0, 20, 20, 660)).AddTags(Tags.Group2);
			World.Create(new Rect(1004, 20, 20, 660)).AddTags(Tags.Group2);
			World.Create(new Rect(0, 680, 1024, 20)).AddTags(Tags.Group2);

			SpawnBoxes();

		}
		private void SpawnBoxes()
		{
			int maxBoxes = 100;
			int i = 0;
			int width = 15;
			for(int x = 24; x < 1000; x += width * 2)
			{
				for(int y = 140; y < 500; y += width * 5)
				{
					var box = World.Create(new Rect(
						                       x,
						                       y,
						                       (float)(Particle.Random.NextDouble() * width) + 2f,
						                       (float)(Particle.Random.NextDouble() * width) + 2f
					                       )).AddTags(Tags.Group3);
					_particles.Add(new Particle(box));
					i++;
					if(i > maxBoxes) return;
				}
			}
		}

		private void SpawnPlayer()
		{
			if (_player1 != null)
				World.Remove(_player1);

			_player1 = World.Create(new Rect(50, 100, 50, 30)).AddTags(Tags.Group1);
			_velocity = Vector2.Zero;
		}

		public override void Update(GameTime time)
		{
			
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;
			//delta = 100;

			foreach (var p in _particles)
			{
				p.Update(delta);
			}

			//return;
			UpdatePlayer(_player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);
		}

		private Vector2 _velocity = Vector2.Zero;
		private KeyboardState _state;
		private List<Particle> _particles = new List<Particle>();

		private void UpdatePlayer(Box player, float delta, Keys left, Keys up, Keys right, Keys down)
		{
			_velocity.Y -= delta * 0.001f;
			_velocity.X = 0;

			var k = Keyboard.GetState();
			if (k.IsKeyDown(right))
				_velocity.X += 0.1f;
			if (k.IsKeyDown(left))
				_velocity.X -= 0.1f;
			if (_state.IsKeyUp(up) && k.IsKeyDown(up))
				_velocity.Y += 0.5f;

			if(_state.IsKeyUp(Keys.R) && k.IsKeyDown(Keys.R))
			{
				SpawnBoxes();
			}

			// Moving player
			var move = player.Move(player.Bounds.Position + delta * _velocity, Response.Slide);

			// Testing if on ground
			if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group2, Tags.Group3) && (c.Normal.Y < 0)))
			{
				_velocity.Y = 0;
			}

			_state = k;
		}
	}
}

