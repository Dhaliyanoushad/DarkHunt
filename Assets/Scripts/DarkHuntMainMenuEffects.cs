using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DarkHuntMainMenuEffects : MonoBehaviour
{
    private TMP_Text title;
    private Graphic backgroundGlow;
    private float elapsed;

    private void Start()
    {
        title = FindText("GameTitle");
        backgroundGlow = FindGraphic("LeftAtmosphere");
        if (title != null)
        {
            Color c = title.color;
            c.a = 0f;
            title.color = c;
        }
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        if (title != null)
        {
            Color c = title.color;
            c.a = Mathf.MoveTowards(c.a, 1f, Time.unscaledDeltaTime * 0.65f);
            float pulse = 0.94f + 0.06f * Mathf.Sin(elapsed * 1.2f);
            title.color = new Color(c.r * pulse, c.g * pulse, c.b * pulse, c.a);
        }
        if (backgroundGlow != null)
        {
            Color c = backgroundGlow.color;
            c.a = 0.08f + 0.045f * (0.5f + 0.5f * Mathf.Sin(elapsed * 0.55f));
            backgroundGlow.color = c;
        }
    }

    private TMP_Text FindText(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }

    private Graphic FindGraphic(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<Graphic>() : null;
    }
}

