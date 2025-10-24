using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteLib;

public class WiiServerManager : MonoBehaviour
{
    private Wiimote wiimote;

    void Start()
    {
        try
        {
            wiimote = new Wiimote();
            wiimote.Connect();
            Debug.Log("Wiiリモコンに接続しました！");

            // ボタンイベントハンドラ登録
            wiimote.WiimoteChanged += OnWiimoteChanged;
            wiimote.WiimoteExtensionChanged += OnExtensionChanged;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Wiiリモコンの接続に失敗しました: " + e.Message);
        }
    }

    private void OnWiimoteChanged(object sender, WiimoteChangedEventArgs args)
    {
        var ws = args.WiimoteState;

        // Aボタンが押された場合
        if (ws.ButtonState.A)
        {
            Debug.Log("Aボタンが押されました！");
        }

        // 十字キーなども例
        if (ws.ButtonState.Up)
        {
            Debug.Log("上ボタンが押されています");
        }
    }

    private void OnExtensionChanged(object sender, WiimoteExtensionChangedEventArgs args)
    {
        Debug.Log("拡張コントローラの接続状態が変わりました");
    }

    void OnApplicationQuit()
    {
        if (wiimote != null)
        {
            wiimote.Disconnect();
        }
    }
}
