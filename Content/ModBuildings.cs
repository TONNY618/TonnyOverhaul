using System.IO;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using TonnyOverhaul.Components;
using TonnyOverhaul.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TonnyOverhaul.Content;

public static class ModBuildings {
	public static void Register() {
		BuildingEntityRegistry.Register("toCaveLamp", new CustomBuildingEntityDefinition {
			Name = "Bioluminescent lamp",
			Description = "An odd glowing fixture rooted into the cave floor. Breaks easily.",
			Sprite = ModAssets.LampSprite,
			SortingOrder = 5,

			UseGlowPlantMaterial = true,
			CopyGlowPlantLayer = true,

			Health = 40f,
			RequireGround = true,
			Metallic = false,
			HitSoundReferenceId = "glass",
			BlockFootstepSoundId = 7,

			ItemsDropOnDestroy = [
				BuildingEntityRegistry.AddDrop("toStrangeLamp", chance: 0.6f, conditionMin: 0.5f, conditionMax: 1f),
				BuildingEntityRegistry.AddDrop("bundleofwires", chance: 0.4f),
				BuildingEntityRegistry.AddDrop("glassshards"),
			],

			GenerationStyle = BuildingGenerationStyle.Standard,
			Placement = BuildingPlacementType.Floor,
			Scale = new Vector3(2f, 4f, 1f),
			SurfaceOffset = 1.8f,
			RandomFlip = true,
			SpawnMinPerChunk = 0.25f,
			SpawnMaxPerChunk = 0.5f,

			ConfigureInstance = instance => {
				var light = instance.AddComponent<Light2D>();
				light.lightType = Light2D.LightType.Point;
				light.color = new Color(1f, 0.95f, 0.7f);
				light.intensity = 1f;
				light.falloffIntensity = 0.5f;
				light.pointLightInnerRadius = 0.5f;
				light.pointLightOuterRadius = 20f;
			},
		});

		BuildingEntityRegistry.Register("toLampShrine", new CustomBuildingEntityDefinition {
			Name = "Luminescent sconce",
			Description = "A wall-mounted bulb filled with glowing nectar.",
			Sprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "Lamp2.png"), 50f),
			Health = 35f,
			Placement = BuildingPlacementType.Wall,
			GenerationStyle = BuildingGenerationStyle.Standard,
			SpawnMinPerChunk = 0.2f,
			SpawnMaxPerChunk = 0.4f,
			SurfaceOffset = 0.6f,
			HitSoundReferenceId = "glass",
			AlwaysDrop = [
				BuildingEntityRegistry.AddDrop("toStrangeLamp", 1f, 0.2f, 0.6f),
			],
			SpawnComponents = [
				typeof(CaveLampStationBehaviour).AssemblyQualifiedName!,
			],
			ConfigurePrefab = prefab => {
				var usable = prefab.AddComponent<UsableObject>();
				usable.didLangString = false;
				usable.langToggleString = "toAbsorbGlow";
				usable.rangeMultiplier = 1f;
			},
		});

		BuildingEntityRegistry.Register("toLockedCrate", new CustomBuildingEntityDefinition {
			Name = "Locked supply crate",
			Description = "A reinforced crate with a mechanical lock.",
			Sprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "Crates", "LockedCrate.png")),
			Health = 2000f,
			Metallic = true,
			HitSoundReferenceId = "metal",
			BlockFootstepSoundId = 6,
			LayerEnum = BuildingLayer.Ground,

			AddRigidbody2D = true,
			RigidbodyBodyType = RigidbodyType2D.Dynamic,
			RigidbodyGravityScale = 1f,
			RequireGround = false,

			ItemsDropOnDestroy = [
				BuildingEntityRegistry.AddDrop("toStrangeLamp", chance: 0.8f, conditionMin: 0.6f, conditionMax: 1f),
				BuildingEntityRegistry.AddDrop("scrapmetal", conditionMin: 0.5f, conditionMax: 1f),
			],

			ItemCategoriesToAdd = ["medical", "utility"],
			GuaranteedDropAmount = 2,

			ConfigurePrefab = prefab => {
				var usable = prefab.AddComponent<UsableObject>();
				usable.didLangString = false;
				usable.langToggleString = "toPickLock";

				var openable = prefab.AddComponent<Openable>();
				openable.isKeypad = false;
				openable.lockpickAnglePrecision = 12f;
			},
		});
	}
}
