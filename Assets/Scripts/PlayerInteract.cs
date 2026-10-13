using UnityEngine;

// Put this on Main Camera.
// The ray follows the camera, and the camera follows your head, so you pick things up
// by pointing the flashlight at them and pressing F.
public class PlayerInteract : MonoBehaviour
{
    // F, because Q and E are already used for turning the player.
    public const KeyCode PickupKey = KeyCode.F;

    public float range = 3f;

    // read by GameManager to show the "Press F" prompt
    public static Collectible Looking;

    void Update()
    {
        Looking = null;

        if (GameManager.Instance != null &&
            GameManager.Instance.state != GameManager.State.Playing)
            return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
                            range, ~0, QueryTriggerInteraction.Ignore))
        {
            Collectible c = hit.collider.GetComponentInParent<Collectible>();
            if (c != null)
            {
                Looking = c;
                if (Input.GetKeyDown(PickupKey))
                    c.Collect();
            }
        }
    }
}
