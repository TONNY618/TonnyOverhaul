using System.IO;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using TonnyOverhaul.Components;
using TonnyOverhaul.Core;
using TonnyOverhaul.Minigames;
using TonnyOverhaul.Statuses;
using UnityEngine;

namespace TonnyOverhaul.Content;

public static class ModItems {
	public static void Register() {
		ItemRegistry.Register("toStrangeLamp", new CustomItemInfo {
			fullName = "Strange lamp",
			description = "When eaten, it makes a strange sound and makes you glow.",
			weight = 0.5f,
			value = 60,
			category = "food",
			rec = new Recognition(5),
			usable = true,
			useAction = (body, item) => {
				item.condition -= 1f;
				var glow = body.GetStatus<LampGlowStatus>();
				glow.RemainingSeconds += 5f;
				glow.Stacks++;

				if (ModAssets.CustomSound != null)
					ModAssets.AudioSource.PlayOneShot(ModAssets.CustomSound);
			},
			usableOnLimb = true,
			useLimbAction = (limb, item) => {
				limb.shrapnel += 5;
				limb.skinHealth -= 30;
				limb.muscleHealth -= 5;
				limb.pain += 50;
				limb.bleedAmount += 15;
				item.condition -= 1f;
				Sound.Play("bulbbreak", limb.transform.position);

				limb.GetStatus<GlassShardsLimbStatus>().IrritationTimer = 15f;
			},
		}, ModAssets.LampSprite);

		ItemRegistry.Register("toPortableLamp", new CustomItemInfo {
			fullName = "Portable lamp",
			description = "A battery-fed lamp for the fuzzy cave explorer.",
			category = "utility",
			tags = "belttool",
			value = 22,
			weight = 0.8f,
			rec = new Recognition(5),
			decayMinutes = 180f,
			Battery = new BatteryProperties {
				Preset = BatteryItem.BatteryPreset.Medium,
				SpawnWithBattery = false,
			},
			Light = new LightProperties {
				Intensity = 0.75f,
				Color = Color.white,
				FalloffIntensity = 0.5f,
				PointLightOuterRadius = 7.5f,
				PointLightInnerRadius = 0f,
				PointLightOuterAngle = 360f,
				PointLightInnerAngle = 360f,
				LightType = CustomLightType.Point,
				Rotation = 0f,
				Offset = Vector2.zero,
			},
			SpawnFrequency = 1,
			useAction = (body, item) => {
				if (item.battery == null || !item.battery.hasBattery) {
					PlayerCamera.main?.PlayUISound(PlayerCamera.UISoundType.Deny);
					return;
				}

				CUCoreMinigames.TryStartDefinition(() => new LampTuneMinigame(), item);
			},
		}.AddSpawnComponent<PortableLampWorldBehaviour>(), ModAssets.LampSprite);

		ItemRegistry.Register("toCryoliteChunk", new CustomItemInfo {
			fullName = "Cryolite chunk",
			description = "Na3[AlF6]. A natural mineral that can be processed into a small amount of liquid cryolite.",
			value = 1,
			weight = 1f,
			rec = new Recognition(8),
		}, AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "Items", "CryoliteChunk.png"), 10f));
	}
}
