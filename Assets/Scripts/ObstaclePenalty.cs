
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

    private void OnTriggerStay(Collider other)
    {
        ApplyPenalty(other);
    }

    private void ApplyPenalty(Collider other)
    {
        if (other.GetComponentInParent<PlayerMove>() == null)
            return;

        if (Time.time - lastHitTime < cooldown)
            return;

        lastHitTime = Time.time;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimePenalty(
                penaltySeconds
            );
        }

        Debug.Log("Obstacle hit! -5 seconds");
    }
}
