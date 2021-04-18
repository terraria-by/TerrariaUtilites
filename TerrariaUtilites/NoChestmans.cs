using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;
using TShockAPI;

namespace TerrariaUtilites {
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin {
        public override string Name => "NoChestmans";
        public override string Author => "Neo";
        public override string Description => "Say \"No!\" to chestmans.";

        public Plugin(Main main) : base(main) { }

        public override void Initialize() {
            RegionHooks.RegionEntered += OnRegionEnter;
            RegionHooks.RegionLeft += OnRegionLeave;
            ServerApi.Hooks.NetSendData.Register(this, OnSendPackets);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                RegionHooks.RegionEntered -= OnRegionEnter;
                RegionHooks.RegionLeft -= OnRegionLeave;
                ServerApi.Hooks.NetSendData.Deregister(this, OnSendPackets);
            }
            base.Dispose(disposing);
        }

        void OnSendPackets(SendDataEventArgs e) {
            if (e.MsgId == PacketTypes.PlayerActive) {
                var plr = TShock.Players[e.number];
                if (plr == null)
                    return;

                if (plr.CurrentRegion != null && plr.CurrentRegion.Name.Contains("Sklad") && e.number2 != false.ToInt()) {
                    NetMessage.SendData(14, e.remoteClient, e.ignoreClient, null, e.number, false.ToInt());
                    e.Handled = true;
                }
            }
        }

        void OnRegionEnter(RegionHooks.RegionEnteredEventArgs e) {
            if (e.Region.Name.Contains("Sklad"))
                NetMessage.SendData(14, -1, e.Player.Index, null, e.Player.Index, false.ToInt());
        }

        void OnRegionLeave(RegionHooks.RegionLeftEventArgs e) {
            if (e.Region.Name.Contains("Sklad") && (e.Player.CurrentRegion == null || !e.Player.CurrentRegion.Name.Contains("Sklad"))) {
                NetMessage.SendData(14, -1, e.Player.Index, null, e.Player.Index, true.ToInt());

                NetMessage.TrySendData(4, -1, e.Player.Index, null, e.Player.Index);
                NetMessage.TrySendData(16, -1, e.Player.Index, null, e.Player.Index);
                NetMessage.TrySendData(42, -1, e.Player.Index, null, e.Player.Index);
                NetMessage.TrySendData(50, -1, e.Player.Index, null, e.Player.Index);

                var plr = e.Player.TPlayer;

                for (int i = 0; i < plr.inventory.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, i, plr.inventory[i].prefix, 0f, 0, 0, 0);

                for (int i = 0; i < plr.armor.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 59 + i, plr.armor[i].prefix, 0f, 0, 0, 0);

                for (int i = 0; i < plr.dye.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 79 + i, plr.dye[i].prefix);

                for (int i = 0; i < plr.miscEquips.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 89 + i, plr.miscEquips[i].prefix);

                for (int i = 0; i < plr.miscDyes.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 94 + i, plr.miscDyes[i].prefix);

                for (int i = 0; i < plr.bank.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 99 + i, plr.bank.item[i].prefix);

                for (int i = 0; i < plr.bank2.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 139 + i, plr.bank2.item[i].prefix);

                NetMessage.TrySendData(5, -1, e.Player.Index, null, e.Player.Index, 179, plr.trashItem.prefix);

                for (int i = 0; i < plr.bank3.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 180 + i, plr.bank3.item[i].prefix);

                for (int i = 0; i < plr.bank4.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 220 + i, plr.bank4.item[i].prefix);
            }
        }
    }
}