using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TonnyOverhaul;

public class LampGlowController : MonoBehaviour {
	private Light2D _light2D = null!;
	private float _duration;
	private float _timer;
	private const float StartIntensity = 2.5f;
	private const float FadeDuration = 1.5f;

	private void Awake() {
		_light2D = gameObject.AddComponent<Light2D>();
		_light2D.lightType = Light2D.LightType.Point;
		_light2D.pointLightInnerRadius = 0.5f;
		_light2D.pointLightOuterRadius = 20.0f;
		_light2D.color = new Color(1f, 0.95f, 0.7f);
		_light2D.enabled = false;
	}

	public void Trigger(float duration) {
		_duration = duration;
		_timer = 0f;
		_light2D.intensity = StartIntensity;
		_light2D.enabled = true;
	}

	private void Update() {
		if (!_light2D.enabled) return;

		_timer += Time.deltaTime;

		var fadeStart = _duration - FadeDuration;
		if (_timer > fadeStart) {
			var fadeProgress = (_timer - fadeStart) / FadeDuration;
			_light2D.intensity = Mathf.Lerp(StartIntensity, 0f, fadeProgress);
		}

		if (_timer >= _duration) {
			_light2D.enabled = false;
		}
	}
}
