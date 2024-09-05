using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class UdpReceiver
{
    private UdpClient udpClient;
    private readonly Queue<ushort> incomingQueue = new Queue<ushort>();
    private Thread receiveThread;
    private bool threadRunning = false;

    public void StartReceiving(int receivePort)
    {
        try
        {
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, receivePort));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to listen for UDP at port {receivePort}: {e.Message}");
            return;
        }
        Debug.Log($"Created receiving client at port {receivePort}");
        StartReceiveThread();
    }

    private void StartReceiveThread()
    {
        receiveThread = new Thread(() => ListenForMessages(udpClient));
        receiveThread.IsBackground = true;
        threadRunning = true;
        receiveThread.Start();
    }

    private void ListenForMessages(UdpClient client)
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (threadRunning)
        {
            try
            {
                byte[] receiveBytes = client.Receive(ref remoteIpEndPoint);
                if (receiveBytes.Length >= 2)
                {
                    ushort adcValue = BitConverter.ToUInt16(receiveBytes, 0);

                    lock (incomingQueue)
                    {
                        incomingQueue.Enqueue(adcValue);
                    }
                }
                else
                {
                    Debug.LogWarning($"Received incomplete data. Length: {receiveBytes.Length}");
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10004) Debug.LogError($"Socket exception while receiving data from UDP client: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving data from UDP client: {e.Message}");
            }
            Thread.Sleep(1);
        }
    }

    public ushort[] GetMessages()
    {
        ushort[] pendingMessages;
        lock (incomingQueue)
        {
            pendingMessages = incomingQueue.ToArray();
            incomingQueue.Clear();
        }
        return pendingMessages;
    }

    public void Stop()
    {
        threadRunning = false;
        receiveThread.Join();
        udpClient.Close();
    }
}