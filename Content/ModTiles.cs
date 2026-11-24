using System.IO;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;

namespace TonnyOverhaul.Content;

public static class ModTiles {
	public static void Register() {
		var cryoSprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "Tiles", "CryoliteTile.png"));
		TileRegistry.Register("toCryolite", new CustomTileDefinition {
			Name = "Stone cryolite",
			Sprite = cryoSprite,

			Health = 500f,
			HitSound = "rock",
			StepSound = "Rock",

			SleepQuality = Body.SleepQuality.Bad,
			Slippery = false,
			Metallic = false,
			Toxicity = 0f,

			SpawnAmount = 0.8f,
			GenerationStyle = TileGenerationStyle.Vein | TileGenerationStyle.Inner,

			Drops = [
				BuildingEntityRegistry.AddDrop("toCryoliteChunk"),
			],
		});
	}
}
