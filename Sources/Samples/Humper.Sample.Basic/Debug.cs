using System;
using Humper.Base;
using Mandarin.Common.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Humper.Sample.Basic
{
	public static class Debug
	{
		private static Texture2D _pixel;
		private static Texture2D GetPixel(this SpriteBatch sb)
		{
			if (_pixel == null)
			{
				_pixel = new Texture2D(sb.GraphicsDevice, 1, 1);
				_pixel.SetData(new Color[] { Color.White });
			}

			return _pixel;
		}

		public static Rectangle ToRectangle(this SpriteBatch spriteBatch, Rect r)
		{
			return new Rectangle((int)r.X,spriteBatch.GraphicsDevice.Viewport.Height - (int)(r.Y + r.Height),(int)r.Width,(int)r.Height);
		}

		public static void Draw(this SpriteBatch spriteBatch, Rect rect, Color color, float fillOpacity)
		{
			//fillOpacity = 1;
			var fill = new Color(color, fillOpacity);
			spriteBatch.DrawFill(rect, fill);
			spriteBatch.DrawStroke(rect, color);
		}

		public static void DrawFill(this SpriteBatch spriteBatch, Rect rect, Color fill)
		{
			spriteBatch.Draw(spriteBatch.GetPixel(), destinationRectangle: spriteBatch.ToRectangle(rect), color: fill);
		}

		public static void DrawStroke(this SpriteBatch spriteBatch, Rect rect, Color stroke)
		{
			_pixel = spriteBatch.GetPixel();

			var left = new Rect((int)rect.Left, (int)rect.Bottom, 1,(int) rect.Height);
			var right = new Rect((int)rect.Right, (int)rect.Bottom, 1, (int)rect.Height);
			var top = new Rect((int)rect.Left, (int)rect.Top, (int)rect.Width, 1);
			var bottom = new Rect((int)rect.Left, (int)rect.Bottom, (int)rect.Width, 1);

			spriteBatch.Draw(_pixel, destinationRectangle: spriteBatch.ToRectangle(left), color: stroke);
			spriteBatch.Draw(_pixel, destinationRectangle: spriteBatch.ToRectangle(right), color: stroke);
			spriteBatch.Draw(_pixel, destinationRectangle: spriteBatch.ToRectangle(top), color: stroke);
			spriteBatch.Draw(_pixel, destinationRectangle: spriteBatch.ToRectangle(bottom), color: stroke);
		}
	}
}

