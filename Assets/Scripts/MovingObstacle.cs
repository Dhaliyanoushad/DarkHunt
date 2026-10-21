
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.right;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.state != GameManager.State.Playing)
            return;

        if (moveDistance <= 0f ||
            moveDirection.sqrMagnitude < 0.0001f)
            return;

        float offset = Mathf.PingPong(
            Time.timeSinceLevelLoad * moveSpeed,
            moveDistance
        );

        transform.position = startPosition +
            moveDirection.normalized * offset;
    }
}
