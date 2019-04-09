using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class GameInfo
    {
        public GameInfo (string region, string title, int readOffset, int gameID)
        {
            Region = region;
            Title = title;
            ReadOffset = readOffset;
            GameID = gameID;
        }

        public string Region { get; set; }
        public string Title { get; set; }
        public int ReadOffset { get; set; }
        public int GameID { get; set; }
    }
}
