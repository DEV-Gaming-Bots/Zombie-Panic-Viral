using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPViral;

public partial class ZPVGame
{
	public enum RoundEnum 
	{ 
		Idle,
		Starting,
		Active,
		Post,
		MapChange
	}

	public RoundEnum RoundStatus;
	public static RoundEnum StaticStatus => Instance?.RoundStatus ?? RoundEnum.Idle;

	public bool CanStartRound()
	{
		if ( RoundStatus != RoundEnum.Idle ) return false;

		int survivors = GetTeamCount( PlayerPawn.TeamEnum.Survivor );
		int zombies = GetTeamCount(PlayerPawn.TeamEnum.Zombie);

		if ( survivors >= 2 || (zombies >= 1 && survivors >= 1) )
			return true;

		return false;
	}

	public async Task AwaitRoundDelay(float time)
	{
		await Task.DelayRealtimeSeconds( time );
	}

	public void PreStartRound()
	{
		RoundStatus = RoundEnum.Starting;

		_ = AwaitRoundDelay( 7.5f );
		StartRound();
	}

	public void StartRound()
	{
		RoundStatus = RoundEnum.Active;
	}

	public void CheckRoundStatus()
	{
		if ( RoundStatus != RoundEnum.Active ) return;

		int survivors = GetTeamCount( PlayerPawn.TeamEnum.Survivor );
		int zombies = GetTeamCount( PlayerPawn.TeamEnum.Zombie );

		if ( survivors <= 0 )
			PostRound( WinEnum.Zombie );
	}

	public enum WinEnum
	{
		Draw,
		Survivor,
		Zombie
	}

	public void PostRound( WinEnum winners )
	{
		RoundStatus = RoundEnum.Post;

		BroadcastSound( "roundend_zombie" );

		_ = AwaitRoundDelay( 10.0f );

		ResetGame();
	}

	public void ResetGame()
	{
		Game.ResetMap( All.OfType<PlayerPawn>().ToArray() );

		foreach ( var player in All.OfType<PlayerPawn>() )
			UpdatePawn( player.Client, PlayerPawn.TeamEnum.Unassigned );
	}

	public int GetTeamCount(PlayerPawn.TeamEnum team)
	{
		switch(team)
		{
			case PlayerPawn.TeamEnum.Survivor: All.OfType<PlayerPawn>().Where( x => x is SurvivorPawn).Count(); break;
			case PlayerPawn.TeamEnum.Zombie: All.OfType<PlayerPawn>().Where( x => x is ZombiePawn ).Count(); break;
		}

		return 0;
	}
}
