using System;
using System.Collections;
using System.Collections.Generic;
using PythonConnection;
using UnityEngine;

public class ConnectionTest : MonoBehaviour
{
    bool con;

    //Pythonへ送信するデータ形式
    [Serializable]
    private class SendingData
    {
        public SendingData(int testValue0, List<float> testValue1)
        {
            this.testValue0 = testValue0;
            this.testValue1 = testValue1;
        }

        public int testValue0;

        [SerializeField]
        private List<float> testValue1;
    }

    void Connection()
    {
        //データ受信時のコールバックを登録
        PythonConnector.instance.RegisterAction(typeof(SubjugationData), OnDataReceived);

        //Pythonへの接続を開始
        if (PythonConnector.instance.StartConnection())
        {
            Debug.Log("Connected");
            con=true;
        }
        else
        {
            Debug.Log("Connection Failed");
        }
    }

    void Start()
    {
        con = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && !con) Connection();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PythonConnector.instance.StopConnection();
            Debug.Log("Stop");
        }
    }

    public void OnTimeout()
    {
        Debug.Log("Timeout");
    }

    public void OnStop()
    {
        Debug.Log("Stopped");
    }

    //データ受信時のコールバック

    /*
    public void OnDataReceived(DataClass data)
    {
        //DataClass型で渡されてしまうため、明示的に型変換
        SubjugationData testData = data as SubjugationData;

        //受け取り結果表示
        Debug.Log("subjugation_num: " + testData.subjugation_num);

        /*
        //Python側へ送るデータを生成
        int v1 = UnityEngine.Random.Range(0, 100);
        List<float> v2 = new List<float>()
        {
            UnityEngine.Random.Range(0.1f, 0.9f),
            UnityEngine.Random.Range(0.1f, 0.9f)
        };
        SendingData sendingData = new SendingData(v1, v2);

        Debug.Log("Sending Data: " + v1 + ", " + v2[0] + ", " + v2[1]);

        //Python側へ送信
        PythonConnector.instance.Send("test", sendingData);
        */
    //}

    public void OnDataReceived(DataClass data)
    {
        if (data is SubjugationData testData)
        {
            Debug.Log("subjugation_num: " + testData.subjugation_num);
        }
        else
        {
            Debug.LogError("Received data is not of type SubjugationData.");
        }

        // データを生成して送信
        int v1 = UnityEngine.Random.Range(0, 100);
        List<float> v2 = new List<float>
        {
            UnityEngine.Random.Range(0.1f, 0.9f),
            UnityEngine.Random.Range(0.1f, 0.9f)
        };
        SendingData sendingData = new SendingData(v1, v2);
        Debug.Log("Sending Data: " + v1 + ", " + v2[0] + ", " + v2[1]);
        PythonConnector.instance.Send("test", sendingData);
    }
}
