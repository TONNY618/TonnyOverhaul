using CUCoreLib.Helpers;
using TonnyOverhaul.Core;
using TonnyOverhaul.Statuses;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TonnyOverhaul.Components;

public class CaveLampStationBehaviour : MonoBehaviour {
	private BuildingEntity _building = null!;
	private Light2D? _light;
	private bool _depleted;

	private void Awake() {
		_building = GetComponent<BuildingEntity>();

		var lightObj = new GameObject("BuildingLight");
		lightObj.transform.SetParent(transform, false);

		_light = lightObj.AddComponent<Light2D>();
		_light.lightType = Light2D.LightType.Point;
		_light.color = new Color(1f, 0.95f, 0.7f);
		_light.intensity = 1f;
		_light.falloffIntensity = 0.5f;
		_light.pointLightInnerRadius = 0.5f;
		_light.pointLightOuterRadius = 20f;
	}

	public void OnUse() {
		var player = PlayerCamera.main?.body;
		if (player == null || _depleted) return;

		_depleted = true;
		if (_light != null)
			_light.intensity = 0.25f;

		var glow = player.GetStatus<LampGlowStatus>();
		glow.RemainingSeconds += 25f;
		glow.Stacks++;

		var sound = ModAssets.CustomSound;
		if (sound != null)
			ModAssets.AudioSource.PlayOneShot(sound);

		if (TryGetComponent<UsableObject>(out var usable))
			Destroy(usable);
	}

	private void OnDestroy() {
		if (_building == null || _building.GetHealth() >= 0.5f) return;

		Sound.Play("bulbbreak", transform.position);

		var player = PlayerCamera.main?.body;
		if (player == null || Vector2.Distance(transform.position, player.transform.position) > 3f) return;

		var head = System.Array.Find(player.limbs, l => l.isHead);
		if (head == null) return;

		head.shrapnel += 2;
		head.skinHealth -= 15f;
		head.pain += 25f;
		head.GetStatus<GlassShardsLimbStatus>().IrritationTimer = 10f;
	}
}
