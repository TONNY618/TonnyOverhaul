using BepInEx;
using BepInEx.Logging;
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

	private void Awake() {
		Logger = base.Logger;
		Instance = this;

		_harmony.PatchAll();
		Logger.LogInfo($"Plugin {PName} is loaded!");

		// foreach (var res in Resources.LoadAll<GameObject>("")) {
		// 	if (res.name.ToLower().Contains("pear")) {
		// 		Logger.LogInfo($"A suitable resource has been found: {res.name}");
		// 	}
		// }
	}

	private void Start() {
	}

	private void Update() {
	}
}
