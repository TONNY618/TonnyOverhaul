using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TonnyOverhaul;

[BepInPlugin(PGuid, PName, PVersion)]
[BepInDependency("net.cucorelib")]
public class Plugin : BaseUnityPlugin {
	public const string PGuid = "com.TONNY618.TonnyOverhaul";
	public const string PName = "Tonny Overhaul";
	public const string PVersion = "0.1.0";

	internal new static ManualLogSource Logger = null!;
	private readonly Harmony _harmony = new(PGuid);
	public static Plugin Instance {get; private set;} = null!;

	public static readonly List<string> CustomItems = [];

	public AudioSource audioSource = null!;
	public AudioClip? customSound;
	private Sprite? _lampSprite;

	private void Awake() {
		Logger = base.Logger;
		Instance = this;

		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.spatialBlend = 0f;

		_harmony.PatchAll();
		Logger.LogInfo($"Plugin {PName} is loaded!");

		// foreach (var res in Resources.LoadAll<GameObject>("")) {
		// 	if (res.name.ToLower().Contains("pear")) {
		// 		Logger.LogInfo($"A suitable resource has been found: {res.name}");
		// 	}
		// }
	}

	private void Start() {
		var modFolder = Path.GetDirectoryName(Info.Location)!;

		var soundPath = Path.Combine(modFolder, "Assets", "Audio", "Sound.ogg");
		if (File.Exists(soundPath)) {
			customSound = AssetLoader.LoadAudioFromPluginFolder(this, soundPath);
			AmplifyClip(customSound, 3f);
		} else {
			Logger.LogWarning($"There is no file at the path: {soundPath}");
		}

		var spritePath = Path.Combine(modFolder, "Assets", "Sprites", "Lamp.png");
		if (File.Exists(spritePath)) {
			_lampSprite = AssetLoader.LoadSpriteFromPluginFolder(this, spritePath, 16f);
		} else {
			Logger.LogWarning($"There is no file at the path: {spritePath}");
		}

		CustomItems.Add("toStrangeLamp");
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

				if (customSound != null)
					audioSource.PlayOneShot(customSound);
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
		}, _lampSprite);

		CustomItems.Add("toPortableLamp");
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
		}.AddSpawnComponent<PortableLampWorldBehaviour>(), _lampSprite);

		RecipeRegistry.Register(new Recipe {
			INT = 7,
			category = Recipes.RecipeCategory.Utilities,
			result = new RecipeResult {
				id = "toPortableLamp",
				amount = 1,
				isLiquid = false,
				resultCondition = 0,
			},
			items = [
				new RecipeItem(0) {specific = true, specificId = "lightbulb"},
				new RecipeItem(0.25f) {specific = true, specificId = "bundleofwires"},
				new RecipeItem {specific = true, specificId = "flexiglass"},
			],
		});

		LiquidRegistry.Register("toGlowJuice", new CustomLiquidInfo {
			name = "Luminescent nectar",
			description = "A glowing liquid derived from cave lamps. Drinking it fills you with light.",
			color = new Color(1f, 0.9f, 0.2f, 0.85f),
			unobtainable = false,
			healthUsable = true,
			injectable = true,
			injectionSickness = 0,
			onDrink = (ml, body) => {
				var sip = ml * 0.01f;
				var glow = body.GetStatus<LampGlowStatus>();
				glow.RemainingSeconds += 10f * sip;

				if (customSound != null && sip >= 1)
					audioSource.PlayOneShot(customSound);
			},
			onInject = (ml, limb) => {
				limb.pain -= ml * 2;
			},
			qualities = [
				CUCoreUtils.CreateCraftingQuality("water", 0.5f),
			],
		});

		StartCoroutine(SortConsoleAutofillRoutine());
		// StartCoroutine(GiveStartingItemsRoutine());

		var glowMoodle = AssetLoader.LoadSpriteFromPluginFolder(this, "Assets/Sprites/GlowMoodle.png", 100f);
		var irritationMoodle = AssetLoader.LoadSpriteFromPluginFolder(this, "Assets/Sprites/IrritationMoodle.png", 100f);

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
				IconSprite = glowMoodle,
				Name = "Bioluminescent Glow",
				Description = $"You emit light ({status.RemainingSeconds:0.0}s left, stacks: {status.Stacks}). +Max Speed, +Heart Rate",
				Important = true,
				Critical = false,
			};
		});

		MoodleRegistry.RegisterLimb<GlassShardsLimbStatus>((limb, status) => {
			if (status.IrritationTimer <= 0f)
				return null;

			return new StatusMoodleDefinition {
				Intensity = status.IrritationTimer > 10f ? 2 : 1,
				IconSprite = irritationMoodle,
				Name = $"Glass Irritation ({limb.fullName})",
				Description = $"Shards are irritating your {limb.fullName.ToLower()} ({status.IrritationTimer:0}s).",
				Important = false,
				Critical = false,
			};
		});

		BuildingEntityRegistry.Register("toCaveLamp", new CustomBuildingEntityDefinition {
			Name = "Bioluminescent Lamp",
			Description = "An odd glowing fixture rooted into the cave floor. Breaks easily.",
			Sprite = _lampSprite,
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
			Name = "Luminescent Sconce",
			Description = "A wall-mounted bulb filled with glowing nectar.",
			Sprite = AssetLoader.LoadSpriteFromPluginFolder(this, Path.Combine(modFolder, "Assets", "Sprites", "Lamp2.png"), 50f),
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
				usable.didLangString = true;
				usable.toggleString = "Absorb glow";
				usable.rangeMultiplier = 1f;
			},
		});
	}

	private static IEnumerator SortConsoleAutofillRoutine() {
		yield return new WaitUntil(() => ConsoleScript.Commands is not null && ConsoleScript.Commands.Count > 0);

		var spawnCommand = ConsoleScript.Commands.Find(c => c.name == "spawn");

		if (spawnCommand?.argAutofill is null || !spawnCommand.argAutofill.TryGetValue(0, out List<string> itemList)) yield break;
		var sorted = itemList
			.OrderByDescending(id => CustomItems.Contains(id))
			.ThenBy(x => x)
			.ToList();
		itemList.Clear();
		itemList.AddRange(sorted);
	}

	private static IEnumerator GiveStartingItemsRoutine() {
		yield return new WaitUntil(() => PlayerCamera.main != null && PlayerCamera.main.body != null);

		var player = PlayerCamera.main.body;
		var pos = player.transform.position;

		string[] itemsToGive = ["lightbulb", "bundleofwires", "flexiglass"];

		for (var i = 0; i < itemsToGive.Length; i++) {
			var go = Utils.Create(itemsToGive[i], pos, 0f);
			if (go == null) continue;
			var item = go.GetComponent<Item>();
			if (item != null)
				player.PickUpItem(item, i, true);
		}
	}

	private static void AmplifyClip(AudioClip clip, float multiplier) {
		var samples = new float[clip.samples * clip.channels];
		if (!clip.GetData(samples, 0)) return;

		for (var i = 0; i < samples.Length; i++) {
			samples[i] = Mathf.Clamp(samples[i] * multiplier, -1f, 1f);
		}

		clip.SetData(samples, 0);
	}
}
