using CUCoreLib.Helpers;
using HarmonyLib;
using TonnyOverhaul.Components;
using TonnyOverhaul.Statuses;
using UnityEngine;

namespace TonnyOverhaul.Patches;

[HarmonyPatch(typeof(Body), "HandleVariableUpdates")]
internal static class StatusPatches {
	[HarmonyPostfix]
	private static void UpdateStatuses(Body __instance) {
		if (__instance == null) return;

		var glow = __instance.GetStatus<LampGlowStatus>();

		if (glow.RemainingSeconds > 0f)
			glow.RemainingSeconds = Mathf.Max(0f, glow.RemainingSeconds - Time.deltaTime);

		var isActive = glow.RemainingSeconds > 0f;
		if (isActive || glow.WasActiveLastFrame) {
			SyncLampVisual(__instance, glow);
			glow.WasActiveLastFrame = isActive;
		}

		using (CCLBody.Use(__instance)) {
			CCLBody.MaxSpeed = isActive ? 1.5f : 0f;
			CCLBody.HeartRate = isActive ? 15f : 0f;
		}

		if (__instance.limbs == null) return;

		foreach (var limb in __instance.limbs) {
			if (limb == null) continue;

			var shards = limb.GetStatus<GlassShardsLimbStatus>();
			if (!(shards.IrritationTimer > 0f)) continue;
			shards.IrritationTimer = Mathf.Max(0f, shards.IrritationTimer - Time.deltaTime);
			limb.pain += 2f * Time.deltaTime;
		}
	}

	private static void SyncLampVisual(Body body, LampGlowStatus glow) {
		var controller = body.transform.GetComponentInChildren<LampGlowController>();
		if (glow.RemainingSeconds > 0f) {
			if (controller == null) {
				var lightObj = new GameObject("LampGlowEffect");
				lightObj.transform.SetParent(body.transform, false);
				lightObj.transform.localPosition = Vector3.zero;
				controller = lightObj.AddComponent<LampGlowController>();
			}
			controller.SetFromStatus(glow.RemainingSeconds);
		} else if (controller != null) {
			controller.DisableLight();
		}
	}
}
