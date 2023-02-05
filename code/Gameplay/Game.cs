
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

namespace ZPViral;

public partial class ZPVGame : GameManager
{
	public static ZPVGame Instance => Current as ZPVGame;

	public ZPVGame()
	{
		if ( Game.IsServer )
		{
			if ( Game.IsEditor )
				Debugging = true;
		}

		if ( Game.IsClient )
		{

		}
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

	public static void UpdatePawn( IClient client, PlayerPawn.TeamEnum newTeam)
	{
		var oldPawn = client.Pawn;

		switch (newTeam)
		{
			case PlayerPawn.TeamEnum.Unassigned:
				client.Pawn = new PlayerPawn();
				break;
			case PlayerPawn.TeamEnum.Survivor:
				client.Pawn = new SurvivorPawn();
				break;
			case PlayerPawn.TeamEnum.Zombie:
				client.Pawn = new ZombiePawn();
				break;
			//TODO: Spectator Pawn
		}

		oldPawn?.Delete();
	}
}
