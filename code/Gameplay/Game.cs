
global using System;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using ZPViral.Player;


namespace ZPViral;


public partial class ZPVGame : GameManager
{
	public ZPVGame()
	{
		if ( Game.IsServer )
		{

		}

		if ( Game.IsClient )
		{

		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new PlayerPawn(client);
		client.Pawn = pawn;
	}
}
