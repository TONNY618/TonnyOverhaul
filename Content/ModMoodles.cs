using System.IO;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using TonnyOverhaul.Statuses;

namespace TonnyOverhaul.Content;

public static class ModMoodles {
	public static void Register() {
		var glowSprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "Moodles", "GlowMoodle.png"), 100f);
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
				Name = LocaleRegistry.Get("moodle", "toGlow", "Bioluminescent glow"),
				Description = string.Format(
					LocaleRegistry.Get("moodle", "toGlowdsc", "You emit light ({0}s left, stacks: {1}). +Max Speed, +Heart Rate."),
					status.RemainingSeconds.ToString("0.0"),
					status.Stacks
				),
				Important = true,
				Critical = false,
			};
		});

		var irritationSprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "Moodles", "IrritationMoodle.png"), 100f);
		MoodleRegistry.RegisterLimb<GlassShardsLimbStatus>((limb, status) => {
			if (status.IrritationTimer <= 0f)
				return null;

			return new StatusMoodleDefinition {
				Intensity = status.IrritationTimer > 10f ? 2 : 1,
				IconSprite = irritationSprite,
				Name = string.Format(
					LocaleRegistry.Get("moodle", "toGlassIrritation", "Glass irritation ({0})"),
					limb.fullName
				),
				Description = string.Format(
					LocaleRegistry.Get("moodle", "toGlassIrritationdsc", "Shards are irritating your {0} ({1}s)."),
					limb.fullName.ToLower(),
					status.IrritationTimer.ToString(".0")
				),
				Important = false,
				Critical = false,
			};
		});
	}
}
