using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private CancellationTokenSource cancellationTokenSource;

    public string ip = "10.202.253.246"; //192.168.3.13, 10.202.227.43, 10.202.253.246
    public int port = 8080;

    public bool isEnd = false;

    public GameManager gameManager;

    // メインスレッドで実行するアクションを保持するキュー
    private Queue<Action> mainThreadActions = new Queue<Action>();

    public void StartServer()
    {   
        server = new TcpListener(IPAddress.Parse(ip), port);
        cancellationTokenSource = new CancellationTokenSource();
        serverThread = new Thread(() => ListenForClients(cancellationTokenSource.Token));
        serverThread.Start();
        Debug.Log("Server started, waiting for connection...");
    }

    public void Update()
    {
        // キューに溜まったアクションを実行
        while(mainThreadActions.Count > 0)
        {
            mainThreadActions.Dequeue().Invoke();
        }

        //if(Input.GetKeyDown(KeyCode.Return)) Stop();
    }

    private void ListenForClients(CancellationToken cancellationToken)
    {
        server.Start();
        while(!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if(server.Pending())
                {
                    TcpClient client = server.AcceptTcpClient();
                    HandleClient(client);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            catch(SocketException e)
            {
                Debug.LogError($"Socket Exceptions: {e.Message}");
            }
        }
    }

    /*
    private void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if(bytesRead > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log($"Received message: {message}");

                if(message == "-5")
                {
                    //メインスレッドに戻るアクションをキューに追加->いらないかも
                    mainThreadActions.Enqueue(() =>
                    {
                        gameManager.TimerStart(); //タイマースタート
                    });
                }
                else if(message == "-6")
                {
                    gameManager.Blotting(); //吸い取り中
                }
                else if(message == "-10")
                {
                    isEnd = true;
                    client.Close();
                    gameManager.gameClear(); //ゲームクリア
                }
                else if(int.TryParse(message, out int s))
                {
                    gameManager.Subjugate(s); //お化けを1体討伐
                }

                // クライアントに応答を送信
                byte[] response = Encoding.UTF8.GetBytes("Unity is received message!");
                stream.Write(response, 0, response.Length);

                if(!isEnd) StartServer();
            }
        }

        // 通信後にクライアントの接続を閉じる
        client.Close();
    }
    */


    private void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            while (true) // クライアントとの通信を繰り返す
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log($"Received message: {message}");
                    /*
                    if(message == "-4") 
                    {
                        client.Close();
                        StartServer();
                    }
                    */
                    if (message == "-5")
                    {
                        mainThreadActions.Enqueue(() => gameManager.TimerStart());
                    }
                    else if (message == "-6")
                    {
                        gameManager.Blotting();
                    }
                    else if (message == "-10")
                    {
                        isEnd = true;
                        client.Close();
                        gameManager.gameClear();
                        break; // 通信終了
                    }
                    else if (int.TryParse(message, out int s))
                    {
                        gameManager.Subjugate(s);
                    }

                    byte[] response = Encoding.UTF8.GetBytes("Unity received your message!");
                    stream.Write(response, 0, response.Length);
                    //if(!isEnd) StartServer();
                }
                else
                {
                    // クライアントが接続を閉じた場合
                    break;
                }
            }
        }
    }
    


    public void Stop()
    {
        cancellationTokenSource.Cancel(); //スレッドに停止を指示
        server.Stop();
        serverThread.Join(); //サーバースレッドが終了するのを待つ
        Debug.Log("Server stopped");
    }
}
