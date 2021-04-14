using System;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using TerrariaApi.Server;

namespace TerrariaUtilites {
    [ApiVersion(2, 1)]
    public class GGPO : TerrariaPlugin {
        public override string Name => "GGPO";
        public override string Description => "Displays the last run of players of a certain group ";

        public GGPO(Main main) : base(main) { }

        public override void Initialize() {
            Commands.ChatCommands.Add(new Command("kd.gonline", Com, "gonline"));
        }

        public static void Com(CommandArgs args) {
            if (args.Parameters.Count < 1) {
                args.Player.SendErrorMessage("You must enter the name of a particular group. (Not default) ");
                return;
            }

            Group group = TShock.Groups.GetGroupByName(args.Parameters[0]);

            if (group.Name.ToLower() == TShock.Config.DefaultRegistrationGroupName.ToLower()) {
                args.Player.SendErrorMessage("You must enter the name of a particular group. [c/ff0000:(Not default)]");
                return;
            }

            string tt = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");

            using (var query = TShock.DB.QueryReader("SELECT * FROM Users WHERE Usergroup=@0", group.Name)) {
                while (query.Read()) {
                    try {
                        UserAccount account = TShock.UserAccounts.GetUserAccountByName(query.Get<string>("Username"));

                        //DateTime time = DateTime.Parse(account.LastAccessed).ToLocalTime();
                        DateTime time;
                        if (DateTime.TryParse(account.LastAccessed, out time)) {
                            time = DateTime.Parse(account.LastAccessed).ToLocalTime();
                            args.Player.SendInfoMessage("[c/082567:{0}]'s last login occured {1} {2} UTC{3}.", account.Name, time.ToShortDateString(), time.ToShortTimeString(), tt);
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

        }
    }
}
