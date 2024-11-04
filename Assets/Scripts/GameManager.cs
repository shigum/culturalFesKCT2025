using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    public bool isRanking;
    public GameObject yourRankingImage;

    public GameObject inputImage;
    public bool isRanking2;

    AudioSource audioSource;
    public AudioClip gameStartSound;
    public AudioClip m2Sound;
    public AudioClip m1Sound;
    public AudioClip gameClearSound;
    public AudioClip timeOverSound;
    public bool ism2;
    public bool ism1;

    public void Start()
    {
        inputNicknamePanel.SetActive(true);
        tutorialPanel.SetActive(false);
        timerPanel.SetActive(false);

        isTimerStart = false;
        isBlotting = false;

        currentTime = 300f;
        subjugation_num = 0;
        subjugationText.text = (5 - subjugation_num).ToString();

        isRanking = false;
        yourRankingImage.SetActive(true);
        isRanking2 = false;

        audioSource = GetComponent<AudioSource>();
        ism1 = true;
        ism2 = true;
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
            else if(currentTime < 60 && ism1)
            {
                ism1 = false;
                audioSource.PlayOneShot(m1Sound);
            }
            else if(currentTime < 120 && ism2)
            {
                ism2 = false;
                audioSource.PlayOneShot(m2Sound);
            }
            int i = (int)currentTime;
            int m = i / 60;
            int s1 = (i % 60) / 10;
            int s2 = (i % 60) % 10;
            txt_m.text = m.ToString();
            txt_s.text = s1.ToString() + " " + s2.ToString();
        }
        if(isRanking && Input.GetKeyDown(KeyCode.Return))
        {
            isRanking = false;
            yourRankingImage.SetActive(false);
            isRanking2 = true;
        }
        if(isRanking2 && Input.GetKeyDown(KeyCode.Escape))
        {
            isRanking2 = false;
            SceneManager.LoadScene("main");
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
        audioSource.PlayOneShot(gameStartSound);
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
        audioSource.PlayOneShot(gameClearSound);
        Invoke("GoRanking", 2.0f);
    }

    public void TimeOver()
    {
        isTimerStart = false;
        mainImage.SetActive(true);
        mainImage.GetComponent<Image>().sprite = gameOverSprite;
        audioSource.PlayOneShot(timeOverSound);
        Invoke("GaRanking", 2.0f);
    }

    private void GoRanking()
    {
        timerPanel.SetActive(false);
        isRanking = true;
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
