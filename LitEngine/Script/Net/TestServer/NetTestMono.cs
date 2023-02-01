using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitEngine.Net;
public class NetTestMono : MonoBehaviour
{
    public enum TestType
    {
        none = 0,
        TCPTest,
        UDPTest,
        KCPTest,
    }
    public TestType type = TestType.TCPTest;

    DataFormat headinfo = new SocketDataFormat(4, 4, DataFormat.CmdPosType.lenFirst, DataFormat.ByteLenType.allbytes);
    SendData testData = null;
    private void Awake()
    {
        testData = new SendData(headinfo, 10);
        testData.AddInt(4);
    }

    void Start()
    {
        switch (type)
        {
            case TestType.TCPTest:
                StartTcpTest();
                break;
            case TestType.UDPTest:
                StartUDPTest();
                break;
            case TestType.KCPTest:
                StartKCPTest();
                break;
            default:
                break;
        }
    }

    void StartTcpTest()
    {
        TCPNet.Init("127.0.0.1", 20240);
        TCPNet.Format = headinfo;
        TCPNet.ShowMsgLog(true);
        TCPNet.Connect();
    }

    void StartUDPTest()
    {
        UDPNet.Init("127.0.0.1", 20236);
        UDPNet.Format = headinfo;
        UDPNet.ShowMsgLog(true);
        UDPNet.Connect();
    }

    void StartKCPTest()
    {
        KCPNet.Init("127.0.0.1", 20250);
        KCPNet.Format = headinfo;
        KCPNet.ShowMsgLog(true);
        KCPNet.Connect();
    }


    void Update()
    {
        switch (type)
        {
            case TestType.TCPTest:
                UpdateTCPTest();
                break;
            case TestType.UDPTest:
                UpdateUDPTest();
                break;
            case TestType.KCPTest:
                UpdateKCPNetTest();
                break;
            default:
                break;
        }
    }

    void UpdateTCPTest()
    {
        if (!TCPNet.IsConnect()) return;
        TCPNet.SendObject(testData);
    }

    void UpdateUDPTest()
    {
        if (!UDPNet.IsConnect()) return;
        UDPNet.SendObject(testData);
    }

    void UpdateKCPNetTest()
    {
        if (!KCPNet.IsConnect()) return;
        KCPNet.SendObject(testData);
    }
}
