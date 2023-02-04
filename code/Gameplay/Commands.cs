namespace ZPViral;


public partial class ZPVGame
{
	public static bool Debugging { get; protected set; }

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
}
