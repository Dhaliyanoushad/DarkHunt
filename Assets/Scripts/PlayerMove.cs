using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 3f;
    public bool useMouseTurn = true;
    public float mouseSensitivity = 2f;
    public float keyTurnSpeed = 90f;   // Q / E, degrees per second

    CharacterController cc;
    float vy;
    float baseYaw;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        baseYaw = transform.eulerAngles.y;
        LockCursor(useMouseTurn);
    }

    void LockCursor(bool on)
    {
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }

    void Update()
    {
        // freeze the player on the level-complete / game-over / win screens
        if (GameManager.Instance != null &&
            GameManager.Instance.state != GameManager.State.Playing)
            return;

        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (useMouseTurn && Input.GetMouseButtonDown(0)) LockCursor(true);

        // normal turning: mouse and Q / E
        if (useMouseTurn && Cursor.lockState == CursorLockMode.Locked)
            baseYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        if (Input.GetKey(KeyCode.E)) baseYaw += keyTurnSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q)) baseYaw -= keyTurnSpeed * Time.deltaTime;

        // body rotation = normal turning + head turning (if enabled)
        transform.rotation = Quaternion.Euler(0f, baseYaw + HeadFlashlight.bodyYaw, 0f);

        // walking
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        if (cc.isGrounded && vy < 0) vy = -2f;
        vy += -9.81f * Time.deltaTime;

        cc.Move((move * speed + Vector3.up * vy) * Time.deltaTime);
    }
}
