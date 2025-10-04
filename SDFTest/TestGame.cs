using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StbRectPackSharp;
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
			for(var i = 0; i < colorBuffer.Length; ++i)
			{
				// Premultiply alpha
				var b = buffer[i];
				colorBuffer[i] = new Color(b, b, b, b);
			}

			// Load to texture
			var bounds = glyph.TextureRectangle;
			_atlas.SetData(0, bounds,colorBuffer, 0, bounds.Width * bounds.Height);

			_letters[c] = glyph;

			return glyph;
		}

		private void DrawString(string text, Vector2 position, Color color)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			for(var i = 0; i < text.Length; ++i)
			{
				var glyph = GetGlyph(text[i]);
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// TODO: Add your drawing code here
			DrawString("Hello, World!", new Vector2(50, 100), Color.White);

			_spriteBatch.Begin();

			_spriteBatch.Draw(_atlas, new Vector2(0, 500), Color.White);

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
