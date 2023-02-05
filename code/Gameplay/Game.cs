
global using System;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Editor;
global using ZPViral.Player;
global using ZPViral.Weapons;
global using ZPViral.Entities.Hammer;
global using ZPViral.Weapons.FireArms;
global using ZPViral.Items;
global using ZPViral.UI;

namespace ZPViral;

public partial class ZPVGame : GameManager
{
	public static ZPVGame Instance => Current as ZPVGame;

	public ZPVGame()
	{
		if ( Game.IsServer )
		{
			RoundStatus = RoundEnum.Idle;

			if ( Game.IsEditor )
				Debugging = true;
		}

		if ( Game.IsClient )
		{
			_ = new ZPVHud();
		}
	}

	[Event.Hotload]
	public void HotloadGame()
	{
		if ( Game.IsServer )
		{

		}

		if ( Game.IsClient )
		{
			_ = new ZPVHud();
		}
	}

	Sound music;
	public void SimulateMusic( bool shouldPlay )
	{
		if ( !shouldPlay )
		{
			music.Stop();
			return;
		}

		if ( music.Finished )
			music = Sound.FromScreen( "music_randomtrack" );
	}

	[Event.Client.Frame]
	public void FrameGameplay()
	{
		var player = Game.LocalPawn as PlayerPawn;

		bool canPlay = false;

		if ( player != null )
			canPlay = player.ShouldPlayMusic;

		SimulateMusic( canPlay );
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new PlayerPawn();
		client.Pawn = pawn;
		pawn.Spawn();
	}

	public static void BroadcastSound(string path)
	{
		Game.AssertServer();

		foreach ( var player in All.OfType<PlayerPawn>() )
			player.PlaySoundClientside( To.Single(player.Client), path );

	}

	public static void UpdatePawnWithPosition( IClient client, PlayerPawn.TeamEnum newTeam, Vector3 position, Angles angles)
	{
		UpdatePawn( client, newTeam, false );

		client.Pawn.Position = position;

		(client.Pawn as PlayerPawn).Inventory?.Clear();
		(client.Pawn as PlayerPawn).SetViewAngles( To.Single( client ), angles );
	}

	public static void UpdatePawn( IClient client, PlayerPawn.TeamEnum newTeam, bool shouldFreeze = false)
	{
		var oldPawn = client.Pawn;

		PlayerPawn newPawn = null;
		switch (newTeam)
		{
			case PlayerPawn.TeamEnum.Unassigned:
				newPawn = new PlayerPawn();
				break;
			case PlayerPawn.TeamEnum.Survivor:
				newPawn = new SurvivorPawn();
				break;
			case PlayerPawn.TeamEnum.Zombie:
				newPawn = new ZombiePawn();
				break;
			//TODO: Spectator Pawn
		}

		if ( newPawn == null ) return;

		client.Pawn = newPawn;
		newPawn.Spawn();
		newPawn.FreezeMovement = shouldFreeze;

		oldPawn?.Delete();
	}
}
