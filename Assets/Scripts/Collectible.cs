using UnityEngine;

// Put this on every object the player must find (sphere, cube, any model with a collider).
public class Collectible : MonoBehaviour
{
    public float spinSpeed = 60f;   // slow spin so it catches the flashlight beam
    bool collected;

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    public void Collect()
    {
        if (collected || GameManager.Instance == null ||
            GameManager.Instance.state != GameManager.State.Playing) return;
        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.ItemCollected();

        Destroy(gameObject);
    }
}
