using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPViral;

public partial class ZPVGame
{
	[Net] public int ZombieLives { get; set; }

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
		if ( Debugging ) return true;

		if ( RoundStatus != RoundEnum.Idle ) return false;

		int survivors = GetTeamCount( PlayerPawn.TeamEnum.Survivor );
		int zombies = GetTeamCount( PlayerPawn.TeamEnum.Zombie );

		if ( survivors >= 2 || (zombies >= 1 && survivors >= 1) )
			return true;

		return false;
	}

	public async Task AwaitRoundDelay(float time)
	{
		await Task.DelayRealtimeSeconds( time );
	}

	public async void PreStartRound()
	{
		RoundStatus = RoundEnum.Starting;

		BroadcastSound( "round_starting" );

		await AwaitRoundDelay( 7.5f );
		StartRound();
	}

	public void AddZombieLive()
	{
		ZombieLives++;
	}

	public void TakeZombieLive()
	{
		if ( ZombieLives <= 0 ) return;
		ZombieLives--;
	}

	public void StartRound()
	{
		RoundStatus = RoundEnum.Active;

		foreach ( var player in GetTeamMembers() )
			player.FreezeMovement = false;

		int assignZombies = (int)MathF.Ceiling( 0.2f * Game.Clients.Count() );
		//Get and subtract from voluntary zombie members
		assignZombies -= GetTeamCount( PlayerPawn.TeamEnum.Zombie );

		//We're debugging, set int to 0 so we don't break things
		if ( Debugging )
			assignZombies = 0;

		if ( assignZombies > 0 )
		{
			for ( int i = 0; i < assignZombies; i++ )
			{
				var chosen = GetTeamMembers( PlayerPawn.TeamEnum.Survivor )
					.OrderBy(x => Guid.NewGuid()).FirstOrDefault().Client;

				UpdatePawn( chosen, PlayerPawn.TeamEnum.Zombie );
			}
		}

		ZombieLives = 3 * (Game.Clients.Count() - 1);

		if ( GetTeamCount(PlayerPawn.TeamEnum.Zombie) > 0)
		{
			GetTeamMembers( PlayerPawn.TeamEnum.Zombie )
				.OrderBy( x => Guid.NewGuid() ).FirstOrDefault().ZombieType = PlayerPawn.ZombieEnum.Carrier;
		}
		foreach ( var player in GetTeamMembers())
		{
			GiveWeapons( player );
		}
	}

	public void CheckRoundStatus()
	{
		if ( RoundStatus != RoundEnum.Active ) return;

		int survivors = GetTeamMembers( PlayerPawn.TeamEnum.Survivor )
			.Where( x => x.LifeState == LifeState.Alive ).Count();

		int infected = GetTeamMembers( PlayerPawn.TeamEnum.Infected )
			.Where( x => x.LifeState == LifeState.Alive ).Count();

		int zombies = GetTeamMembers( PlayerPawn.TeamEnum.Zombie )
			.Where(x => x.LifeState == LifeState.Alive).Count();

		if ( survivors <= 0 )
			PostRound( WinEnum.Zombie );

		if ( zombies <= 0 && ZombieLives <= 0 && infected <= 0 )
			PostRound( WinEnum.Survivor );
	}

	public enum WinEnum
	{
		Draw,
		Survivor,
		Zombie
	}

	public async void PostRound( WinEnum winners )
	{
		RoundStatus = RoundEnum.Post;
		foreach ( var player in GetTeamMembers() )
			player.FreezeMovement = true;

		BroadcastSound( winners == WinEnum.Survivor ? "roundend_survivor" : "roundend_zombie" );

		await AwaitRoundDelay( 10.0f );

		ResetGame();
	}

	public void ResetGame()
	{
		Game.ResetMap( All.OfType<PlayerPawn>().ToArray() );

		foreach ( var player in All.OfType<PlayerPawn>().ToArray() )
			UpdatePawn( player.Client, PlayerPawn.TeamEnum.Unassigned );
	}

	public List<PlayerPawn> GetTeamMembers( PlayerPawn.TeamEnum team = PlayerPawn.TeamEnum.Unassigned )
	{
		List<PlayerPawn> list = null;
		
		switch ( team )
		{
			case PlayerPawn.TeamEnum.Survivor: 
				list = All.OfType<PlayerPawn>().Where( x => x is SurvivorPawn ).ToList(); 
				break;
			case PlayerPawn.TeamEnum.Zombie: 
				list = All.OfType<PlayerPawn>().Where( x => x is ZombiePawn ).ToList(); 
				break;
			case PlayerPawn.TeamEnum.Infected: list = All.OfType<PlayerPawn>()
				.Where( x => x is SurvivorPawn survivor && survivor.IsInfected ).ToList();
				break;

			default: list = All.OfType<PlayerPawn>().ToList(); 
				break;
		}

		return list;
	}

	Weapon GetRandomMelee()
	{
		Weapon melee = null;

		switch ( Game.Random.Int( 1, 1 ) )
		{
			case 1: melee = new Machete(); break;
		}

		return melee;
	}
	Weapon GetRandomPistol()
	{
		Weapon pistol = null;

		switch(Game.Random.Int(1, 3))
		{
			case 1: pistol = new USP(); break;
			case 2: pistol = new Glock17(); break;
			case 3: pistol = new Glock18c(); break;
		}

		return pistol;
	}


	public void GiveWeapons(PlayerPawn player)
	{
		if(player is SurvivorPawn)
		{
			var types = TypeLibrary.GetTypes<Weapon>().ToList();
			types.Remove( TypeLibrary.GetType<ZombieArms>() );
			types.Remove( TypeLibrary.GetType<Weapon>() );

			player.Inventory.AddWeapon( GetRandomMelee(), false );
			player.Inventory.AddWeapon( GetRandomPistol() );
		} 
		if(player is ZombiePawn)
		{
			player.Inventory.AddWeapon( new ZombieArms(), true );
		}
	}

	public int GetTeamCount(PlayerPawn.TeamEnum team)
	{
		switch(team)
		{
			case PlayerPawn.TeamEnum.Survivor: return Game.Clients.Where( x => x.Pawn is SurvivorPawn ).Count();
			case PlayerPawn.TeamEnum.Infected: return All.OfType<SurvivorPawn>().Where( x => x.IsInfected ).Count();
			case PlayerPawn.TeamEnum.Zombie: return Game.Clients.Where( x => x.Pawn is ZombiePawn ).Count();
		}

		return 0;
	}
}
