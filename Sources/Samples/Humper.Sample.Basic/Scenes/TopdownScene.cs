using Humper.Base;
using Mandarin.Common.Misc;
using Microsoft.Xna.Framework;	
using System;
using Humper.Responses;
using Microsoft.Xna.Framework.Input;
using Vector2 = Mandarin.Common.Misc.Vector2;

namespace Humper.Sample.Basic
{


	public class TopdownScene : WorldScene
	{
		public TopdownScene()
		{
		}

		private Box _player1, _player2;

		public override void Initialize()
		{
			//World = new World(new Grid(1024, 700));
			World = new World(new DynamicTree());

			_player1 = World.Create(new Rect(50, 50, 24, 24)).AddTags(Tags.Group3);
			_player2 = World.Create(new Rect(100, 50, 24, 24)).AddTags(Tags.Group1);

			_player1.IsActive = true;
			_player2.IsActive = true;

			// Map
			World.Create(new Rect(100, 100, 150, 20)).AddTags(Tags.Group2);
			World.Create(new Rect(180, 140, 200, 200)).AddTags(Tags.Group2);
			World.Create(new Rect(190, 20, 80, 400)).AddTags(Tags.Group2);
		}

		public override void Update(GameTime time)
		{
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;

			UpdatePlayer(_player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);
			UpdatePlayer(_player2, delta, Keys.F, Keys.T, Keys.H, Keys.G);
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
				velocity.Y -= 0.1f;
			if (k.IsKeyDown(up)) 
				velocity.Y += 0.1f;
			
			var move = player.Move(player.Bounds.Position + delta * velocity, Response.Slide);
			//var move = player.Move(player.Bounds.Position + new Vector2(0.05f,0.1f) *3f, (collision) => CollisionResponses.Slide);
		}

	}
}

