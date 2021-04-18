using Microsoft.Xna.Framework;
using TShockAPI;

namespace TerrariaUtilites {

    public class PlayerInfo {
        /*            private int _x = -1;
                    private int _y = -1; */

        public const string Key = "TerrariaUtilites_Data";
        public bool StartWarp = false;
        public Vector2 StartWarpXY;

        /*            public int X {
                        get => _x;
                        set {
                            _x = value;
                        }
                    }
                    public int Y {
                        get => _y;
                        set {
                            _y = value;
                        }
                    } */


    }

    public static class TSPlayerExtensions {
        public static PlayerInfo GetPlayerInfo(this TSPlayer tsplayer) {
            if (!tsplayer.ContainsData(PlayerInfo.Key))
                tsplayer.SetData(PlayerInfo.Key, new PlayerInfo());

            return tsplayer.GetData<PlayerInfo>(PlayerInfo.Key);
        }
    }

}