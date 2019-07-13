using System;
using System.Diagnostics;
using System.Linq;
using Humper.Responses;
using JetBrains.Profiler.Api;
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
        GraphicsDeviceManager _graphics;
        SpriteBatch           _spriteBatch;
        private SpriteFont    _font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            IsFixedTimeStep = false;

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
            IsFixedTimeStep = false;
            base.Initialize();
        }

        private IScene _scene;

        private Type[] _scenes = new[]
        {
            typeof(TopdownScene),
            typeof(PlatformerScene),
            typeof(ParticlesScene),
            typeof(PreviewScene),
        };

        private int _sceneIndex = -1;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");
        }

        private void NextScene()
        {
            _sceneIndex = (_sceneIndex + 1) % _scenes.Length;
            _scene = (IScene)Activator.CreateInstance(_scenes[_sceneIndex]);
            _scene.LoadContent(Content);
            _scene.Initialize();
        }

        private KeyboardState _state;
        private float         _updateMs;
        private float         _drawMs;

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
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif
            if(Keyboard.GetState().IsKeyDown(Keys.Enter) && _state.IsKeyUp(Keys.Enter))
                NextScene();
            _state = Keyboard.GetState();

            var sw = Stopwatch.StartNew();
                _scene.Update(gameTime);
            sw.Stop();
            _updateMs = sw.ElapsedMilliseconds;

            base.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            var sw = Stopwatch.StartNew();

            _graphics.GraphicsDevice.Clear(new Color(44, 45, 51));

            _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

            _scene.Draw(_spriteBatch);

            _spriteBatch.DrawString(_font, _scene.Message, new Vector2(20, 20), new Color(Color.White, 0.5f));

            _spriteBatch.DrawString(_font, $"ms:{_updateMs:###0} " +
                                           $"fps:{1000 / (_updateMs + 0.001f):00.0}",
                                    new Vector2(20, 80), new Color(Color.Red, 0.5f));

            _spriteBatch.DrawString(_font, $"ms:{_drawMs:###0} " +
                                           $"fps:{1000 / (_drawMs + 0.001f):00.0}",
                                    new Vector2(20, 100), new Color(Color.Orange, 0.5f));

            _spriteBatch.DrawString(_font, $"monogame {1 / gameTime.ElapsedGameTime.TotalSeconds:##.0}",
                                    new Vector2(20, 120), new Color(Color.Yellow, 0.5f));

            _spriteBatch.End();
            
            sw.Stop();
            base.Draw(gameTime);

            _drawMs = sw.ElapsedMilliseconds;
        }
    }
}