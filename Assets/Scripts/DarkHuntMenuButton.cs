using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DarkHuntMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public TMP_Text buttonLabel;
    public bool isPrimary;
    private RectTransform rectTransform;
    private Vector3 targetScale = Vector3.one;
    private Color normalTextColor;
    private Color hoverTextColor = new Color32(151, 222, 248, 255);

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        if (buttonLabel != null) normalTextColor = buttonLabel.color;
    }

    private void Update()
    {
        if (rectTransform != null)
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * 12f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * 1.025f;
        if (buttonLabel != null) buttonLabel.color = hoverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
        if (buttonLabel != null) buttonLabel.color = normalTextColor;
    }
}

