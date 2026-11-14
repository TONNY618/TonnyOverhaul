using CUCoreLib.Data;

namespace TonnyOverhaul;

[StatusOptions(Key = "tonnyoverhaul.glassShards", SaveEnabled = true)]
public class GlassShardsLimbStatus : LimbStatus {
	public float IrritationTimer;
}
