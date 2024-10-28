using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    private TcpListener server;

    public string ip;
    public int port;

    /*
    public TCPServer(string ip, int port)
    {
        server = new TcpListener(IPAddress.Parse(ip), port);
    }
    */

    public void Start()
    {
        server = new TcpListener(IPAddress.Parse(ip), port);
        server.Start();
        Console.WriteLine("Server started, waiting for connection...");
    }

    public void Update()
    {
        TcpClient client = server.AcceptTcpClient();
        Console.WriteLine("Client connected");
        
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Received message: {message}");
        // Send response back to client
        byte[] response = Encoding.UTF8.GetBytes("Hello from C#!");
        stream.Write(response, 0, response.Length);
        client.Close();
    }

    public void Stop()
    {
        server.Stop();
    }
}

/*
class Program 
{
    public void Start()
    {
        TCPServer server = new TCPServer("127.0.0.1", 8080);
        server.Start();
    }
}
*/