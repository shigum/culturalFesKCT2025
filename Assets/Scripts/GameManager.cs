using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public InputField inputFieldNickname;
    public ServerManager serverManager;
    public GameObject inputNicknamePanel;
    
    public GameObject connectingPanel; //後で消す
    public bool isConnectingPanel;
    public GameObject tutorialPanel;
    public GameObject timerPanel; //後で消す

    public Text txt_m;
    public Text txt_s;
    public bool isTimerStart;
    public float currentTime = 180f; //現在の時間

    public void Start()
    {
        inputNicknamePanel.SetActive(true);

        isConnectingPanel = true;
        connectingPanel.SetActive(true);
        tutorialPanel.SetActive(false);

        isTimerStart = false;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isConnectingPanel)
        {
            isConnectingPanel = false;
            connectingPanel.SetActive(false);
            tutorialPanel.SetActive(true);
        }

        if(isTimerStart)
        {
            currentTime -= Time.deltaTime;
            if(currentTime < 0)
            {
                currentTime = 0f;
                timerPanel.SetActive(false);
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
            serverManager.StartServer();
        } 
    }
}
