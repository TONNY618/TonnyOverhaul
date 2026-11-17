using System.IO;
using CUCoreLib.Helpers;
using UnityEngine;

namespace TonnyOverhaul.Core;

public static class ModAssets {
	public static AudioSource AudioSource = null!;
	public static AudioClip? CustomSound;
	public static Sprite? LampSprite;

	public static void Load() {
		AudioSource = Plugin.Instance.gameObject.AddComponent<AudioSource>();
		AudioSource.playOnAwake = false;
		AudioSource.spatialBlend = 0f;

		CustomSound = AssetLoader.LoadAudioFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Audio", "Sound.ogg"));
		AmplifyClip(CustomSound, 3f);

		LampSprite = AssetLoader.LoadSpriteFromPluginFolder(Plugin.Instance, Path.Combine("Assets", "Sprites", "Lamp.png"), 16f);
	}

	public static void AmplifyClip(AudioClip? clip, float multiplier) {
		if (clip == null) return;

		var samples = new float[clip.samples * clip.channels];
		if (!clip.GetData(samples, 0)) return;

		for (var i = 0; i < samples.Length; i++) {
			samples[i] = Mathf.Clamp(samples[i] * multiplier, -1f, 1f);
		}

		clip.SetData(samples, 0);
	}
}
