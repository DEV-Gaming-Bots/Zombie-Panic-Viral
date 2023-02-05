namespace ZPViral.Entities.Hammer;

[Library( "zpv_trigger_teamassign" ), Category("Trigger")]
[HammerEntity]
public class TeamAssignTrigger : BaseTrigger
{
	public enum TeamAssignEnum
	{
		Spectator,
		Survivor,
		Zombie
	}

	[Property]
	public TeamAssignEnum TeamAssign { get; set; } = TeamAssignEnum.Spectator;

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if(other is PlayerPawn player )
		{
			switch ( TeamAssign )
			{
				case TeamAssignEnum.Spectator:
					ZPVGame.UpdatePawn( player.Client, PlayerPawn.TeamEnum.Spectator );
					break;

				case TeamAssignEnum.Survivor:
					ZPVGame.UpdatePawn( player.Client, PlayerPawn.TeamEnum.Survivor );
					break;

				case TeamAssignEnum.Zombie:
					ZPVGame.UpdatePawn( player.Client, PlayerPawn.TeamEnum.Zombie );
					break;
			}

			if( ZPVGame.Instance.CanStartRound() )
			{

			}
		}
	}
}

[Library("zpv_spawnpoint"), Title("Team Spawnpoint"), Category( "Player" ), Icon( "place" )]
[EditorModel( "models/editor/playerstart.vmdl", "white", "white", FixedBounds = true )]
[HammerEntity, RenderFields]
public class ZPVSpawn : Entity
{
	public enum TeamSpawnEnum
	{
		Spectator,
		Survivor,
		Zombie
	}

	[Property]
	public TeamSpawnEnum TeamSpawnpoint { get; set; } = TeamSpawnEnum.Spectator;
}

