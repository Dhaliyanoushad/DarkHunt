
using UnityEngine;

public class ObstaclePenalty : MonoBehaviour
{
    public float penaltySeconds = 5f;
    public float cooldown = 2f;

    private float lastHitTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        ApplyPenalty(other);
    }

    private void ApplyPenalty(Collider other)
    {
        if (other.GetComponentInParent<PlayerMove>() == null)
            return;

        var manager = GameManager.Instance;
        if (manager == null || manager.state != GameManager.State.Playing)
            return;

        if (Time.time - lastHitTime < cooldown)
            return;

        lastHitTime = Time.time;

        manager.ApplyTimePenalty(penaltySeconds);
        Debug.Log($"Obstacle hit! -{penaltySeconds} seconds");
    }
}
