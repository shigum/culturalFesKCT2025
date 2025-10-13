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
    private TcpListener server;                        // クライアントからの接続を待ち受けるサーバー
    private Thread serverThread;                       // サーバーの待受処理を実行する別スレッド
    private CancellationTokenSource cancellationTokenSource; // スレッド停止用のトークン

    // --- ネットワーク設定 ---
    public string ip = "10.202.253.246"; // サーバーのIPアドレス（例: 教室LAN内など）
    public int port = 8080;              // ポート番号（クライアントと一致させる）

    public bool isEnd = false;           // 終了フラグ（未使用だが将来的に停止制御用）

    // --- 他コンポーネント参照 ---
    public GameManager gameManager;      // ゲーム進行制御用マネージャへの参照

    // --- メインスレッド実行用キュー ---
    // Unityではメインスレッド以外からUIやGameObjectを操作できないため、
    // スレッドで受け取ったデータはここに登録し、Update()で処理する。
    private Queue<Action> mainThreadActions = new Queue<Action>();

    // ------------------------------------------------
    // サーバーの起動処理
    // ------------------------------------------------
    public void StartServer()
    {
        // 指定されたIPとポートでサーバーを初期化
        server = new TcpListener(IPAddress.Parse(ip), port);

        // 停止用トークンを作成（後でStop()でキャンセルできる）
        cancellationTokenSource = new CancellationTokenSource();

        // 別スレッドでクライアント待受を開始
        serverThread = new Thread(() => ListenForClients(cancellationTokenSource.Token));
        serverThread.Start();

        Debug.Log("Server started, waiting for connection...");
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

        // ※開発時デバッグ用（手動停止したいとき用）
        // if(Input.GetKeyDown(KeyCode.Return)) Stop();
    }

    // ------------------------------------------------
    // クライアントの接続を待ち受ける（別スレッドで動作）
    // ------------------------------------------------
    private void ListenForClients(CancellationToken cancellationToken)
    {
        server.Start();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // 接続要求があるか確認
                if (server.Pending())
                {
                    // クライアントからの接続を受け付ける
                    TcpClient client = server.AcceptTcpClient();

                    // クライアントごとの処理を実行
                    HandleClient(client);
                }
                else
                {
                    // 接続要求がない場合、CPU負荷軽減のため少し待つ
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
        // usingで自動的にNetworkStreamを破棄
        using (NetworkStream stream = client.GetStream())
        {
            while (true) // クライアントが切断するまで通信を続ける
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // クライアントからのメッセージを受信
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log($"Received message: {message}");

                    // --- 受信したメッセージ内容で処理を分岐 ---
                    if (message == "-5")
                    {
                        // 「-5」はゲームスタート信号として扱う
                        mainThreadActions.Enqueue(() => gameManager.TimerStart());
                    }
                    else if (int.TryParse(message, out int s))
                    {
                        // 数値を受け取った場合は「討伐数更新」として扱う
                        mainThreadActions.Enqueue(() => gameManager.Subjugate(s));
                    }

                    // クライアントへ応答を返す
                    byte[] response = Encoding.UTF8.GetBytes("Unity received your message!");
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    // クライアントが接続を閉じた場合、通信終了
                    break;
                }
            }
        }
    }

    // ------------------------------------------------
    // サーバーを停止する処理
    // ------------------------------------------------
    public void Stop()
    {
        // スレッドに停止を指示
        cancellationTokenSource.Cancel();

        // サーバーソケットを停止
        server.Stop();

        // サーバースレッドの終了を待機（安全に終了させる）
        serverThread.Join();

        Debug.Log("Server stopped");
    }
}
