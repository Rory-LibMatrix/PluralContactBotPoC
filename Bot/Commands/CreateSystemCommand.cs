using LibMatrix;
using LibMatrix.EventTypes.Spec.State;
using LibMatrix.Helpers;
using LibMatrix.Services;
using LibMatrix.Utilities.Bot.Interfaces;
using PluralContactBotPoC.Bot.AccountData;
using PluralContactBotPoC.Bot.StateEventTypes;

namespace PluralContactBotPoC.Bot.Commands;

public class CreateSystemCommand(IServiceProvider services, HomeserverProviderService hsProvider, HomeserverResolverService hsResolver) : ICommand {
    public string Name { get; } = "createsystem";
    public string[]? Aliases { get; }
    public string Description { get; } = "Create a new system";
    public bool Unlisted { get; }

    public async Task<bool> CanInvoke(CommandContext ctx) {
        return true;
    }

    public async Task Invoke(CommandContext ctx) {
        if (ctx.Args.Length != 1) {
            await ctx.Reply(MessageFormatter.FormatError("Only one argument is allowed: system name!"));
            return;
        }

        var sysName = ctx.Args[0];
        try {
            try {
                await ctx.Homeserver.GetAccountDataAsync<BotData>("gay.rory.plural_contact_bot.system_data");
                await ctx.Reply(MessageFormatter.FormatError($"System {sysName} already exists!"));
            }
            catch (MatrixException e) {
                if (e is { ErrorCode: "M_NOT_FOUND" }) {
                    var sysData = new SystemData() {
                        ControlRoom = ctx.Room.RoomId,
                        Members = new(),
                    };

                    var state = ctx.Room.GetMembersEnumerableAsync();
                    await foreach (var member in state) {
                        sysData.Members.Add(member.StateKey);
                    }

                    await ctx.Room.SendStateEventAsync("m.room.name", new RoomNameEventContent() {
                        Name = sysName + " control room"
                    });

                    return;
                }

                throw;
            }
        }
        catch (Exception e) {
            await ctx.Reply(MessageFormatter.FormatException("Something went wrong!", e));
        }
    }
}