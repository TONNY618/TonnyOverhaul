using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using TonnyOverhaul.Core;
using TonnyOverhaul.Statuses;
using UnityEngine;

namespace TonnyOverhaul.Content;

public static class ModLiquids {
	public static void Register() {
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

				if (ModAssets.CustomSound != null && sip >= 1)
					ModAssets.AudioSource.PlayOneShot(ModAssets.CustomSound);
			},
			onInject = (ml, limb) => {
				limb.pain -= ml * 2;
			},
			qualities = [
				CUCoreUtils.CreateCraftingQuality("water", 0.5f),
			],
		});
	}
}
