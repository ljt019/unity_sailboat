using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Receiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning;

    void Start()
    {
        udpClient = new UdpClient(3030); // Use the desired port number
        isRunning = true;
        receiveThread = new Thread(new ThreadStart(ReceiveMessages));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveMessages()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (isRunning)
        {
            try
            {
                byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);
                if (receiveBytes.Length >= 2)
                {
                    ushort adcValue = BitConverter.ToUInt16(receiveBytes, 0);
                    Debug.Log($"Received ADC value: {adcValue}");
                }
                else
                {
                    Debug.Log($"Received incomplete data. Length: {receiveBytes.Length}");
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10004) Debug.Log("Socket exception: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data: " + e.Message);
            }
            Thread.Sleep(1);
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        udpClient.Close();
        receiveThread.Join(); // Use Join instead of Abort for cleaner thread termination
    }
}