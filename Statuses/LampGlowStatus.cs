using CUCoreLib.Data;
using Newtonsoft.Json;

namespace TonnyOverhaul.Statuses;

[StatusOptions(Key = "tonnyoverhaul.lampGlow", SaveEnabled = true, ClientAuthoritative = false)]
public class LampGlowStatus : BodyStatus {
	public float RemainingSeconds;
	public int Stacks;

	[JsonIgnore] public bool WasActiveLastFrame;
}
