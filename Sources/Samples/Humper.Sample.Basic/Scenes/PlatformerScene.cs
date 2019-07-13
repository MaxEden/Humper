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
				this._box = box.AddTags(Tags.Group5);
				this._box.Data = this;
			}

			private Box _box;

			public Vector2 Velocity;

			private bool _inWater;

			public void Update(float delta)
			{
				Velocity.Y += delta * 0.001f;


				if (_inWater)
					Velocity.Y *= 0.5f;

				var move = _box.Move(_box.Bounds.Position + delta * Velocity, (collision) =>
				{
					if (collision.Other.HasTag(Tags.Group3))
					{
						return Response.Cross(collision);
					}

					return Response.Slide(collision);
				});

				_inWater = (move.Hits.Any((c) => c.Box.HasTag(Tags.Group3)));


				Velocity.X *= 0.85f;

				// Testing if on ground
				if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group2) && (c.Normal.Y < 0)))
				{
					Velocity.Y = 0;
					Velocity.X *= 0.85f;
				}

			}
		}

		public PlatformerScene()
		{
		}

		private Box _player1, _platform;

		private Crate[] _crates;

		private Vector2 _platformVelocity = Vector2.right * 0.05f;

		public override void Initialize()
		{
			//World = new World(new Grid(1024, 700));
			World = new World(new DynamicTree());

			SpawnPlayer();

			_platform = World.Create(new Rect(0, 200, 100, 20)).AddTags(Tags.Group4);

			_crates = new[]
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
			if(_player1 != null)
				World.Remove(_player1);

			_player1 = World.Create(new Rect(50, 50, 10, 24)).AddTags(Tags.Group1);
			_velocity = Vector2.Zero;
		}

		public override void Update(GameTime time)
		{
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;

			UpdatePlatform(_platform, delta);
			 
			foreach (var crate in _crates)
			{
				crate.Update(delta);
			}

			UpdatePlayer(_player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);
		}

		private Vector2 _velocity = Vector2.Zero;
		private KeyboardState _state;
		private float _timeInRed;
		private bool _onPlatform;

		private void UpdatePlatform(Box platform, float delta)
		{
			if ((platform.Bounds.xMin < 50 && _platformVelocity.X < 0) || (platform.Bounds.xMin > 300 && _platformVelocity.X > 0))
			{
				_platformVelocity.X *= -1;
			}

			platform.Move(new Vector2(platform.Bounds.xMin + _platformVelocity.X * delta, platform.Bounds.Y), Response.Cross);
		}

		private void UpdatePlayer(Box player, float delta, Keys left, Keys up, Keys right, Keys down)
		{
			_velocity.Y += delta * 0.001f;
			_velocity.X = 0;

			var k = Keyboard.GetState();
			if (k.IsKeyDown(right))
				_velocity.X += 0.1f;
			if (k.IsKeyDown(left))
				_velocity.X -= 0.1f;
			if (_state.IsKeyUp(up) && k.IsKeyDown(up))
				_velocity.Y -= 0.5f;

			if (_onPlatform)
				_velocity += _platformVelocity;

			if (_timeInRed > 0)
				_velocity.Y *= 0.75f;

			// Moving player
			var move = player.Move(player.Bounds.Position + delta * _velocity, (collision) =>
			{
				if (collision.Other.HasTag(Tags.Group3))
				{
					return Response.Cross(collision);
				}

				return Response.Slide(collision);
			});

			// Testing if on moving platform
			_onPlatform = move.Hits.Any((c) => c.Box.HasTag(Tags.Group4));

			// Testing if on ground
			if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group4, Tags.Group2, Tags.Group5) && (c.Normal.Y < 0)))
			{
				_velocity.Y = 0;
			}

			var pushedCrateCollision = move.Hits.FirstOrDefault((c) => c.Box.HasTag(Tags.Group5));
			if (pushedCrateCollision != null)
			{
				var pushedCrate = pushedCrateCollision.Box.Data as Crate;
				var n = pushedCrateCollision.Normal;
				pushedCrate.Velocity = new Vector2(n.X * n.X,n.Y * n.Y) * _velocity;
			}

			// Testing if in red water
			if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group3)))
			{
				_timeInRed += delta;
				if (_timeInRed > 3000)
					SpawnPlayer();
			}
			else
			{
				_timeInRed = 0;
			}

			player.Data = _velocity;

			_state = k;
		}
	}
}

