using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    // --- サーバー関連 ---
    private TcpListener server;                        
    private Thread serverThread;                       
    private CancellationTokenSource cancellationTokenSource;

    // --- ネットワーク設定 ---
    public int port = 50001;  // ポート番号

    // --- 他コンポーネント参照 ---
    public GameManager gameManager;      

    // --- メインスレッド実行用キュー ---
    private Queue<Action> mainThreadActions = new Queue<Action>();

    // ------------------------------------------------
    // サーバーの起動処理
    // ------------------------------------------------
    public void StartServer()
    {
        // IPAddress.Anyで全ネットワークインターフェースで待機
        server = new TcpListener(IPAddress.Any, port);

        // 停止用トークン作成
        cancellationTokenSource = new CancellationTokenSource();

        // 別スレッドでクライアント待受
        serverThread = new Thread(() => ListenForClients(cancellationTokenSource.Token));
        serverThread.Start();

        Debug.Log($"Server started on port {port}, waiting for connection...");
    }

    // ------------------------------------------------
    // 毎フレーム呼ばれる処理
    // ------------------------------------------------
    public void Update()
    {
        // メインスレッドで安全に実行する必要がある処理を順に実行
        while (mainThreadActions.Count > 0)
        {
            mainThreadActions.Dequeue().Invoke();
        }
    }

    // ------------------------------------------------
    // クライアントの接続を待ち受ける
    // ------------------------------------------------
    private void ListenForClients(CancellationToken cancellationToken)
    {
        try
        {
            server.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError($"Server failed to start: {e.Message}");
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (server.Pending())
                {
                    TcpClient client = server.AcceptTcpClient();
                    HandleClient(client);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            catch (SocketException e)
            {
                Debug.LogError($"Socket Exception: {e.Message}");
            }
        }
    }

    // ------------------------------------------------
    // クライアントからの通信を処理する
    // ------------------------------------------------
    private void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
                catch
                {
                    break; // クライアント切断時に例外が出る場合
                }

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log($"Received message: {message}");

                    if (message == "-5")
                        mainThreadActions.Enqueue(() => gameManager.TimerStart());
                    else if (int.TryParse(message, out int s))
                        mainThreadActions.Enqueue(() => gameManager.Subjugate(s));

                    byte[] response = Encoding.UTF8.GetBytes("Unity received your message!");
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    break; // クライアント切断
                }
            }
        }
    }

    // ------------------------------------------------
    // サーバーを停止する処理
    // ------------------------------------------------
    public void StopServer()
    {
        cancellationTokenSource.Cancel();

        try
        {
            server.Stop();
        }
        catch { }

        if (serverThread != null && serverThread.IsAlive)
            serverThread.Join();

        Debug.Log("Server stopped");
    }
}
