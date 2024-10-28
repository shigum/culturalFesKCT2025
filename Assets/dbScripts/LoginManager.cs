using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

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

    [SerializeField]
    private InputField inputFieldNickname; //ニックネーム
    [SerializeField]
    private InputField inputFieldScore; //討伐数
    [SerializeField]
    private InputField inputFieldTime; //残り時間(s)
    
    public void OnClickUpdateRanking()
    {
        if(inputFieldNickname.text != "" && inputFieldScore.text != "" && inputFieldTime.text != "")
        {
            PlayerName = inputFieldNickname.text;
            UpdateRanking(inputFieldNickname.text, int.Parse(inputFieldScore.text), int.Parse(inputFieldTime.text));
        }
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
        }, result =>
        {
            //1～10位までを配列に代入 & プレイヤーの順位を検索
            int i = 0;
            foreach (var item in result.Leaderboard)
            {
                if(0 <= i && i <= 9) rankingDatas[i] = new rankingData(item.DisplayName, item.StatValue);
                if(PlayerName == item.DisplayName)
                {
                    Debug.Log(PlayerName + "'s ranking is " + (i + 1).ToString() + "!!");
                }
                i++;
            }
            //1～10位までのプレイヤーを表示
            for(int j = 0; j < 10;j++)
            {
                Debug.Log($"{j + 1}位:{rankingDatas[j].displayName} " + $"スコア {rankingDatas[j].statValue}");
            }
        }, error =>
        {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    private string CreateNewPlayerId()
    {
        return System.Guid.NewGuid().ToString("N");
    }
}
