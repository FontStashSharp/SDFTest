using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDFTest
{
	public class FontGlyph
	{
		public int Codepoint;
		public int Id;
		public int XAdvance;
		public Texture2D Texture;
		public Point RenderOffset;
		public Point TextureOffset;
		public Point Size;

		public bool IsEmpty
		{
			get
			{
				return Size.X == 0 || Size.Y == 0;
			}
		}

		public Rectangle TextureRectangle => new Rectangle(TextureOffset.X, TextureOffset.Y, Size.X, Size.Y);
		public Rectangle RenderRectangle => new Rectangle(RenderOffset.X, RenderOffset.Y, Size.X, Size.Y);
	}
}
