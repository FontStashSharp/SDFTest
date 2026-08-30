using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SDFTest.MonoGame;
using StbRectPackSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace SDFTest
{
	public class TestGame : Game
	{
		private const int FontSize = 32;
		private const int GlyphPad = 2;

		private readonly GraphicsDeviceManager _graphics;
		private Packer _packer;
		private StbTrueTypeSharpSource _fontSource;
		private Texture2D _atlas;
		private readonly Dictionary<char, FontGlyph> _letters = new Dictionary<char, FontGlyph>();
		private SpriteBatch _spriteBatch;
		private bool _animatedScaling = false;

		private float Scale { get; set; }

		public TestGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800,
			};
			Window.AllowUserResizing = true;

			IsMouseVisible = true;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			byte[] data;
			using (var stream = File.OpenRead("Assets/DroidSans.ttf"))
			{
				data = stream.ToByteArray();
			}

			_fontSource = new StbTrueTypeSharpSource(data);
			_atlas = new Texture2D(GraphicsDevice, 1024, 1024);
			_packer = new Packer(_atlas.Width, _atlas.Height);
			_spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			KeyboardUtils.Begin();

			if (KeyboardUtils.IsPressed(Keys.LeftShift))
			{
				_animatedScaling = !_animatedScaling;
			}

			KeyboardUtils.End();
		}

		private FontGlyph GetGlyph(char c)
		{
			FontGlyph glyph;
			if (_letters.TryGetValue(c, out glyph))
			{
				return glyph;
			}

			var g = _fontSource.GetGlyphId(c);
			if (g == null)
			{
				return null;
			}

			int left, top, width, height;
			var buffer = _fontSource.RasterizeGlyphSDF(g.Value, FontSize, out left, out top, out width, out height);
			if (buffer == null)
			{
				return null;
			}

			int advance, x0, y0, x1, y1;
			_fontSource.GetGlyphMetrics(g.Value, FontSize, out advance, out x0, out y0, out x1, out y1);

			glyph = new FontGlyph
			{
				Codepoint = c,
				Id = g.Value,
				RenderOffset = new Point(left, top),
				Size = new Point(width, height),
				XAdvance = advance
			};

			var pack = _packer.PackRect(width + 2 * GlyphPad, height + 2 * GlyphPad, null);

			glyph.TextureOffset = new Point(pack.X + GlyphPad, pack.Y + GlyphPad);
			glyph.Size = new Point(width, height);

			// Convert to color
			var colorBuffer = new Color[width * height];
			for (var i = 0; i < colorBuffer.Length; ++i)
			{
				// Premultiply alpha
				var b = buffer[i];
				colorBuffer[i] = new Color(b, b, b, b);
			}

			// Load to texture
			var bounds = glyph.TextureRectangle;
			_atlas.SetData(0, bounds, colorBuffer, 0, bounds.Width * bounds.Height);

			_letters[c] = glyph;

			return glyph;
		}

		private void DrawString(string text, Vector2 position, Color color)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			var effect = Resources.GetEffect(GraphicsDevice, effectType: EffectType.Shadow, superSamling: true);

			effect.Parameters["cShadowColor"].SetValue(new Vector4(0, 0, 0, 1));
			effect.Parameters["cShadowOffset"].SetValue(new Vector2(5, 5));

			// effect.Parameters["cStrokeColor"].SetValue(new Vector4(1, 0, 0, 1));

			var vp = GraphicsDevice.Viewport;

			_spriteBatch.Begin(effect: effect, blendState: BlendState.NonPremultiplied);
			for (var i = 0; i < text.Length; ++i)
			{
				var glyph = GetGlyph(text[i]);
				if (glyph == null)
				{
					continue;
				}

				_spriteBatch.Draw(_atlas,
					new Vector2((int)position.X + glyph.RenderOffset.X * Scale, (int)position.Y + glyph.RenderOffset.Y * Scale),
					glyph.TextureRectangle,
					Color.White,
					0f,
					Vector2.Zero,
					scale: new Vector2(Scale),
					SpriteEffects.None,
					0);

				position.X += glyph.XAdvance * Scale;
			}

			_spriteBatch.End();
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here
			Scale = _animatedScaling
				? 3 + 2f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * .5f)
				: 1.0f;

			DrawString("Hello, World!", new Vector2(100, 200), Color.White);

			_spriteBatch.Begin();

			_spriteBatch.Draw(_atlas, new Vector2(0, 500), Color.White);

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
