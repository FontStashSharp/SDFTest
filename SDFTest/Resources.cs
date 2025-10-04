using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace SDFTest.MonoGame
{
	internal enum EffectType
	{
		None,
		Stroke,
		Shadow
	}

	internal static class Resources
	{
#if FNA
		private const string EffectsResourcePath = "Effects.FNA.bin";
#elif MONOGAME
		private const string EffectsResourcePath = "Effects.MonoGameOGL.bin";
#endif
		private static AssetManager _assetManager = AssetManager.CreateResourceAssetManager(typeof(Resources).Assembly, EffectsResourcePath);


		private static Effect GetEffect(GraphicsDevice graphicsDevice, string name, Dictionary<string, string> defines)
		{
			name = Path.ChangeExtension(name, "efb");
			return _assetManager.LoadEffect(graphicsDevice, name, defines);
		}

		public static Effect GetEffect(GraphicsDevice graphicsDevice, EffectType effectType = EffectType.None, bool superSamling = false)
		{
			var defines = new Dictionary<string, string>();

			switch (effectType)
			{
				case EffectType.Stroke:
					defines["EFFECTSTROKE"] = "1";
					break;
				case EffectType.Shadow:
					defines["EFFECTSHADOW"] = "1";
					break;
			}

			if (superSamling)
			{
				defines["SUPERSAMPLING"] = "1";
			}

			return GetEffect(graphicsDevice, "Text", defines);
		}
	}
}
