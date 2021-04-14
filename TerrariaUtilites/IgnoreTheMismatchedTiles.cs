using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.IO;

namespace TerrariaUtilites {
    [ApiVersion(2, 1)]
    public class IgnoreTheMismatchedTiles : TerrariaPlugin {
        public override string Author {
            get {
                return "Zoom L1 | Colag";
            }
        }
        public override string Name {
            get {
                return "Ignore the mismatched tiles";
            }
        }

        public IgnoreTheMismatchedTiles(Main game) : base(game) { }

        public override void Initialize() {
            GetDataHandlers.TileEdit += OnTileEdit;
            GetDataHandlers.PlaceObject += OnPlaceObject;
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
        }

        public static void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs args) {
            if (args.Action == GetDataHandlers.EditAction.PlaceTile) {
                if (Main.tile[args.X, args.Y] != null && Main.tile[args.X, args.Y].type == 127) {
                    args.Handled = true;
                    return;
                }
                if (args.Player.SelectedItem.placeStyle != args.Style) {
                    args.Handled = true;
                    return;
                }
            }
        }

        public static void OnPlaceObject(object sender, GetDataHandlers.PlaceObjectEventArgs args) {
            if (Main.tile[args.X, args.Y] != null && Main.tile[args.X, args.Y].type == 127) {
                args.Handled = true;
                return;
            }
            if (args.Player.SelectedItem.placeStyle != args.Style) {
                args.Handled = true;
                return;
            }
        }

        public static void OnGetData(GetDataEventArgs e) {
            if (e.MsgID == (PacketTypes)34) {
                TSPlayer player = TShock.Players[e.Msg.whoAmI];
                using (var data = new BinaryReader(new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))) {
                    data.ReadByte();
                    int x = (int)data.ReadInt16();
                    int y = (int)data.ReadInt16();
                    short style = data.ReadInt16();
                    if (Main.tile[x, y] != null && Main.tile[x, y].type == 127) {
                        e.Handled = true;
                        return;
                    }
                    if (player.SelectedItem.placeStyle != style) {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }
    }

}
