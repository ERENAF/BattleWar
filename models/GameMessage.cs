using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaWar.models
{
    public class GameMessage
    {
        public string Command;
        public int X;
        public int Y;
        public string PlayerNickName;
        public string chatText;
        public DateTime Timestamp;
        public ShotResult Result;
    }
}
