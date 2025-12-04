using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SeaWar.models
{
    public class NetworkManager
    {
        public TcpListener Listener { get; set; }
        public TcpClient Client { get; set; }
        public bool IsHost { get; set; }
        public string HostIP { get; set; }
        public int Port { get; set; }
        public event Action<NetworkMessage> MessageReceived;

        public NetworkManager()
        {
            Port = 8888;
            HostIP = "127.0.0.1";
        }

        public async Task StartHostAsync()
        {
            try
            {
                IsHost = true;
                Listener = new TcpListener(IPAddress.Any, Port);
                Listener.Start();

                Client = await Listener.AcceptTcpClientAsync();

                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка запуска хоста: {ex.Message}");
            }
        }

        public async Task ConnectAsync(string ip, int port)
        {
            try
            {
                IsHost = false;
                Port = port;
                HostIP = ip;

                Client = new TcpClient();
                await Client.ConnectAsync(ip, port);

                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка подключения: {ex.Message}");
            }
        }


        public async Task SendMessageAsync(NetworkMessage message)
        {
            if (Client == null || !Client.Connected)
                return;

            try
            {
                string json = message.ToJson();
                byte[] data = Encoding.UTF8.GetBytes(json + "\n");

                NetworkStream stream = Client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception)
            {
                // Игнорируем ошибки отправки
            }
        }

        public async Task ReceiveMessagesAsync()
        {
            if (Client == null)
                return;

            try
            {
                NetworkStream stream = Client.GetStream();
                byte[] buffer = new byte[1024];
                StringBuilder messageBuilder = new StringBuilder();

                while (Client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(data);

                    string[] messages = messageBuilder.ToString().Split('\n');

                    for (int i = 0; i < messages.Length - 1; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(messages[i]))
                        {
                            NetworkMessage message = NetworkMessage.FromJson(messages[i]);
                            if (message != null)
                            {
                                MessageReceived?.Invoke(message);
                            }
                        }
                    }

                    messageBuilder.Clear();
                    if (messages.Length > 0 && !string.IsNullOrWhiteSpace(messages[messages.Length - 1]))
                    {
                        messageBuilder.Append(messages[messages.Length - 1]);
                    }
                }
            }
            catch (Exception)
            {
                // Соединение разорвано
            }
        }

        public void Disconnect()
        {
            try
            {
                Client?.Close();
                Listener?.Stop();
            }
            catch (Exception)
            {
                // Игнорируем ошибки при отключении
            }
        }
    }
}
