using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --- UIオブジェクト関連 ---
    public GameObject tutorialPanel;         // チュートリアル画面
    public GameObject timerPanel;            // タイマー表示部分
    public GameObject inputNicknamePanel;    // ニックネーム入力パネル
    public InputField inputFieldNickname;    // ニックネーム入力欄

    // --- 外部マネージャ参照 ---
    public ServerManager serverManager;      // サーバー処理を担当
    public LoginManager loginManager;        // ランキングなどのログイン処理を担当

    // --- メイン画像とスプライト関連 ---
    public GameObject mainImage;             // 画面中央の大きな画像（スタート/クリア/ゲームオーバーなどを表示）
    public Sprite gameStartSprite;           // ゲーム開始時のスプライト
    public Sprite gameOverSprite;            // タイムオーバー時のスプライト
    public Sprite gameClearSprite;           // ゲームクリア時のスプライト

    // --- タイマー関連 ---
    public Text txt_m;                       // 分表示用テキスト
    public Text txt_s;                       // 秒表示用テキスト
    public bool isTimerStart;                // タイマーが動作中かどうか
    public float currentTime;                // 残り時間（秒）

    // --- 討伐（進行状況）関連 ---
    public Text subjugationText;             // 残り討伐数を表示
    public int subjugation_num;              // 討伐済み数

    // --- 状態フラグ ---
    public bool isBlotting;                  // 吸い取り中（=敵を吸収中）かどうか
    public bool isRanking;                   // ランキング表示中かどうか
    public GameObject yourRankingImage;      // 「あなたの順位」などの表示用画像
    public GameObject inputImage;            // 入力用UI（使用箇所によって別用途の可能性あり）
    public bool isRanking2;                  // ランキング後の再遷移待ち状態（例：ESCでメインに戻る）

    // --- サウンド関連 ---
    AudioSource audioSource;
    public AudioClip gameStartSound;         // ゲーム開始音
    public AudioClip m2Sound;                // 残り2分の効果音
    public AudioClip m1Sound;                // 残り1分の効果音
    public AudioClip gameClearSound;         // ゲームクリア音
    public AudioClip timeOverSound;          // タイムオーバー音
    public bool ism2;                        // m2Soundを再生済みか判定
    public bool ism1;                        // m1Soundを再生済みか判定

    // ------------------------------
    // ゲーム開始時に呼ばれる処理
    // ------------------------------
    public void Start()
    {
        // 初期UI設定
        inputNicknamePanel.SetActive(true);
        tutorialPanel.SetActive(false);
        timerPanel.SetActive(false);

        // 初期状態設定
        isTimerStart = false;
        isBlotting = false;

        // タイマー初期化（300秒 = 5分）
        currentTime = 300f;

        // 討伐カウント初期化
        subjugation_num = 0;
        subjugationText.text = (5 - subjugation_num).ToString(); // 残り数表示

        // ランキング状態初期化
        isRanking = false;
        yourRankingImage.SetActive(true);
        isRanking2 = false;

        // オーディオ初期化
        audioSource = GetComponent<AudioSource>();
        ism1 = true;
        ism2 = true;
    }

    // ------------------------------
    // 毎フレーム呼ばれる更新処理
    // ------------------------------
    public void Update()
    {
        // --- タイマーが動作中の場合 ---
        if (isTimerStart)
        {
            currentTime -= Time.deltaTime; // 経過時間を減算

            // 残り時間が0以下になった場合
            if (currentTime < 0)
            {
                currentTime = 0f;
                if (!isBlotting) TimeOver(); // 吸い取り中でない場合のみゲームオーバー
            }
            // 残り1分未満で効果音を鳴らす
            else if (currentTime < 60 && ism1)
            {
                ism1 = false;
                audioSource.PlayOneShot(m1Sound);
            }
            // 残り2分未満で効果音を鳴らす
            else if (currentTime < 120 && ism2)
            {
                ism2 = false;
                audioSource.PlayOneShot(m2Sound);
            }

            // タイマー表示を更新
            int i = (int)currentTime;
            int m = i / 60;
            int s1 = (i % 60) / 10;
            int s2 = (i % 60) % 10;
            txt_m.text = m.ToString();
            txt_s.text = s1.ToString() + " " + s2.ToString();
        }

        // --- ランキング画面でEnterを押した場合 ---
        if (isRanking && Input.GetKeyDown(KeyCode.Return))
        {
            isRanking = false;
            yourRankingImage.SetActive(false);
            isRanking2 = true;
        }

        // --- ランキング終了後、Escapeキーでメインに戻る ---
        if (isRanking2 && Input.GetKeyDown(KeyCode.Escape))
        {
            isRanking2 = false;
            SceneManager.LoadScene("main");
        }
    }

    // ------------------------------
    // 「決定」ボタン押下時の処理（ニックネーム入力後）
    // ------------------------------
    public void OnClickDecisionButton()
    {
        if (inputFieldNickname.text.Length >= 3)
        {
            inputNicknamePanel.SetActive(false);
            tutorialPanel.SetActive(true);

            // サーバー開始（例：接続準備）
            serverManager.StartServer();
        }
    }

    // ------------------------------
    // チュートリアル後、ゲームスタート
    // ------------------------------
    public void TimerStart()
    {
        tutorialPanel.SetActive(false);
        timerPanel.SetActive(true);
        isTimerStart = true;

        // 「ゲームスタート」演出
        mainImage.SetActive(true);
        mainImage.GetComponent<Image>().sprite = gameStartSprite;
        audioSource.PlayOneShot(gameStartSound);

        // 2秒後に画像を非表示にする
        Invoke("InactiveSprite", 2.0f);
    }

    // メイン画像を非表示にする
    public void InactiveSprite()
    {
        mainImage.SetActive(false);
    }

    // ------------------------------
    // ゲームクリア処理
    // ------------------------------
    public void gameClear()
    {
        isTimerStart = false;
        mainImage.SetActive(true);
        mainImage.GetComponent<Image>().sprite = gameClearSprite;
        audioSource.PlayOneShot(gameClearSound);

        // 2秒後にランキング画面へ
        Invoke("GoRanking", 2.0f);
    }

    // ------------------------------
    // タイムオーバー処理
    // ------------------------------
    public void TimeOver()
    {
        isTimerStart = false;
        mainImage.SetActive(true);
        mainImage.GetComponent<Image>().sprite = gameOverSprite;
        audioSource.PlayOneShot(timeOverSound);

        // 2秒後にランキング画面へ（メソッド名はtypoっぽいが呼び出し先あり）
        Invoke("GaRanking", 2.0f);
    }

    // ------------------------------
    // ランキング画面表示へ遷移
    // ------------------------------
    private void GoRanking()
    {
        timerPanel.SetActive(false);
        isRanking = true;

        // スコア送信（討伐数と残り時間を送る）
        loginManager.StartRanking(subjugation_num, (int)currentTime);
    }

    // ------------------------------
    // 討伐数更新
    // ------------------------------
    public void Subjugate(int s)
    {
        if (s > 0)
        {
            isBlotting = false;
            subjugation_num = s;
            subjugationText.text = (5 - subjugation_num).ToString();

            // 5体討伐でクリア
            if (s >= 5) gameClear();
        }
    }

    // ------------------------------
    // 吸い取り状態に入る
    // ------------------------------
    public void Blotting()
    {
        isBlotting = true;
    }
}
