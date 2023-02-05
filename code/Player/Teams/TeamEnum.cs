namespace ZPViral.Player;

public partial class PlayerPawn
{
	public enum TeamEnum
	{
		Unassigned,
		Spectator,
		Survivor,
		Infected,
		Zombie
	}

	public enum ZombieEnum
	{
		Standard,
		Carrier
	}

	public ZombieEnum ZombieType { get; set; }

	public void SwitchTeam( TeamEnum newTeam )
	{
		ZPVGame.UpdatePawn( Client, newTeam );
	}
}
