using UnityEngine;

public class HeadFlashlight : MonoBehaviour
{
    [Header("Input")]
    public bool useFakeInput = false;
    public float smoothing = 12f;
    public bool invertYaw = false;
    public bool invertPitch = false;

    [Header("Setup")]
    public Transform viewCamera;     // drag Main Camera here
    // ON : turning your head also turns the whole player (W walks where you look)
    // OFF: head only moves the camera and torch; the body turns with mouse or Q/E
    public bool headTurnsBody = true;

    [Header("Flashlight")]
    public bool flicker = true;

    public static float yaw;       // written by UDPReceiver
    public static float pitch;
    public static float bodyYaw;   // read by PlayerMove

    float curYaw, curPitch;
    Light lamp;
    float baseIntensity;

    void Start()
    {
        Application.targetFrameRate = 60;
        if (viewCamera == null) viewCamera = transform.parent;
        lamp = GetComponent<Light>();
        if (lamp != null) baseIntensity = lamp.intensity;
    }

    void Update()
    {
        if (useFakeInput)
        {
            yaw   = Mathf.Sin(Time.time * 0.5f) * 40f;
            pitch = Mathf.Sin(Time.time * 0.35f) * 10f;
        }

        float targetYaw   = Mathf.Clamp(invertYaw   ? -yaw   : yaw,   -70f, 70f);
        float targetPitch = Mathf.Clamp(invertPitch ? -pitch : pitch, -35f, 35f);

        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        curYaw   = Mathf.Lerp(curYaw,   targetYaw,   t);
        curPitch = Mathf.Lerp(curPitch, targetPitch, t);

        bodyYaw = headTurnsBody ? curYaw : 0f;

        if (viewCamera != null)
            viewCamera.localRotation = Quaternion.Euler(curPitch, headTurnsBody ? 0f : curYaw, 0f);

        transform.localRotation = Quaternion.identity; // torch follows the camera

        if (flicker && lamp != null)
        {
            float n = Mathf.PerlinNoise(Time.time * 8f, 0f);
            lamp.intensity = baseIntensity * Mathf.Lerp(0.9f, 1f, n);
        }
    }
}