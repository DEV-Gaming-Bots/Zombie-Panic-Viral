namespace ZPViral;


public partial class ZPVGame
{
	public static bool Debugging { get; protected set; }

	[ConCmd.Client("zpv.music", CanBeCalledFromServer = true)]
	public static void ToggleMusicCMD(bool toggle)
	{
		var player = ConsoleSystem.Caller.Pawn as PlayerPawn;
		if ( player == null ) return;

		player.ShouldPlayMusic = toggle;
	}

	[ConCmd.Admin("zpv.debug")]
	public static void ToggleDebug(bool toggle)
	{
		Debugging = toggle;
		Log.Info( $"Debug mode is {Debugging}" );
	}

	[ConCmd.Server("zpv.entity.create")]
	public static void CreateEntityCMD(string entName)
	{
		if ( !Debugging ) return;

		var player = ConsoleSystem.Caller.Pawn as PlayerPawn;
		if ( player == null ) return;

		var ent = Entity.CreateByName( entName );
		if ( ent == null ) return;

		ent.Position = player.GetEyeTraceResult( 64.0f ).EndPosition;
	}

	[ConCmd.Server( "zpv.infect" )]
	public static void InfectCMD()
	{
		if ( !Debugging ) return;

		var survivor = ConsoleSystem.Caller.Pawn as SurvivorPawn;
		if ( survivor == null ) return;

		survivor.Infect();
	}
	
	[ConCmd.Server( "zpv.setteam" )]
	public static void SetTeamCMD(int team, string targetName = "")
	{
		if ( !Debugging ) return;

		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		if ( !string.IsNullOrEmpty( targetName ) )
			caller = Game.Clients.Where( x => x.Name.ToLower().Contains( targetName ) ).FirstOrDefault();

		if ( caller == null ) return;

		switch ( team )
		{
			case 0: UpdatePawn( caller, PlayerPawn.TeamEnum.Unassigned ); break;
			case 1: UpdatePawn( caller, PlayerPawn.TeamEnum.Survivor ); break;
			case 2: UpdatePawn( caller, PlayerPawn.TeamEnum.Zombie ); break;
			case 3: UpdatePawn( caller, PlayerPawn.TeamEnum.Spectator ); break;
		}
	}
}
