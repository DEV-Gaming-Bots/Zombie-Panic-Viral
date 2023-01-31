namespace ZPViral;


public partial class DRGame
{
	public static bool Debugging { get; protected set; }

	[ConCmd.Admin("dr.debug")]
	public static void ToggleDebug(bool toggle)
	{
		Debugging = toggle;
		Log.Info( $"Debug mode is {Debugging}" );
	}
}
