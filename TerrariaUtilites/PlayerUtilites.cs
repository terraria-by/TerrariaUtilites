using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;

namespace TerrariaUtilites {
    class PlayerUtilites {

        private void OnGetData(GetDataEventArgs args) {

            var _who = args.Msg.whoAmI;
            var _msgID = args.MsgID;
            var _player = TShock.Players[_who];
            var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));

            using (reader) {
                switch (_msgID) {
                    case PacketTypes.ConnectRequest: // 1
                        PlayerInfo info = _player.GetPlayerInfo();

                        break;

                }
            }

        }

    }
}
