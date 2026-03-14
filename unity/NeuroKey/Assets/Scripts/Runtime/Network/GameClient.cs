using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NeuroKey.Network
{
    public class GameClient : MonoBehaviour
    {
        public static GameClient Instance { get; private set; }

        private ClientWebSocket socket;
        private CancellationTokenSource cts;
        private PacketManager packetManager = new PacketManager();

        public event Action<Packet> OnPacketReceived;
        public bool IsConnected => socket != null && socket.State == WebSocketState.Open;

        [SerializeField] private string serverUrl = "ws://127.0.0.1:8887";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task Connect()
        {
            if (IsConnected) return;

            socket = new ClientWebSocket();
            cts = new CancellationTokenSource();

            try
            {
                await socket.ConnectAsync(new Uri(serverUrl), cts.Token);
                Debug.Log("Connected to server");
                _ = ReceiveLoop();

                // Send handshake
                SendPacket(new HandShakePacket("unity_game"));
            }
            catch (Exception e)
            {
                Debug.LogError("Connection failed: " + e.Message);
            }
        }

        public async void SendPacket(Packet packet)
        {
            if (!IsConnected) return;

            byte[] data = packet.Encode();
            await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, cts.Token);
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token);
                    }
                    else
                    {
                        // Handle binary packet
                        byte[] packetData = new byte[result.Count];
                        Buffer.BlockCopy(buffer, 0, packetData, 0, result.Count);
                        
                        // We need to handle multiple segments if needed, but for simplicity assume one segment for now
                        try {
                            Packet packet = Packet.Decode(packetData, packetManager);
                            OnPacketReceived?.Invoke(packet);
                        } catch (Exception e) {
                            Debug.LogError("Failed to decode packet: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (socket.State != WebSocketState.Closed)
                    Debug.LogError("Receive error: " + e.Message);
            }
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            socket?.Dispose();
        }
    }
}
