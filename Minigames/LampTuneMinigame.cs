using System.Collections.Generic;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using TonnyOverhaul.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TonnyOverhaul.Minigames;

public sealed class LampTuneMinigame : CUCoreMinigameDefinition {
	public override Minigame.HandSpriteType HandType(CUCoreMinigameSession session)
		=> Minigame.HandSpriteType.Wrench;

	public override bool NeedsItem(CUCoreMinigameSession session) => true;

	public override bool CanExit(CUCoreMinigameSession session) => true;

	public override string GuideLocaleKey(CUCoreMinigameSession session) => "toLampTuneGuide";

	public override void Start(CUCoreMinigameSession session) {
		var state = session.GetOrCreateState<LampTuneState>();

		state.GuideTemplate = LocaleRegistry.Get("other", "toLampTuneGuide", "Click all loose contacts to calibrate the lamp.\n{0}s");

		var root = new GameObject("LampTuneScreen", typeof(RectTransform));
		root.transform.SetParent(session.MinigameScreen, false);
		root.transform.SetSiblingIndex(0);

		session.Game.spawnedMiniGame = root.transform;

		var rootRect = root.GetComponent<RectTransform>();
		rootRect.anchorMin = Vector2.zero;
		rootRect.anchorMax = Vector2.one;
		rootRect.offsetMin = Vector2.zero;
		rootRect.offsetMax = Vector2.zero;

		var bgObj = new GameObject("LampBody", typeof(RectTransform), typeof(Image));
		bgObj.transform.SetParent(root.transform, false);

		var bgRect = bgObj.GetComponent<RectTransform>();
		bgRect.sizeDelta = new Vector2(420f, 420f);

		var bgImage = bgObj.GetComponent<Image>();
		bgImage.sprite = ModAssets.LampSprite;
		bgImage.color = new Color(0.85f, 0.85f, 0.85f, 1f);
		bgImage.raycastTarget = false;

		Vector2[] offsets = [
			new(-110f, 80f),
			new(120f, 40f),
			new(0f, -120f),
		];

		for (var i = 0; i < offsets.Length; i++) {
			var targetObj = new GameObject($"Contact_{i}", typeof(RectTransform), typeof(Image));
			targetObj.transform.SetParent(root.transform, false);

			var img = targetObj.GetComponent<Image>();
			img.color = new Color(1f, 0.4f, 0.2f, 1f);
			img.raycastTarget = true;

			var rect = targetObj.GetComponent<RectTransform>();
			rect.anchoredPosition = offsets[i];
			rect.sizeDelta = new Vector2(70f, 70f);

			state.Targets.Add(targetObj);
		}

		state.RemainingTargets = state.Targets.Count;

		UpdateTimer(session, state);
	}

	public override void PhysicsUpdate(CUCoreMinigameSession session, float deltaTime) {
		var state = session.GetOrCreateState<LampTuneState>();

		state.Timer.Tick(deltaTime);
	}

	public override void Update(CUCoreMinigameSession session, List<RaycastResult> uiCasts) {
		var state = session.GetOrCreateState<LampTuneState>();

		UpdateTimer(session, state);

		if (state.Timer.IsComplete) {
			session.Fail();
			return;
		}

		if (session.CurrentItem?.battery == null || !session.CurrentItem.battery.hasBattery) {
			session.Cancel();
			return;
		}

		GameObject hovered = null!;

		foreach (var cast in uiCasts) {
			if (!state.Targets.Contains(cast.gameObject)) continue;

			hovered = cast.gameObject;
			break;
		}

		foreach (var target in state.Targets) {
			if (target == null || !target.activeSelf) continue;

			var img = target.GetComponent<Image>();
			img.color = target == hovered ? new Color(1f, 0.9f, 0.3f, 1f) : new Color(1f, 0.4f, 0.2f, 1f);
		}

		if (!session.HandStartedClicking || hovered == null) return;

		hovered.SetActive(false);
		state.RemainingTargets--;

		Sound.Play("combine", session.Body.transform.position);

		if (state.RemainingTargets <= 0)
			session.Complete();
	}

	public override void End(CUCoreMinigameSession session, CUCoreMinigameEndReason reason) {
		switch (reason) {
			case CUCoreMinigameEndReason.Completed: {
				if (session.CurrentItem != null) {
					if (session.CurrentItem.condition <= 0.5f)
						session.Body.happiness += 2f;

					session.CurrentItem.condition = Mathf.Min(1f, session.CurrentItem.condition + 0.35f);
				}

				PlayerCamera.main?.DoAlert(LocaleRegistry.Get("other", "toLampCalibrated", "The lamp is calibrated."));
				break;
			}
			case CUCoreMinigameEndReason.Failed: {
				if (session.CurrentItem == null || session.CurrentItem.condition <= 0f) break;

				session.CurrentItem.condition -= 0.1f;

				Sound.Play("bulbbreak", session.Body.transform.position);
				PlayerCamera.main?.DoAlert(LocaleRegistry.Get("other", "toLampCalibrationFailed", "You failed the calibration."));
				break;
			}
		}
	}

	private static void UpdateTimer(CUCoreMinigameSession session, LampTuneState state) {
		if (session.Game.guideText == null || string.IsNullOrWhiteSpace(state.GuideTemplate)) return;

		var form = string.Format(state.GuideTemplate, state.Timer.Remaining.ToString("0.0"));
		if (form == state.LastGuideText) return;

		state.LastGuideText = form;
		session.Game.guideText.text = form;
	}
}
