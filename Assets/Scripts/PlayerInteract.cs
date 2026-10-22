using UnityEngine;

// Select after movement and head aiming, using the actual player camera.
[DisallowMultipleComponent]
public class PlayerInteract : MonoBehaviour
{
    public const KeyCode PickupKey = KeyCode.F;
    public float range = 3f;
    [Range(1f, 30f)] public float aimTolerance = 12f;
    public Camera viewCamera;
    public static Collectible Looking;

    Collider[] targets;
    readonly RaycastHit[] sightHits = new RaycastHit[32];
    PlayerMove player;

    void Start()
    {
        if (viewCamera == null) viewCamera = GetComponent<Camera>();
        if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
        if (viewCamera == null) viewCamera = Camera.main;
        player = GetComponentInParent<PlayerMove>();
        var collectibles = FindObjectsByType<Collectible>();
        targets = new Collider[collectibles.Length];
        for (int i = 0; i < collectibles.Length; i++)
            targets[i] = collectibles[i].GetComponentInChildren<Collider>();
        Looking = null;
        if (viewCamera == null)
            Debug.LogError("PlayerInteract needs the player's view camera.", this);
    }

    void OnDisable() => Looking = null;

    void LateUpdate()
    {
        if (viewCamera == null || GameManager.Instance == null ||
            GameManager.Instance.state != GameManager.State.Playing)
        {
            Looking = null;
            return;
        }
        bool pressed = Input.GetKeyDown(PickupKey);
        // Honour the prompt seen on the preceding frame despite small head motion.
        // Recheck range and visibility so this cannot collect through a wall.
        if (pressed && Looking != null && CanReach(Looking.GetComponentInChildren<Collider>()))
        {
            CollectLooking();
            return;
        }
        Collectible selected = null;
        float bestAngle = float.PositiveInfinity;
        foreach (Collider target in targets)
        {
            if (target == null || !target.enabled || !target.gameObject.activeInHierarchy) continue;
            Vector3 delta = target.bounds.center - viewCamera.transform.position;
            float angle = Vector3.Angle(viewCamera.transform.forward, delta);
            Collectible item = target.GetComponentInParent<Collectible>();
            // Retention margin prevents prompt flicker at the edge of the aim tolerance.
            float tolerance = aimTolerance + (item == Looking ? 3f : 0f);
            if (item == null || angle > tolerance || angle >= bestAngle || !CanReach(target)) continue;
            bestAngle = angle;
            selected = item;
        }
        Looking = selected;
        if (pressed && Looking != null) CollectLooking();
    }

    bool CanReach(Collider target)
    {
        if (target == null || !target.enabled || !target.gameObject.activeInHierarchy) return false;
        Vector3 origin = viewCamera.transform.position;
        if ((target.ClosestPoint(origin) - origin).sqrMagnitude > range * range) return false;
        Vector3 direction = target.bounds.center - origin;
        float distance = direction.magnitude;
        if (distance < 0.001f) return true;
        int count = Physics.RaycastNonAlloc(origin, direction / distance, sightHits, distance,
            ~0, QueryTriggerInteraction.Ignore);
        if (count == sightHits.Length) return false;
        Collectible item = target.GetComponentInParent<Collectible>();
        for (int i = 0; i < count; i++)
        {
            Collider hit = sightHits[i].collider;
            if (hit.GetComponentInParent<Collectible>() == item) continue;
            if (player != null && hit.GetComponentInParent<PlayerMove>() == player) continue;
            return false;
        }
        return true;
    }

    void CollectLooking()
    {
        Collectible item = Looking;
        Looking = null;
        item.Collect();
    }
}
