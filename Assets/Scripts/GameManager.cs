using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject timerPanel;
    public GameObject inputNicknamePanel;
    public InputField inputFieldNickname;

    public ServerManager serverManager;
    public LoginManager loginManager;

    public GameObject mainImage;
    public Sprite gameStartSprite;
    public Sprite gameOverSprite;
    public Sprite gameClearSprite;

    //タイマー
    public Text txt_m;
    public Text txt_s;
    public bool isTimerStart;
    public float currentTime; //現在の時間

    public Text subjugationText;
    public int subjugation_num;

    public bool isBlotting; //吸い取り中か判定

    public void Start()
    {
        inputNicknamePanel.SetActive(true);
        tutorialPanel.SetActive(false);

        isTimerStart = false;
        isBlotting = false;

        currentTime = 180f;
        subjugation_num = 0;
        subjugationText.text = (5 - subjugation_num).ToString();
    }

    public void Update()
    {
        if(isTimerStart)
        {
            currentTime -= Time.deltaTime;
            if(currentTime < 0)
            {
                currentTime = 0f;
                if(!isBlotting) TimeOver();
            } 
            int i = (int)currentTime;
            int m = i / 60;
            int s1 = (i % 60) / 10;
            int s2 = (i % 60) % 10;
            txt_m.text = m.ToString();
            txt_s.text = s1.ToString() + " " + s2.ToString();
        }
    }

    public void OnClickDecisionButton()
    {
        if(inputFieldNickname.text.Length >= 3)
        {
            inputNicknamePanel.SetActive(false);
            tutorialPanel.SetActive(true);

            serverManager.StartServer();
        } 
    }

    public void TimerStart()
    {
        tutorialPanel.SetActive(false);
        timerPanel.SetActive(true);
        isTimerStart = true;
        mainImage.SetActive(true);
        mainImage.GetComponent<Image>().sprite = gameStartSprite;
        Invoke("InactiveSprite", 2.0f);
    }

    public void InactiveSprite()
    {
        mainImage.SetActive(false);
    }

    public void gameClear()
    {
        isTimerStart = false;
        mainImage.SetActive(true);
        mainImage.GetComponent<Image>().sprite = gameClearSprite;
        Invoke("GoRanking", 2.0f);
    }

    public void TimeOver()
    {
        isTimerStart = false;
        mainImage.SetActive(true);
        mainImage.GetComponent<Image>().sprite = gameOverSprite;
        Invoke("GaRanking", 2.0f);
    }

    private void GoRanking()
    {
        timerPanel.SetActive(false);
        loginManager.StartRanking(subjugation_num, (int)currentTime);
    }

    public void Subjugate(int s)
    {
        if(s > 0)
        {
            isBlotting = false;
            subjugation_num = s;
            subjugationText.text = (5 - subjugation_num).ToString();
            if(s >= 5) gameClear();
        }
    }

    public void Blotting()
    {
        isBlotting = true;
    }
}
