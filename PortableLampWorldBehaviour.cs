using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace TonnyOverhaul;

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
		_humAudio.clip = Plugin.Instance.customSound;
	}

	private void Start() {
		_light2D = GetComponentInChildren<Light2D>();
		_battery = GetComponent<BatteryItem>();

		if (_light2D != null && _humAudio != null) return;

		enabled = false;
	}

	private void Update() {
		if (_item.condition is <= 0f or > 0.3f) return;

		_flickerTimer -= Time.deltaTime;

		if (_flickerTimer > 0f) return;

		_light2D.enabled = !_light2D.enabled;
		_flickerTimer = Random.Range(0.05f, 0.25f);

		if (!_humAudio.isPlaying && _battery.GetCharge() > 0)
			_humAudio.Play();
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
		PlayerCamera.main?.DoAlert("The lamp shattered in front of you!");

		var head = System.Array.Find(player.limbs, l => l.name.ToLower().Contains("head"));

		if (head == null) return;

		head.shrapnel += 1;
		head.skinHealth -= 10;
		head.muscleHealth -= 1;
		head.pain += 20;
		head.bleedAmount += 5;
	}

	private void OnDestroy() {
		_humAudio.Stop();
	}
}
