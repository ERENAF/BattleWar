using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeaWar.models
{
    public class NetworkMessage
    {
        public string Type;
        public string Data;
        public DateTime TimeStamp;

        public NetworkMessage() 
        { 
            TimeStamp = DateTime.Now; 
        }

        public NetworkMessage(string type, string data)
        {
            Type = type; 
            Data = data;
        }

        public string ToJson()
        {
            try
            {
                return JsonSerializer.Serialize(this);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static NetworkMessage FromJson(string fileJSON)
        {
            try
            {
                return JsonSerializer.Deserialize<NetworkMessage>(fileJSON);
            }
            catch
            {
                return null;
            }
        }

        public static NetworkMessage CreateConnectMessage(string playerName)
        {
            var data = JsonSerializer.Serialize(new { PlayerName = playerName });
            return new NetworkMessage("Connect", data);
        }

        public static NetworkMessage CreateShotMessage(int x, int y, string playerName)
        {
            var data = JsonSerializer.Serialize(new { X = x, Y = y, PlayerName = playerName });
            return new NetworkMessage("Shot", data);
        }

        public static NetworkMessage CreateResultMessage(int x, int y, string result)
        {
            var data = JsonSerializer.Serialize(new { X = x, Y = y, Result = result });
            return new NetworkMessage("Result", data);
        }

        public static NetworkMessage CreateChatMessage(string playerName, string text)
        {
            var data = JsonSerializer.Serialize(new { PlayerName = playerName, Text = text });
            return new NetworkMessage("Chat", data);
        }

        public static NetworkMessage CreateStartGameMessage()
        {
            return new NetworkMessage("StartGame", "");
        }

        public static NetworkMessage CreateReadyMessage(bool isReady)
        {
            var data = JsonSerializer.Serialize(new { IsReady = isReady });
            return new NetworkMessage("Ready", data);
        }

        public static NetworkMessage CreateReadyMessage(string playerName)
        {
            var data = JsonSerializer.Serialize(new
            {
                PlayerName = playerName,
                IsReady = true
            });
            return new NetworkMessage("Ready", data);
        }

    }


}
