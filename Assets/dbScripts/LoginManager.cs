using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class LoginManager : MonoBehaviour
{
    // ------------------------------------------------------
    // ランキングデータの構造体
    // ------------------------------------------------------
    struct rankingData
    {
        public string displayName; // プレイヤー名
        public int statValue;      // スコア値

        public rankingData(string _displayName, int _statValue)
        {
            displayName = _displayName;
            statValue = _statValue;
        }
    }

    // ランキング上位10件を格納する配列
    rankingData[] rankingDatas = new rankingData[10];

    // 現在ログイン中のプレイヤー名
    public string PlayerName = "";

    // ランキング表示用のText（1〜10位）
    public Text text1;
    public Text text2;
    public Text text3;
    public Text text4;
    public Text text5;
    public Text text6;
    public Text text7;
    public Text text8;
    public Text text9;
    public Text text10;

    // 自分の順位を表示するテキスト
    public Text yourNametext;

    // ニックネーム入力フィールド
    [SerializeField]
    private InputField inputFieldNickname;

    // ------------------------------------------------------
    // ★ ランキング送信のエントリーポイント
    // ゲーム終了時にGameManagerから呼ばれる
    // ------------------------------------------------------
    public void StartRanking(int score, int time)
    {
        // ニックネームが空でなければ送信開始
        if (inputFieldNickname.text != "")
        {
            PlayerName = inputFieldNickname.text;
            UpdateRanking(inputFieldNickname.text, score, time);
        }
    }

    // ------------------------------------------------------
    // 新規プレイヤー用の一意なIDを作成
    // （PlayFabにアカウントを作るために必要）
    // ------------------------------------------------------
    private string CreateNewPlayerId()
    {
        return System.Guid.NewGuid().ToString("N"); // ランダムな一意IDを生成
    }

    // ------------------------------------------------------
    // ① PlayFabにログイン（なければ新規アカウント作成）
    // ------------------------------------------------------
    public void UpdateRanking(string userName, int score, int time)
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = CreateNewPlayerId(), // 一意のIDでログイン
            CreateAccount = true            // アカウントが無ければ作成
        };

        PlayFabClientAPI.LoginWithCustomID(request,
        (result) =>
        {
            // 既に同じIDのアカウントが存在した場合
            if (!result.NewlyCreated)
            {
                Debug.LogWarning("Already account exists, retrying...");
                // 新しいIDで再試行
                UpdateRanking(userName, score, time);
                return;
            }

            // 新規アカウント作成成功
            Debug.Log("Create Account Success!!");
            SetUserName(userName, score, time); // 次にユーザー名登録へ
        },
        (error) =>
        {
            Debug.LogError("Create Account Failed...");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    // ------------------------------------------------------
    // ② 作成したアカウントにユーザー名（ニックネーム）を設定
    // ------------------------------------------------------
    public void SetUserName(string userName, int score, int time)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(
        new UpdateUserTitleDisplayNameRequest { DisplayName = userName },
        (result) =>
        {
            Debug.Log("Save Display Name Success!!");
            SendUserScore(score, time); // 次にスコア送信へ
        },
        (error) =>
        {
            Debug.LogError("Save Display Name Failed...");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    // ------------------------------------------------------
    // ③ ランキングスコア送信（PlayFabの統計データとして保存）
    // ------------------------------------------------------
    public void SendUserScore(int score, int time)
    {
        // 「Subjugation」というランキング名にスコアを送信
        // スコアは「討伐数 × 1000 + 残り時間」で計算される（大きいほど上位）
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Subjugation",
                    Value = score * 1000 + time
                }
            }
        },
        (result) =>
        {
            Debug.Log("Send Ranking Score Success!!");
            // 少し待ってからランキング取得
            Invoke(nameof(GetLeaderboard), 3f);
        },
        (error) =>
        {
            Debug.LogError("Send Ranking Score Failed...");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    // ------------------------------------------------------
    // ④ ランキング取得（PlayFabから上位10人を取得）
    // ------------------------------------------------------
    public void GetLeaderboard()
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
        {
            StatisticName = "Subjugation"
        },
        (result) =>
        {
            // 上位10位までを配列に格納
            int i = 0;
            foreach (var item in result.Leaderboard)
            {
                if (0 <= i && i <= 9)
                    rankingDatas[i] = new rankingData(item.DisplayName, item.StatValue);

                // 自分の順位を検出
                if (PlayerName == item.DisplayName)
                {
                    Debug.Log($"{PlayerName}'s ranking is {i + 1}!!");
                    yourNametext.text = $"あなたの順位は{i + 1}位です";
                }
                i++;
            }

            // --- ランキングをUIに表示 ---
            for (int j = 0; j < 10; j++)
            {
                Debug.Log($"{j + 1}位: {rankingDatas[j].displayName} スコア {rankingDatas[j].statValue}");
            }

            // Text UIに上位10人を反映
            text1.text = $"1位: {rankingDatas[0].displayName} スコア {rankingDatas[0].statValue}";
            text2.text = $"2位: {rankingDatas[1].displayName} スコア {rankingDatas[1].statValue}";
            text3.text = $"3位: {rankingDatas[2].displayName} スコア {rankingDatas[2].statValue}";
            text4.text = $"4位: {rankingDatas[3].displayName} スコア {rankingDatas[3].statValue}";
            text5.text = $"5位: {rankingDatas[4].displayName} スコア {rankingDatas[4].statValue}";
            text6.text = $"6位: {rankingDatas[5].displayName} スコア {rankingDatas[5].statValue}";
            text7.text = $"7位: {rankingDatas[6].displayName} スコア {rankingDatas[6].statValue}";
            text8.text = $"8位: {rankingDatas[7].displayName} スコア {rankingDatas[7].statValue}";
            text9.text = $"9位: {rankingDatas[8].displayName} スコア {rankingDatas[8].statValue}";
            text10.text = $"10位: {rankingDatas[9].displayName} スコア {rankingDatas[9].statValue}";
        },
        (error) =>
        {
            Debug.Log(error.GenerateErrorReport());
        });
    }
}
