using System;
using System.Collections;
using CUCoreLib.Helpers;
using TonnyOverhaul.Statuses;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TonnyOverhaul.Components;

public class LightMineTrapBehaviour : MonoBehaviour {
	public float triggerDistance = 4.5f;
	public float armDelay = 0.5f;
	public float effectRadius = 6f;

	private BuildingEntity _building = null!;
	private Light2D? _light;
	private bool _armed;
	private bool _detonated;

	private Body _plr = null!;

	private void Awake() {
		_building = GetComponent<BuildingEntity>();

		var lightObj = new GameObject("TrapLight");
		lightObj.transform.SetParent(transform, false);

		_light = lightObj.AddComponent<Light2D>();
		_light.lightType = Light2D.LightType.Point;
		_light.color = new Color(1f, 0.85f, 0.35f);
		_light.intensity = 0.7f;
		_light.pointLightInnerRadius = 0.3f;
		_light.pointLightOuterRadius = 8f;
	}

	private void Start() {
		if (WorldGeneration.world == null || PlayerCamera.main == null || PlayerCamera.main.body == null || _light == null) {
			enabled = false;
			return;
		}

		_plr = PlayerCamera.main.body;
	}

	private void Update() {
		if (_detonated || _armed ||
		    WorldGeneration.world.generatingWorld) return;

		if (!_plr.alive ||
		    Vector2.Distance(transform.position, _plr.transform.position) > triggerDistance ||
		    Physics2D.Linecast(transform.position, _plr.transform.position, LayerMask.GetMask("Ground"))) return;

		StartCoroutine(ArmAndDetonateRoutine());
	}

	private IEnumerator ArmAndDetonateRoutine() {
		_armed = true;
		Sound.Play("mine", transform.position);

		var elapsed = 0f;
		while (elapsed < armDelay && !_detonated) {
			elapsed += Time.deltaTime;

			_light!.color = new Color(1f, 0.35f, 0.2f);
			_light.intensity = Mathf.PingPong(elapsed * 20f, 1f) > 0.5f ? 2.5f : 0f;

			yield return null;
		}

		if (_detonated) yield break;
		Detonate();
		Destroy(gameObject);
	}

	private void OnDestroy() {
		if (_detonated || _building == null ||
		    WorldGeneration.world.generatingWorld ||
		    _building.GetHealth() >= 0.5f) return;

		Detonate();
	}

	private void Detonate() {
		if (_detonated) return;
		_detonated = true;

		Sound.Play("bulbbreak", transform.position);
		Instantiate(Resources.Load<GameObject>("DustBig"), transform.position, Quaternion.identity);
		PlayerCamera.main.shaker.Shake(120f);

		if (!_plr.alive ||
		    Vector2.Distance(transform.position, _plr.transform.position) > effectRadius ||
		    Physics2D.Linecast(transform.position, _plr.transform.position, LayerMask.GetMask("Ground"))) return;

		_plr.eyePanicTime = 1.5f;
		_plr.happiness -= 6f;
		_plr.Ragdoll();
		_plr.shock = Mathf.Max(_plr.shock, 30f);
		_plr.rb.AddForce(((Vector2) _plr.transform.position - (Vector2) transform.position).normalized * 400f, ForceMode2D.Impulse);

		var glow = _plr.GetStatus<LampGlowStatus>();
		glow.RemainingSeconds += 20f;
		glow.Stacks++;

		var limb = _plr.GetClosestLimb(transform.position);
		if (limb == null || limb.dismembered) return;

		var armor = limb.GetArmorReduction();
		limb.DamageWearables(0.2f);
		limb.shrapnel += 5;
		limb.skinHealth -= 20f / armor;
		limb.pain += 30f / armor;
		limb.bleedAmount += 6f / armor;
		limb.GetStatus<GlassShardsLimbStatus>().IrritationTimer = 10f;
	}
}
