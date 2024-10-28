using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class ServerManager : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private CancellationTokenSource cancellationTokenSource;

    public string ip = "10.202.227.43"; //192.168.3.13, 10.202.227.43
    public int port = 8080;

    private bool isFirst;
    public GameObject connectingPanel;

    // メインスレッドで実行するアクションを保持するキュー
    private Queue<Action> mainThreadActions = new Queue<Action>();

    public void StartServer()
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
        // キューに溜まったアクションを実行
        while (mainThreadActions.Count > 0)
        {
            mainThreadActions.Dequeue().Invoke();
        }

        if(Input.GetKeyDown(KeyCode.Return)) Stop();
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
        if(isFirst)
        {
            Debug.Log("Client connected");

            // メインスレッドに戻るアクションをキューに追加
            mainThreadActions.Enqueue(() =>
            {
                connectingPanel.SetActive(false);

                // クライアントに応答を送信
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] response = Encoding.UTF8.GetBytes("Hello from Unity");
                    stream.Write(response, 0, response.Length);
                }
            });
        }
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log($"Received message: {message}");
            if(message == "-10")
            {
                Debug.Log("クライアントとの通信を終了しました");
                client.Close();
                Stop();
            }

            if(isFirst)
            {
                isFirst = false;
                connectingPanel.SetActive(false);

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
