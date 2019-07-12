using System;
using System.Diagnostics;
using System.Linq;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Humper.Sample.Basic
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		private SpriteFont font;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			Window.AllowUserResizing = true;
			_stopwatch = new Stopwatch();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			NextScene();

			base.Initialize();

		}

		private IScene scene;

		private Type[] scenes = new[] 
		{ 
			typeof(TopdownScene),
			typeof(PlatformerScene),
			typeof(ParticlesScene),
			typeof(PreviewScene),
		};

		private int sceneIndex = -1;

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			font = Content.Load<SpriteFont>("font");
		}

		private void NextScene()
		{
			sceneIndex = (sceneIndex + 1) % scenes.Length;
			scene = (IScene) Activator.CreateInstance(scenes[sceneIndex]);
			scene.LoadContent(Content);
			scene.Initialize();
		}

		private KeyboardState state;
		private Stopwatch _stopwatch;

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
#endif
			if (Keyboard.GetState().IsKeyDown(Keys.Enter) && state.IsKeyUp(Keys.Enter))
				NextScene();
			state = Keyboard.GetState();

			
			_stopwatch.Restart();
			scene.Update(gameTime);
			_stopwatch.Stop();

			base.Update(gameTime);
		}



		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(new Color(44,45,51));

			spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

			scene.Draw(spriteBatch);

			spriteBatch.DrawString(font, scene.Message, new Vector2(20, 20), new Color(Color.White, 0.5f));

			spriteBatch.DrawString(font, $"ms:{_stopwatch.Elapsed.TotalMilliseconds:####} fps:{1/(_stopwatch.Elapsed.TotalSeconds + 0.0001f):00.0} monogame {1 / gameTime.ElapsedGameTime.TotalSeconds:##.0}", 
			                       new Vector2(20, 80), new Color(Color.Red, 0.5f));

			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}

