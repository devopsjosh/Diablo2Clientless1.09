namespace ConsoleBot.Bots.Types.Andariel;

public class AndarielConfiguration : AccountConfig
{
	public AndarielDebugConfiguration Debug { get; set; } = new();
}

public class AndarielDebugConfiguration
{
	public bool Enabled { get; set; }

	public int LogEveryTicks { get; set; } = 5;
}