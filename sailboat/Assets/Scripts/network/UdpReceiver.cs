using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// Handles receiving UDP messages on a specified port.
/// This class is thread-safe and ensures that messages are processed efficiently.
/// </summary>
public class UdpReceiver : IDisposable
{
    #region Constants

    private const int BUFFER_SIZE = 1024; // Define a reasonable buffer size

    #endregion

    #region Private Fields

    private UdpClient udpClient;
    private readonly ConcurrentQueue<ushort> incomingQueue = new ConcurrentQueue<ushort>();
    private readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
    private Thread receiveThread;
    private volatile bool threadRunning = false;
    private readonly int receivePort;

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates whether the UDP receiver is currently running.
    /// </summary>
    public bool IsRunning => threadRunning;

    /// <summary>
    /// Retrieves and clears all pending log messages.
    /// Should be called from the main thread.
    /// </summary>
    /// <returns>An array of log messages.</returns>
    public string[] GetLogMessages()
    {
        var logs = new System.Collections.Generic.List<string>();
        while (logQueue.TryDequeue(out string log))
        {
            logs.Add(log);
        }
        return logs.ToArray();
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the UdpReceiver class and starts listening on the specified port.
    /// </summary>
    /// <param name="port">The UDP port to listen on.</param>
    public UdpReceiver(int port)
    {
        receivePort = port;
        StartReceiving();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Retrieves all pending messages received via UDP.
    /// </summary>
    /// <returns>An array of ushort ADC values.</returns>
    public ushort[] GetMessages()
    {
        var messages = new System.Collections.Generic.List<ushort>();
        while (incomingQueue.TryDequeue(out ushort adcValue))
        {
            messages.Add(adcValue);
        }
        return messages.ToArray();
    }

    /// <summary>
    /// Stops the UDP receiver and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        StopReceiving();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Starts the UDP receiving thread.
    /// </summary>
    private void StartReceiving()
    {
        try
        {
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, receivePort));
            threadRunning = true;
            receiveThread = new Thread(() => ListenForMessages(udpClient))
            {
                IsBackground = true,
                Name = "UdpReceiverThread"
            };
            receiveThread.Start();
            EnqueueLog($"UDP Receiver started on port {receivePort}.");
        }
        catch (Exception e)
        {
            EnqueueLog($"Failed to listen for UDP on port {receivePort}: {e.Message}");
        }
    }

    /// <summary>
    /// Stops the UDP receiving thread and closes the UDP client.
    /// </summary>
    private void StopReceiving()
    {
        if (!threadRunning)
            return;

        threadRunning = false;
        udpClient?.Close();

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
            receiveThread = null;
        }

        EnqueueLog($"UDP Receiver stopped on port {receivePort}.");
    }

    /// <summary>
    /// Continuously listens for incoming UDP messages.
    /// </summary>
    /// <param name="client">The UdpClient instance to receive messages from.</param>
    private void ListenForMessages(UdpClient client)
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] buffer = new byte[BUFFER_SIZE];

        while (threadRunning)
        {
            try
            {
                var asyncResult = client.BeginReceive(null, null);
                // Wait for data or timeout after 1 second to check the threadRunning flag
                WaitHandle.WaitAny(new WaitHandle[] { asyncResult.AsyncWaitHandle }, 1000);

                if (asyncResult.IsCompleted)
                {
                    buffer = client.EndReceive(asyncResult, ref remoteIpEndPoint);

                    if (buffer.Length >= 2)
                    {
                        ushort adcValue = BitConverter.ToUInt16(buffer, 0);
                        incomingQueue.Enqueue(adcValue);
                    }
                    else
                    {
                        EnqueueLog($"Received incomplete data. Length: {buffer.Length}");
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Expected when udpClient is closed. Exit gracefully.
                break;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.Interrupted)
                {
                    EnqueueLog($"Socket exception: {e.Message}");
                }
                break;
            }
            catch (Exception e)
            {
                EnqueueLog($"Error receiving data: {e.Message}");
                break;
            }
        }
    }

    /// <summary>
    /// Enqueues a log message to be processed on the main thread.
    /// </summary>
    /// <param name="message">The log message.</param>
    private void EnqueueLog(string message)
    {
        logQueue.Enqueue(message);
    }

    #endregion
}
