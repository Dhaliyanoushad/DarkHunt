using UnityEngine;

// Put this on every object the player must find (sphere, cube, any model with a collider).
public class Collectible : MonoBehaviour
{
    public float spinSpeed = 60f;   // slow spin so it catches the flashlight beam

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    public void Collect()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ItemCollected();

        Destroy(gameObject);
    }
}
