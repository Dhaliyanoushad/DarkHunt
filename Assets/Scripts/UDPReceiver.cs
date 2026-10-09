using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    public int port = 5005;
    public bool showDebug = true;

    UdpClient client;
    Thread thread;
    volatile bool running;
    volatile float y, p;
    volatile int packets;

    void Start()
    {
        Application.runInBackground = true;
        client = new UdpClient(port);
        running = true;
        thread = new Thread(Listen) { IsBackground = true };
        thread.Start();
    }

        void Listen()
    {
        var ep = new IPEndPoint(IPAddress.Any, 0);
        while (running)
        {
            try
            {
                string msg = Encoding.UTF8.GetString(client.Receive(ref ep));
                if (packets == 0) Debug.Log("UDP first packet: " + msg);
                string[] s = msg.Split(',');
                y = float.Parse(s[0], CultureInfo.InvariantCulture);
                p = float.Parse(s[1], CultureInfo.InvariantCulture);
                packets++;
            }
            catch (System.Exception e)
            {
                if (running) Debug.LogWarning("UDP error: " + e.Message);
            }
        }
    }

    void Update()
    {
        HeadFlashlight.yaw = y;
        HeadFlashlight.pitch = p;
    }

    void OnGUI()
    {
        if (showDebug)
            GUI.Label(new Rect(10, 10, 400, 25), $"Yaw {y:F1}   Pitch {p:F1}   packets {packets}");
    }

    void OnDestroy()
    {
        running = false;
        client?.Close();
    }
}