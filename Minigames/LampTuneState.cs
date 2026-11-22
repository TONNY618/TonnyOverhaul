using System.Collections.Generic;
using CUCoreLib.Helpers;
using UnityEngine;

namespace TonnyOverhaul.Minigames;

public sealed class LampTuneState {
	public readonly List<GameObject> Targets = [];
	public int RemainingTargets;
	public readonly CUCoreMinigameTimer Timer = new(15f);
	public string GuideTemplate = null!;
	public string LastGuideText = null!;
}
