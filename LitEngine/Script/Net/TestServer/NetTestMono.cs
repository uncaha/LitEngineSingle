using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitEngine.Net;
using LitEngine.Net.TestServer;

public class NetTestMono : MonoBehaviour
{
    public enum TestType
    {
        none = 0,
        TCPTest,
        UDPTest,
        KCPTest,
        WebSocketTest,
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
            case TestType.WebSocketTest:
                StartWebSocketTest();
                break;
            default:
                break;
        }
    }

    void StartTcpTest()
    {
        var server = gameObject.AddComponent<ServerTestTCP>();
        server.InitServer();
        
        TCPNet.Init("127.0.0.1", 20240);
        TCPNet.Format = headinfo;
        TCPNet.ShowMsgLog(true);
        TCPNet.Connect();
        
    }

    void StartUDPTest()
    {
        var server = gameObject.AddComponent<ServerTest>();
        server.InitServer();
        
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
    
    void StartWebSocketTest()
    {
        WebSocketNet.Init("127.0.0.1:20260", 20260);
        WebSocketNet.Format = headinfo;
        WebSocketNet.ShowMsgLog(true);
        WebSocketNet.Connect();
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
            case TestType.WebSocketTest:
                UpdateWebSocketNetTest();
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
    
    void UpdateWebSocketNetTest()
    {
        if (!WebSocketNet.IsConnect()) return;
        WebSocketNet.SendObject(testData);
    }
}
