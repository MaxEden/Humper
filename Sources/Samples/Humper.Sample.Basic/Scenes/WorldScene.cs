﻿using System;
using Humper.Base;
using Mandarin.Common.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Humper.Sample.Basic
{
	public abstract class WorldScene : IScene
	{
		public WorldScene()
		{
		}

		protected World World { get; set; }
		 
		public virtual string Message
		{
			get
			{
				return $"[Up,Right,Down,Left]: move\n[Space]: show grid; Boxes count:" + World.Boxes;
			} 
		}

		private SpriteBatch spriteBatch;

		private SpriteFont font;

		public virtual void Draw(SpriteBatch sb)
		{
			var b = World.Bounds;
			spriteBatch = sb;
			World.DrawDebug(b, DrawCell, DrawBox, DrawString);

			
			spriteBatch.Draw(new Rect(Mandarin.Common.Misc.Vector2.Zero, Mandarin.Common.Misc.Vector2.One*5f), Color.Red, 1f);
			spriteBatch.Draw(new Rect(Mandarin.Common.Misc.Vector2.up * 100, Mandarin.Common.Misc.Vector2.One*5f), Color.Blue, 1f);
		}

		private void DrawCell(Rect rect, float alpha)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Space))
				spriteBatch.DrawStroke(rect, new Color(Color.White, alpha));
		}

		private void DrawBox(Box box)
		{
			Color color;

			if (box.HasTag(Tags.Group1))
				color = Color.White;
			else if (box.HasTag(Tags.Group3))
				color = Color.Red;
			else if (box.HasTag(Tags.Group4))
				color = Color.Green;
			else if (box.HasTag(Tags.Group5))
				color = Color.Yellow;
			else
				color = new Color(165, 155, 250);

			spriteBatch.Draw(box.Bounds, color, 0.3f);
		}

		public void LoadContent(ContentManager content)
		{
			font = content.Load<SpriteFont>("font");
		}

		private void DrawString(string message, int x, int y, float alpha)
		{
			var size = font.MeasureString(message);
			if (Keyboard.GetState().IsKeyDown(Keys.Space))
				spriteBatch.DrawString(font, message, new Vector2(x - size.X / 2, spriteBatch.GraphicsDevice.Viewport.Height - (y - size.Y / 2)), new Color(Color.White, alpha));
		}

		public abstract void Initialize();


		public abstract void Update(GameTime time);
	}
}

