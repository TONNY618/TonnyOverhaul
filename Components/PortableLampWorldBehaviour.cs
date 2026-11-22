using CUCoreLib.Registries;
using TonnyOverhaul.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace TonnyOverhaul.Components;

public class PortableLampWorldBehaviour : MonoBehaviour {
	private Item _item = null!;
	private Light2D _light2D = null!;
	private AudioSource _humAudio = null!;
	private float _flickerTimer;
	private BatteryItem _battery = null!;

	private void Awake() {
		_item = GetComponent<Item>();

		_humAudio = gameObject.AddComponent<AudioSource>();
		_humAudio.spatialBlend = 1f;
		_humAudio.minDistance = 1f;
		_humAudio.maxDistance = 5f;
		_humAudio.loop = true;
		_humAudio.playOnAwake = false;
		_humAudio.volume = 3f;
		_humAudio.clip = ModAssets.CustomSound;
	}

	private void Start() {
		_light2D = GetComponentInChildren<Light2D>();
		_battery = GetComponent<BatteryItem>();

		if (_light2D != null && _humAudio != null) return;

		enabled = false;
	}

	private void Update() {
		switch (_item.condition) {
			case > 0f and <= 0.3f: {
				_flickerTimer -= Time.deltaTime;

				if (_flickerTimer > 0f) return;

				_light2D.enabled = !_light2D.enabled;
				_flickerTimer = Random.Range(0.05f, 0.25f);

				if (!_humAudio.isPlaying)
					_humAudio.Play();

				return;
			}
			case > 0.3f:
				_light2D.enabled = true;
				break;
			case <= 0f:
				_light2D.enabled = false;
				break;
		}

		if (_humAudio.isPlaying)
			_humAudio.Stop();
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.relativeVelocity.magnitude < 25f) return;

		if (_item.condition <= 0f) return;

		Sound.Play("bulbbreak", transform.position);
		_item.condition -= 0.1f;

		if (_item.condition > 0f) return;

		_light2D.enabled = false;
		_humAudio.Stop();

		var player = PlayerCamera.main?.body;
		if (player == null) return;

		if (Vector2.Distance(transform.position, player.transform.position) > 3f) return;

		player.happiness -= 10f;
		PlayerCamera.main?.DoAlert(LocaleRegistry.Get("other", "toLampShattered", "The lamp shattered in front of you!"));

		var head = System.Array.Find(player.limbs, l => l.name.ToLower().Contains("head"));

		if (head == null) return;

		head.shrapnel += 1;
		head.skinHealth -= 10;
		head.muscleHealth -= 1;
		head.pain += 20;
		head.bleedAmount += 5;

		MoodleRegistry.AddMoodle(
			intensity: 1,
			iconId: "pain",
			name: LocaleRegistry.Get("moodle", "toShatteredGlass", "Shattered Glass"),
			description: LocaleRegistry.Get("moodle", "toShatteredGlassdsc", "A lamp burst right in your face."),
			critical: false,
			important: false,
			key: "tonnyoverhaul.shattered",
			holdSeconds: 4f
		);
	}

	private void OnDestroy() {
		_humAudio.Stop();
	}
}
