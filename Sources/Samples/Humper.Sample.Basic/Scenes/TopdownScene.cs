using Humper.Base;
using Microsoft.Xna.Framework;
using Vector2 = Humper.Base.Vector2;

namespace Humper.Sample.Basic
{
	using System;
	using Responses;
	using Microsoft.Xna.Framework.Input;

	public class TopdownScene : WorldScene
	{
		public TopdownScene()
		{
		}

		private Box player1, player2;

		public override void Initialize()
		{
			World = new World(new DynamicTree());

			player1 = World.Create(new Rect(50, 50, 24, 24)).AddTags(Tags.Group1);
			player2 = World.Create(new Rect(100, 50, 24, 24)).AddTags(Tags.Group1);

			// Map
			World.Create(new Rect(100, 100, 150, 20)).AddTags(Tags.Group2);
			World.Create(new Rect(180, 140, 200, 200)).AddTags(Tags.Group2);
			World.Create(new Rect(190, 20, 80, 400)).AddTags(Tags.Group2);
		}

		public override void Update(GameTime time)
		{
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;

			UpdatePlayer(player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);
			UpdatePlayer(player2, delta, Keys.F, Keys.T, Keys.H, Keys.G);
		}

		private void UpdatePlayer(Box player, float delta, Keys left, Keys up, Keys right, Keys down)
		{
			var velocity = Vector2.Zero;

			var k = Keyboard.GetState();
			if (k.IsKeyDown(right)) 
				velocity.X += 0.1f;
			if (k.IsKeyDown(left)) 
				velocity.X -= 0.1f;
			if (k.IsKeyDown(down)) 
				velocity.Y += 0.1f;
			if (k.IsKeyDown(up)) 
				velocity.Y -= 0.1f;
			
			var move = player.Move(player.Bounds.Location + delta * velocity, (collision) => CollisionResponses.Slide);

		}

	}
}

