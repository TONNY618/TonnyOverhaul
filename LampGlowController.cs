using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TonnyOverhaul;

public class LampGlowController : MonoBehaviour {
	private Light2D _light2D = null!;
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

	public void SetFromStatus(float remaining) {
		var fadeProgress = remaining < FadeDuration ? 1f - Mathf.Clamp01(remaining / FadeDuration) : 0f;
		_light2D.intensity = Mathf.Lerp(StartIntensity, 0f, fadeProgress);
		_light2D.enabled = remaining > 0f;
	}

	public void DisableLight() {
		_light2D.enabled = false;
	}
}
