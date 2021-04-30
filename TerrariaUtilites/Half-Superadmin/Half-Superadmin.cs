using System;
using TShockAPI.Hooks;
using TShockAPI.DB;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Collections.Generic;
using System.Linq;

namespace HalfSuperadmin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "Neo";
        public override string Name => "Half-Superadmin"; 

        string Permission = "halfsuperadmin"; // это типа пермисс к команде если не нравятся поменяйте 
        string[] CommandNames = // это названия команды тоже можно поменять
        {
            "hu",
            "halfuser"
        };

        public Plugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            GeneralHooks.ReloadEvent += OnReload;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }

        void OnPostInitialize(EventArgs e)
        {
            Commands.ChatCommands.Add(new Command(Permission, Command, CommandNames));
            Config.Load();
        }

        void OnReload(ReloadEventArgs e)
        {
            Config.Load();
            e.Player.SendSuccessMessage("[Half-Superadmin] Successfully reloaded config.");
        }

        void Command(CommandArgs e)
        {
            switch (e.Parameters.Count == 0 ? "help" : e.Parameters[0]) 
            {
                case "group":
                    {
                        if (e.Parameters.Count < 3) 
                        {
                            e.Player.SendErrorMessage("Забыли какой-то аргумент!");
                            return;
                        }
                        
                        string userName = e.Parameters[1], 
                               groupName = e.Parameters[2]; 

                        Group group = TShock.Groups.GetGroupByName(groupName); // ищем эту группу по названию
                        if (null == group)
                        {
                            e.Player.SendErrorMessage($"Нет такой группы ({groupName}).");
                            return;
                        }

                        if (!Config.List.ContainsKey(e.Player.Group.Name) || !Config.List[e.Player.Group.Name].Any(x => x == group.Name)) 
                        {
                            e.Player.SendErrorMessage("Ты слишком слаб, чтобы управлять этой группой!");
                            return;
                        }

                        if (TShock.DB.Query($"UPDATE Users SET Usergroup = '{group.Name}' WHERE Username = '{userName}' AND Usergroup IN ({string.Join(", ", Config.List[e.Player.Group.Name].Select(x => $"'{x}'"))});") == 0) // тут мы ищем в бд сервера юзера шоб поменять ему группу на указаную нами. важный момент: если у юзера защищеная группа то сам юзер тоже защищен от измеений его группы
                        {
                            e.Player.SendErrorMessage($"Юзера \"{userName}\" нет в этой Вселенной. Может, в другой?"); // если поиск вернул 0 то мы не нашли юзера (0 - нулевая позиция в бд наверное)
                            return;
                        }

                        foreach (var plr in TShock.Players.Where(x => x?.Account?.Name == userName)) // если все ок то группа изменяется и если игрок на сервере меняем ему ее и на сервере, а не тока в бд
                            plr.Group = group;

                        TShock.Log.ConsoleInfo($"{ e.Player.Name} changed account {userName} to group {group.Name}.");
                        e.Player.SendSuccessMessage($"Юзер \"{userName}\" был повышен до \"{group.Name}\"!");
                    }
                    return;
                case "add":
                case "del":
                    {
                        if (!e.Player.HasPermission($"{Permission}.admin")) 
                        {
                            e.Player.SendErrorMessage("Ваш уровень доступа слишком низкий для этого. Попросите повышение у начальника.");
                            return;
                        }

                        if (e.Parameters.Count < 2) 
                        {
                            e.Player.SendErrorMessage("Забыли какой-то аргумент!");
                            return;
                        }

                        string groupName = e.Parameters[1]; 

                        Group group = TShock.Groups.GetGroupByName(groupName); // ищем эту группу по названию
                        if (null == group)
                        {
                            e.Player.SendErrorMessage($"Нет такой группы ({groupName}).");
                            return;
                        }

                        if (e.Parameters[0] == "add")
                        {
                            if (!Config.List.ContainsKey(group.Name))
                                Config.List.Add(group.Name, new List<string>());
                        }
                        else 
                            Config.List.Remove(group.Name);

                        Config.Save();
                        e.Player.SendSuccessMessage($"Готово! Загляни теперь в {Commands.Specifier}{CommandNames[0]} list.");
                    }
                    return;
                case "allow":
                case "remove":
                    {
                        if (!e.Player.HasPermission($"{Permission}.admin"))
                        {
                            e.Player.SendErrorMessage("Ваш уровень доступа слишком низкий для этого. Попросите повышение у начальника.");
                            return;
                        }

                        if (e.Parameters.Count < 3)
                        {
                            e.Player.SendErrorMessage("Забыли какой-то аргумент!");
                            return;
                        }

                        string groupCoordinator = e.Parameters[1],
                               groupName = e.Parameters[2];

                        if (!Config.List.ContainsKey(groupCoordinator))
                        {
                            e.Player.SendErrorMessage($"Группы {groupCoordinator} нет в списке вожатых. Рановато ей пока, начальник.");
                            return;
                        }

                        Group group = TShock.Groups.GetGroupByName(groupName); // ищем эту группу по названию
                        if (null == group)
                        {
                            e.Player.SendErrorMessage($"Нет такой группы ({groupName}).");
                            return;
                        }

                        if (e.Parameters[0] == "allow")
                        {
                            if (!Config.List[groupCoordinator].Contains(group.Name))
                                Config.List[groupCoordinator].Add(group.Name);
                        }
                        else
                            Config.List[groupCoordinator].Remove(group.Name);

                        Config.Save();
                        e.Player.SendSuccessMessage($"Готово! Загляни теперь в {Commands.Specifier}{CommandNames[0]} list.");
                    }
                    return;
                case "list":
                    {
                        e.Player.SendInfoMessage(" - - - Группа вожатого: группы, которыми он может управлять - - -"
                            + "\n" + string.Join("\n", Config.List.Select(x => $"{x.Key}: {string.Join(", ", x.Value)}")));
                    }
                    return;
                case "help":
                default:
                    e.Player.SendInfoMessage("Syntax:"
                        + $"\n - {Commands.Specifier}{CommandNames[0]} group \"имя юзера\" \"новая группа\"");

                    if (e.Player.HasPermission($"{Permission}.admin"))
                    {
                        e.Player.SendInfoMessage($" - {Commands.Specifier}{CommandNames[0]} \"add|del\" \"группа координатора\"");
                        e.Player.SendInfoMessage($" - {Commands.Specifier}{CommandNames[0]} \"allow|remove\" \"группа координатора\" \"управляемая им группа\"");
                        e.Player.SendInfoMessage($" - {Commands.Specifier}{CommandNames[0]} \"list\"");
                    }
                    return;
            }
        }
    }
}