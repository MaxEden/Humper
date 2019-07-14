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
		public enum CollisionResponses
		{
			None,
			Touch,
			Cross,
			Slide,
			Bounce
		}
		public PreviewScene()
		{
		}

		private Rect _origin;

		private Rect _goal;

		private Rect _destination;

		private Rect _collision;

		private Rect _other;

		private Rect _normal;

		private bool _moveDestination, _isMoving;
		private Rect _cursor, _selected;

		private CollisionResponses[] _values = Enum.GetValues(typeof(CollisionResponses)) as CollisionResponses[];
		private int _response = 0;

		private KeyboardState _previous;
		private int _height;

		public string Message 
		{ 
			get 
			{
				var moving = _moveDestination ? nameof(_goal) : nameof(_origin);
				var changed = !_moveDestination ? nameof(_goal) : nameof(_origin);
				var r = _values[_response];
				return $"[N]: select {changed} box\n[Space]: move selected {moving} box\n[R]: Change collision mode ({r})";
			}
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(_origin, color: Color.Green, fillOpacity: 0.3f);
			sb.Draw(_goal, color: Color.Red, fillOpacity: 0.3f);
			sb.Draw(_other, color: new Color(165, 155, 250), fillOpacity: 0.3f);
			sb.Draw(_collision, color: Color.Orange, fillOpacity: 0.0f);
			sb.Draw(_normal, color: Color.Orange, fillOpacity: 0.3f);
			sb.Draw(_destination, color: Color.Green, fillOpacity: 0.0f);
			var s = _destination.Size / 10;
			sb.Draw(new Rect(_destination.Center - (s / 2), s), color: Color.Green, fillOpacity: 0.0f);
			sb.Draw(_cursor, color: Color.White, fillOpacity: 0.0f);
			sb.Draw(_selected, color: Color.White, fillOpacity: 0.5f);
			_height = sb.GraphicsDevice.Viewport.Height;
		}

		public void Initialize()
		{
			_origin = new Rect(0, 0, 100, 100);
			_goal = new Rect(400, 300, 100, 100);
			_selected = new Rect(-3, -3 , 6, 6);

			_other = new Rect(200, 200, 500, 120);
		}


		public void Update(GameTime time)
		{
			var state = Keyboard.GetState();
			if (_previous.IsKeyUp(Keys.N) && state.IsKeyDown(Keys.N))
			{
				_moveDestination = !_moveDestination;
				_selected.Position = (_moveDestination ? _goal.Position : _origin.Position) - _selected.Size / 2;
			}

			if (_previous.IsKeyUp(Keys.R) && state.IsKeyDown(Keys.R))
			{
				
				_response = (_response + 1) % _values.Length;
			}

			_previous = state;

			_isMoving = state.IsKeyDown(Keys.Space);
			var m = Mouse.GetState().Position;
			m.Y = _height - m.Y;

			var pos = new Vector2(m.X, m.Y);
			var size = _isMoving ? 18 : 6;
			_cursor = new Rect(m.X - size/2, m.Y - size/2,size, size);
			
			if (_isMoving)
			{
				_selected.Position = pos - _selected.Size / 2;
				
				if (_moveDestination)
				{
					_goal.Position = pos;
				}
				else
				{
					_origin.Position = pos;
				}
			}

			// Calculate collision
			var hit = Hit.Resolve(_origin, _goal, _other);
			var r = _values[_response];

			if (hit.IsHit && r != CollisionResponses.None)
			{
				_collision = new Rect(hit.Position, _origin.Size);
				_normal = new Rect(_collision.Center + hit.Normal * 50, new Vector2(5, 5));

				// Destination
				var collisionPoint = new Collision()
				{
					Origin = _origin,
					Goal = _goal,
					Hit = hit,
				};

				switch(r)
				{

					case CollisionResponses.None:
						break;
					case CollisionResponses.Touch:
						_destination = Response.Touch(collisionPoint).Value;
						break;
					case CollisionResponses.Cross:
						_destination = Response.Cross(collisionPoint).Value;
						break;
					case CollisionResponses.Slide:
						_destination = Response.Slide(collisionPoint).Value;
						break;
					case CollisionResponses.Bounce:
						_destination = Response.Bounce(collisionPoint).Value;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				_collision = new Rect();
				_normal = new Rect();
				_destination = _goal;
			}
				
		}

		public void LoadContent(ContentManager content)
		{

		}
	}
}

