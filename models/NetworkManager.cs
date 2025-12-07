using SeaWar.models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        private CancellationTokenSource receiveCancellationToken;
        private bool isRunning = false;

        public NetworkManager()
        {
            Port = 8888;
            HostIP = "127.0.0.1";
            receiveCancellationToken = new CancellationTokenSource();
        }

        public async Task StartHostAsync()
        {
            try
            {
                IsHost = true;

                Stop();

                Listener = new TcpListener(IPAddress.Any, Port);
                Listener.Start();

                Console.WriteLine($"[Network] Сервер запущен на порту {Port}");

                var acceptTask = Listener.AcceptTcpClientAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));

                var completedTask = await Task.WhenAny(acceptTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Listener.Stop();
                    throw new Exception("Таймаут ожидания подключения (30 секунд)");
                }

                Client = await acceptTask;
                Console.WriteLine("[Network] Клиент подключился!");

                StartReceiving();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Ошибка запуска сервера: {ex.Message}");
                Stop();
                throw;
            }
        }

        public async Task ConnectAsync(string ip, int port)
        {
            try
            {
                IsHost = false;
                HostIP = ip;
                Port = port;

                Stop();

                Console.WriteLine($"[Network] Подключаемся к {ip}:{port}...");

                Client = new TcpClient();

                var connectTask = Client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Client.Close();
                    throw new Exception($"Не удалось подключиться за 10 секунд");
                }

                await connectTask;

                Console.WriteLine("[Network] Подключение успешно!");

                StartReceiving();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Ошибка подключения: {ex.Message}");
                Stop();
                throw;
            }
        }

        private void StartReceiving()
        {
            if (isRunning) return;

            isRunning = true;
            receiveCancellationToken = new CancellationTokenSource();

            Task.Run(() => ReceiveLoop(), receiveCancellationToken.Token);
        }

        private async Task ReceiveLoop()
        {
            try
            {
                NetworkStream stream = Client.GetStream();
                byte[] buffer = new byte[1024];

                while (isRunning &&
                       Client != null &&
                       Client.Connected &&
                       !receiveCancellationToken.Token.IsCancellationRequested)
                {
                    if (!stream.DataAvailable)
                    {
                        await Task.Delay(50);
                        continue;
                    }

                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[Network] Соединение закрыто");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessReceivedData(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Ошибка приема: {ex.Message}");
            }
            finally
            {
                Stop();
            }
        }

        private void ProcessReceivedData(string data)
        {
            string[] messages = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string message in messages)
            {
                try
                {
                    var networkMessage = NetworkMessage.FromJson(message);
                    if (networkMessage != null)
                    {
                        MessageReceived?.Invoke(networkMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Network] Ошибка обработки сообщения: {ex.Message}");
                }
            }
        }

        public async Task SendMessageAsync(NetworkMessage message)
        {
            try
            {
                if (Client == null || !Client.Connected)
                {
                    Console.WriteLine("[Network] Не могу отправить: нет соединения");
                    return;
                }

                string json = message.ToJson();
                byte[] data = Encoding.UTF8.GetBytes(json + "\n");

                NetworkStream stream = Client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();

                Console.WriteLine($"[Network] Отправлено: {message.Type}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Ошибка отправки: {ex.Message}");
                Stop();
            }
        }

        public void Stop()
        {
            isRunning = false;
            receiveCancellationToken?.Cancel();

            try
            {
                Client?.Close();
                Listener?.Stop();
            }
            catch { }

            Client = null;
            Listener = null;

            Console.WriteLine("[Network] Остановлено");
        }

        public bool IsConnected => Client?.Connected ?? false;
    }
}