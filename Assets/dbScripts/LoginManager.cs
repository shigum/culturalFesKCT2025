using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class LoginManager : MonoBehaviour
{
    struct rankingData{
	    public string displayName;
        public int statValue;

	    public rankingData(string _displayName, int _statValue){
            displayName = _displayName;
            statValue = _statValue;
	    }
    }

    rankingData[] rankingDatas = new rankingData[10];

    public string PlayerName = "";
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

    [SerializeField]
    private InputField inputFieldNickname; //ニックネーム
    
    public void StartRanking(int score, int time) //ランキングにデータを送る
    {
        if(inputFieldNickname.text != "")
        {
            PlayerName = inputFieldNickname.text;
            UpdateRanking(inputFieldNickname.text, score, time);
        }
    }

    private string CreateNewPlayerId()
    {
        return System.Guid.NewGuid().ToString("N");
    }

    public void UpdateRanking(string userName, int score, int time)
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = CreateNewPlayerId(),
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request,
        (result) =>
        {
            // 既に作成済みだった場合
            if (!result.NewlyCreated)
            {
                Debug.LogWarning("already account");
                // 再度ログイン
                UpdateRanking(userName, score, time);
                
                return;
            }

            // アカウント作成完了
            Debug.Log("Create Account Success!!");
            SetUserName(userName, score, time);
        },
        (error) =>
        {
            Debug.LogError("Create Account Failed...");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    public void SetUserName(string userName, int score, int time)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(
        new UpdateUserTitleDisplayNameRequest { DisplayName = userName },
        (result) =>
        {
            Debug.Log("Save Display Name Success!!");
            SendUserScore(score, time);
        },
        (error) =>
        {
            Debug.LogError("Save Display Name Failed...");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    public void SendUserScore(int score, int time)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Subjugation", //ランキング名
                    Value = score * 1000 + time, // スコア(大きいほど高順位)
                }
            }
        },
        (result) =>
        {
            // スコア送信完了
            Debug.Log("Send Ranking Score Success!!");
            Invoke(nameof(GetLeaderboard), 3f);
        },
        (error) =>
        {
            Debug.LogError("Send Ranking Score Failed...");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    public void GetLeaderboard()
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
        {
            StatisticName = "Subjugation"
        }, 
        (result) =>
        {
            //1～10位までを配列に代入 & プレイヤーの順位を検索
            int i = 0;
            foreach (var item in result.Leaderboard)
            {
                if (0 <= i && i <= 9) rankingDatas[i] = new rankingData(item.DisplayName, item.StatValue);
                if (PlayerName == item.DisplayName)
                {
                    Debug.Log(PlayerName + "'s ranking is " + (i + 1).ToString() + "!!");
                }
                i++;
            }
            //1～10位までのプレイヤーを表示
            for (int j = 0; j < 10; j++)
            {
                Debug.Log($"{j + 1}位:{rankingDatas[j].displayName} " + $"スコア {rankingDatas[j].statValue}");
            }
            text1.text = $"{1}位:{rankingDatas[1].displayName} " + $"スコア {rankingDatas[1].statValue}";
            text2.text = $"{2}位:{rankingDatas[2].displayName} " + $"スコア {rankingDatas[2].statValue}";
            text3.text = $"{3}位:{rankingDatas[3].displayName} " + $"スコア {rankingDatas[3].statValue}";
            text4.text = $"{4}位:{rankingDatas[4].displayName} " + $"スコア {rankingDatas[4].statValue}";
            text5.text = $"{5}位:{rankingDatas[5].displayName} " + $"スコア {rankingDatas[5].statValue}";
            text6.text = $"{6}位:{rankingDatas[6].displayName} " + $"スコア {rankingDatas[6].statValue}";
            text7.text = $"{7}位:{rankingDatas[7].displayName} " + $"スコア {rankingDatas[7].statValue}";
            text8.text = $"{8}位:{rankingDatas[8].displayName} " + $"スコア {rankingDatas[8].statValue}";
            text9.text = $"{9}位:{rankingDatas[9].displayName} " + $"スコア {rankingDatas[9].statValue}";
            text10.text = $"{10}位:{rankingDatas[10].displayName} " + $"スコア {rankingDatas[10].statValue}";
         }, 
        (error) =>
        {
            Debug.Log(error.GenerateErrorReport());
        });
    }
}
