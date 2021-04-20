using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;
using TShockAPI;
using OTAPI;
using Terraria.Localization;

namespace TerrariaUtilites
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Name => "NoChestmans"; // да
        public override string Author => "Neo"; // идите за белым кроликом и скажите канзасу прощай
        public override string Description => "Say \"No!\" to chestmans."; // скажем НЕТ! сисянчикам

        const string regionName = "Sklad"; // это должно быть в названии региона который будет скрывать сисянчиков

        public Plugin(Main main) : base(main) { }

        public override void Initialize() // тута добавляем всякие нужные хуки
        {
            RegionHooks.RegionEntered += OnRegionEnter; 
            RegionHooks.RegionLeft += OnRegionLeave;
            Hooks.Net.SendData += OnSendData; // уж удобнее напрямую хуки отапи использовать. Там удобно изменять параметры по ссылке 
        }

        protected override void Dispose(bool disposing) // тута очищаем место от хуков. Типа это важно!!! (!!!)
        {
            if (disposing) // так все делают
            {
                RegionHooks.RegionEntered -= OnRegionEnter;
                RegionHooks.RegionLeft -= OnRegionLeave;
                Hooks.Net.SendData -= OnSendData;
            }
            base.Dispose(disposing);
        }

        HookResult OnSendData(ref int bufferId, ref int msgType, ref int remoteClient, ref int ignoreClient, ref NetworkText text, ref int number, ref float number2, ref float number3, ref float number4, ref int number5, ref int number6, ref int number7)
        {
            if (msgType == (int)PacketTypes.PlayerActive) // нам нужон пакет на блокировку/разблокировку сисянчиков
            {
                var plr = TShock.Players[number]; // number - это ID сисянчика. Ну если этот элемент масива null то этот хитрый сисянчик еще не успел вызвать хук на джоин в регион сркывающий сисянчиков
                if (plr == null || (plr.CurrentRegion != null && plr.CurrentRegion.Name.Contains(regionName))) // если сисянчик инвалид или он в регионе специальном то он должен быть всегда скрыт
                    number2 = false.ToInt(); // типа 0 но так вам понятнее будет
            }
            return HookResult.Continue; // не блокируем этот пакет
        }

        void OnRegionEnter(RegionHooks.RegionEnteredEventArgs e)
        {
            if (e.Region.Name.Contains(regionName)) // сисянчик попал в сеть склада
                NetMessage.SendData(14, -1, e.Player.Index, null, e.Player.Index, false.ToInt()); // сисянчика закутали в сеть склада шобы невидно его было
        }

        void OnRegionLeave(RegionHooks.RegionLeftEventArgs e)
        {
            if (e.Region.Name.Contains(regionName) && (e.Player.CurrentRegion == null || !e.Player.CurrentRegion.Name.Contains(regionName))) // если сисянчик ушел из склада и не попал в другой склад 
            {
                NetMessage.SendData(14, -1, e.Player.Index, null, e.Player.Index, true.ToInt()); // то можно сделать его видимым

                NetMessage.TrySendData(4, -1, e.Player.Index, null, e.Player.Index); // но так как террария пересоздает объект сисянчика у вас то надо заново отправить его кожу и одежку. Это кожа
                NetMessage.TrySendData(16, -1, e.Player.Index, null, e.Player.Index); // это хп
                NetMessage.TrySendData(42, -1, e.Player.Index, null, e.Player.Index); // это мана
                NetMessage.TrySendData(50, -1, e.Player.Index, null, e.Player.Index); // это баффы

                var plr = e.Player.TPlayer;

                for (int i = 0; i < plr.inventory.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, i, plr.inventory[i].prefix, 0f, 0, 0, 0); // это инвентарь

                for (int i = 0; i < plr.armor.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 59 + i, plr.armor[i].prefix, 0f, 0, 0, 0); // это броня включая vanity слоты

                for (int i = 0; i < plr.dye.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 79 + i, plr.dye[i].prefix); // это слоты краски к броне

                for (int i = 0; i < plr.miscEquips.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 89 + i, plr.miscEquips[i].prefix); // это всякие пиомцы и крюки

                for (int i = 0; i < plr.miscDyes.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 94 + i, plr.miscDyes[i].prefix); // это краска к питомцам и крюкам

                for (int i = 0; i < plr.bank.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 99 + i, plr.bank.item[i].prefix); // это хрюшка копилка

                for (int i = 0; i < plr.bank2.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 139 + i, plr.bank2.item[i].prefix); // это сейф стальной

                NetMessage.TrySendData(5, -1, e.Player.Index, null, e.Player.Index, 179, plr.trashItem.prefix); // это трэш полный

                for (int i = 0; i < plr.bank3.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 180 + i, plr.bank3.item[i].prefix); // это летающая хрюшка копилка (level up хрюшки)

                for (int i = 0; i < plr.bank4.item.Length; i++)
                    NetMessage.SendData(5, -1, e.Player.Index, null, e.Player.Index, 220 + i, plr.bank4.item[i].prefix); // это бездонное ничто у которого всетаки есть дно в 40 слотов
            }
        }
    }
}
