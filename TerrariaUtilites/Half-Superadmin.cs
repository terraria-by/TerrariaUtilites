using System;
using System.IO;
using TShockAPI.DB;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HalfSuperadmin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "Neo";
        public override string Name => "Half-Superadmin"; // типа отсылка да

        List<string> ProtectedGroups = new List<string>() // это группы защищенные от изменений и их нельзя кому то присвоить
        {
            "superadmin"
        };
        string Path = TShock.SavePath + "\\Half-Superadmin\\ProtectedGroups.json", // эти переменые вынесены в поля для их удобного редактирования. тут путь к файлу с защищеными группами
               Permission = "halfsuperadmin"; // это типа пермисс к команде
        string[] CommandNames = // это названия команды если не нравятся поменяйте 
        {
            "hu",
            "halfuser"
        };

        public Plugin(Main game) : base(game)
        {
        }

        public override void Initialize() 
            => ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize); // шоб если ошибка возникнет остальной сервер норм загрузился

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
            base.Dispose(disposing);
        }

        void OnPostInitialize(EventArgs e)
        {
            FileLoad(); // загружаем список групп защищеных из файла
            Commands.ChatCommands.Add(new Command(Permission, Command, CommandNames)); // рабочая команда
        }

        void Command(CommandArgs e)
        {
            switch (e.Parameters.Count == 0 ? "help" : e.Parameters[0]) // если просто команду ввел игрок это будет считаться как если бы он ввел с аргументом help
            {
                case "group":
                    {
                        if (e.Parameters.Count < 3) // нада вводить все аргументы
                        {
                            e.Player.SendErrorMessage("Invalid syntax!");
                            return;
                        }
                        
                        var user = new UserAccount()
                        {
                            Name = e.Parameters[1] // первый рабочий аргумент - имя юзера
                        };
                        string groupName = e.Parameters[2]; // второй - название группы которую мы хотим ему дать

                        Group group = TShock.Groups.GetGroupByName(groupName); // ищем эту группу по названию
                        if (null == group) // если не нашли такой группы не существует
                        {
                            e.Player.SendErrorMessage($"That group ({groupName}) doesn't exist or has protected group.");
                            return;
                        }

                        if (ProtectedGroups.Any(x => x == group.Name)) // если группа есть в списке защищеных ее незя трогать
                        {
                            e.Player.SendErrorMessage("You can't change the account group to this one!");
                            return;
                        }

                        if (TShock.DB.Query($"UPDATE Users SET Usergroup = '{group.Name}' WHERE Username = '{user.Name}' AND Usergroup NOT IN ({string.Join(", ", ProtectedGroups.Select(x => $"'{x}'"))});") == 0) // тут мы ищем в бд сервера юзера шоб поменять ему группу на указаную нами. важный момент: если у юзера защищеная группа то сам юзер тоже защищен от измеений его группы
                        {
                            e.Player.SendErrorMessage($"User \"{user.Name}\" doesn't exist!"); // если поиск вернул 0 то мы не нашли юзера (0 - нулевая позиция в бд наверное)
                            return;
                        }

                        foreach (var plr in TShock.Players.Where(x => x?.Account?.Name == user.Name)) // если все ок то группа изменяется и если игрок на сервере меняем ему ее и на сервере, а не тока в бд
                            plr.Group = group;

                        TShock.Log.ConsoleInfo($"{ e.Player.Name} changed account {user.Name} to group {group.Name}.");
                        e.Player.SendSuccessMessage($"Account \"{user.Name}\" has been changed to group \"{group.Name}\"!");
                    }
                    return;
                case "protected": // эта часть команды нужна для управления списком защищеных групп
                    {
                        if (!e.Player.HasPermission($"{Permission}.admin")) // ею владеют тока важные шишки
                        {
                            e.Player.SendErrorMessage("You don't have access to this command.");
                            return;
                        }

                        switch (e.Parameters.Count < 2 ? "" : e.Parameters[1]) // аналогичный с началом метода команды смысол
                        {
                            case "list":
                                e.Player.SendInfoMessage($"Protected groups ({ProtectedGroups.Count}): {string.Join(", ", ProtectedGroups)}."); // это если хотите просто посмотреть список групп защищеных
                                return;
                            case "add": // добавляем в список
                            case "del": // или удаляем из него группу 
                                {
                                    if (e.Parameters.Count < 3) // нада все аргументы указывать
                                    {
                                        e.Player.SendErrorMessage("Invalid syntax!", e.Player);
                                        return;
                                    }

                                    string groupName = e.Parameters[2];

                                    Group group = TShock.Groups.GetGroupByName(groupName); // аналогично прошлой части команды
                                    if (null == group)
                                    {
                                        e.Player.SendErrorMessage($"That group ({groupName}) doesn't exist or has protected group.");
                                        return;
                                    }

                                    if (e.Parameters[1] == "del") //
                                    {
                                        if (group.Name == "superadmin") // суперадмин крутой его нельзя трогать даже суперам
                                        {
                                            e.Player.SendErrorMessage("You can't remove protection from this group!");
                                            return;
                                        }
                                        if (ProtectedGroups.All(x => x != groupName)) //если группы нет в списке то чо мы удаляем
                                        {
                                            e.Player.SendErrorMessage("This group wasn't protected!");
                                            return;
                                        }
                                        ProtectedGroups.Remove(group.Name); // удаление группы из списка если есть
                                    }
                                    else
                                    {
                                        if (ProtectedGroups.Any(x => x == groupName)) // тут все с точностью до наоборот
                                        {
                                            e.Player.SendErrorMessage("This group is arleady protected.");
                                            return;
                                        }
                                        ProtectedGroups.Add(group.Name);
                                    }
                                    FileUpdate(); // обновляем файл если были изменения
                                    e.Player.SendSuccessMessage("Protected groups list has been successfully updated! ");
                                }
                                return;
                            default:
                                e.Player.SendErrorMessage("Invalid syntax!");
                                return;
                        }
                    }
                case "help": // тут инструкция к команде
                default:
                    e.Player.SendInfoMessage("Syntax:"
                        + $"\n - {Commands.Specifier}{CommandNames[0]} group \"username\" \"new group\"");

                    if (e.Player.HasPermission($"{Permission}.admin"))
                        e.Player.SendInfoMessage($" - {Commands.Specifier}{CommandNames[0]} protected \"add|del|list\" \"group\"");
                    break;
            }
        }

        void FileLoad()
        {
            if (!File.Exists(Path)) // если файла нет значет кто та его таво этава удалил или не создал
            {
                File.Create(Path); // создаем его
                FileUpdate(); // и делаем его содержимое по дефолту
                return;
            }

            ProtectedGroups.Clear(); // ну типа чтобы не перемешивались мы сначала чистим список
            ProtectedGroups.AddRange(JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Path))); // и загружаем из файла его
            if (!ProtectedGroups.Any(x => x == "superadmin")) // суперадмин обязательно должен быть в списке защищеных групп
                ProtectedGroups.Add("superadmin");
        }

        void FileUpdate()
            => File.WriteAllText(Path, JsonConvert.SerializeObject(ProtectedGroups.ToArray())); // обновляем файл с защищеными группами
    }
}