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

	private AudioSource _audioSource = null!;
	public AudioClip? customSound;
	private Sprite? _lampSprite;

	private void Awake() {
		Logger = base.Logger;
		Instance = this;

		_audioSource = gameObject.AddComponent<AudioSource>();
		_audioSource.playOnAwake = false;
		_audioSource.spatialBlend = 0f;

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
				TriggerLampEffect(body.transform);
			},
			usableOnLimb = true,
			useLimbAction = (limb, item) => {
				limb.shrapnel += 5;
				limb.skinHealth -= 30;
				limb.muscleHealth -= 5;
				limb.pain += 80;
				limb.bleedAmount += 15;
				item.condition -= 1f;
				Sound.Play("bulbbreak", limb.transform.position);
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
			onDrink = (_, body) => {
				TriggerLampEffect(body.transform);
			},
			onInject = (ml, limb) => {
				var dose = ml * 0.01f;
				limb.pain -= dose * 20;
			},
			qualities = [
				CUCoreUtils.CreateCraftingQuality("water", 0.5f),
			],
		});

		StartCoroutine(SortConsoleAutofillRoutine());
		// StartCoroutine(GiveStartingItemsRoutine());
	}

	public void TriggerLampEffect(Transform playerTransform) {
		if (customSound != null)
			_audioSource.PlayOneShot(customSound);

		var controller = playerTransform.GetComponentInChildren<LampGlowController>();

		if (controller == null) {
			var lightObj = new GameObject("LampGlowEffect");
			lightObj.transform.SetParent(playerTransform, false);
			lightObj.transform.localPosition = Vector3.zero;
			controller = lightObj.AddComponent<LampGlowController>();
		}

		controller.Trigger(5f);
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
