using System.IO;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using TonnyOverhaul.Statuses;

namespace TonnyOverhaul.Content;

public static class ModMoodles {
	public static void Register() {
		var glowSprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "GlowMoodle.png"), 100f);
		MoodleRegistry.RegisterBody<LampGlowStatus>((body, status) => {
			if (status.RemainingSeconds <= 0f)
				return null;

			var tier = status.Stacks switch {
				>= 5 => 2,
				>= 3 => 1,
				_ => 0,
			};

			return new StatusMoodleDefinition {
				Intensity = tier,
				IconSprite = glowSprite,
				Name = "Bioluminescent glow",
				Description = $"You emit light ({status.RemainingSeconds:0.0}s left, stacks: {status.Stacks}). +Max Speed, +Heart Rate",
				Important = true,
				Critical = false,
			};
		});

		var irritationSprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "IrritationMoodle.png"), 100f);
		MoodleRegistry.RegisterLimb<GlassShardsLimbStatus>((limb, status) => {
			if (status.IrritationTimer <= 0f)
				return null;

			return new StatusMoodleDefinition {
				Intensity = status.IrritationTimer > 10f ? 2 : 1,
				IconSprite = irritationSprite,
				Name = $"Glass irritation ({limb.fullName})",
				Description = $"Shards are irritating your {limb.fullName.ToLower()} ({status.IrritationTimer:0}s).",
				Important = false,
				Critical = false,
			};
		});
	}
}
