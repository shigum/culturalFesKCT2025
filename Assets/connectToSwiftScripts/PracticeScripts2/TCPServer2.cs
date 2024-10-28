using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer2 : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private CancellationTokenSource cancellationTokenSource;

    public string ip = "10.202.227.43";
    public int port = 8080;

    private bool isFirst;

    public void Start()
    {   
        isFirst = true;
        server = new TcpListener(IPAddress.Parse(ip), port);
        cancellationTokenSource = new CancellationTokenSource();
        serverThread = new Thread(() => ListenForClients(cancellationTokenSource.Token));
        serverThread.Start();
        Debug.Log("Server started, waiting for connection...");
    }

    public void Update()
    {
        if(Input.GetKeyUp(KeyCode.Return)) Stop();
    }

    private void ListenForClients(CancellationToken cancellationToken)
    {
        server.Start();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if(server.Pending())
                {
                    TcpClient client = server.AcceptTcpClient();
                    HandleClient(client, isFirst);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            catch (SocketException e)
            {
                Debug.LogError($"Socket Exceptions: {e.Message}");
            }
        }
    }

    private void HandleClient(TcpClient client, bool isFirst)
    {
        if(isFirst) Debug.Log("Client connected");
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log($"Received message: {message}");
            if(message == "-1")
            {
                client.Close();
                Stop();
            }

            if(isFirst)
            {
                isFirst = false;
                // クライアントに応答を送信
                byte[] response = Encoding.UTF8.GetBytes("Hello from Unity");
                stream.Write(response, 0, response.Length);
            }
        }
        //client.Close();
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel(); // スレッドに停止を指示
        server.Stop();
        serverThread.Join(); // サーバースレッドが終了するのを待つ
        Debug.Log("Server stopped");
    }
}
