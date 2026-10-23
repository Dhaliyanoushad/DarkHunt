using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Invisible click zones with a subtle red hover wash over the artwork's printed buttons.</summary>
[RequireComponent(typeof(Button))]
public class DarkHuntMenuHotspot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image hoverWash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField, Range(0f, 0.6f)] private float hoverAlpha = 0.18f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (hoverWash != null)
        {
            Color c = hoverWash.color;
            c.a = 0f;
            hoverWash.color = c;
            hoverWash.raycastTarget = false;
        }
        button.onClick.AddListener(PlayClick);
    }

    public void OnPointerEnter(PointerEventData eventData) { SetHover(true); }
    public void OnPointerExit(PointerEventData eventData) { SetHover(false); }
    public void OnSelect(BaseEventData eventData) { SetHover(true); }
    public void OnDeselect(BaseEventData eventData) { SetHover(false); }

    private void SetHover(bool active)
    {
        if (hoverWash != null)
        {
            Color c = hoverWash.color;
            c.a = active ? hoverAlpha : 0f;
            hoverWash.color = c;
        }
        if (active && audioSource != null && hoverClip != null)
            audioSource.PlayOneShot(hoverClip, 0.35f);
    }

    private void PlayClick()
    {
        if (audioSource != null && clickClip != null)
            audioSource.PlayOneShot(clickClip);
    }
}
