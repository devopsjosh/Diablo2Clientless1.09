using ConsoleBot.Bots.Types.Andariel;

namespace ConsoleBot.Bots.Types.MephistoAndariel;

public class MephistoAndarielConfiguration : AccountConfig
{
	public AndarielDebugConfiguration Debug { get; set; } = new();
}
