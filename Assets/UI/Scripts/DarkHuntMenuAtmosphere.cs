using UnityEngine;
using UnityEngine.UI;

/// <summary>Subtle cinematic motion for the DARK HUNT menu artwork.</summary>
public class DarkHuntMenuAtmosphere : MonoBehaviour
{
    [SerializeField] private RectTransform artwork;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInSeconds = 1.8f;
    [SerializeField] private float driftAmount = 10f;
    [SerializeField] private float driftSpeed = 0.18f;
    [SerializeField] private float lightPulseAmount = 0.025f;
    [SerializeField] private Image tintOverlay;

    private Vector3 initialScale;
    private Vector2 initialPosition;
    private float elapsed;

    private void Awake()
    {
        if (artwork != null)
        {
            initialScale = artwork.localScale;
            initialPosition = artwork.anchoredPosition;
        }
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        if (canvasGroup != null)
            canvasGroup.alpha = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, fadeInSeconds));

        if (artwork != null)
        {
            float t = elapsed * driftSpeed;
            artwork.localScale = initialScale * (1f + 0.012f * Mathf.Sin(t));
            artwork.anchoredPosition = initialPosition + new Vector2(Mathf.Sin(t * 0.7f), Mathf.Cos(t * 0.55f)) * driftAmount;
        }

        if (tintOverlay != null)
        {
            Color c = tintOverlay.color;
            c.a = lightPulseAmount * (0.5f + 0.5f * Mathf.Sin(elapsed * 1.15f));
            tintOverlay.color = c;
        }
    }
}
