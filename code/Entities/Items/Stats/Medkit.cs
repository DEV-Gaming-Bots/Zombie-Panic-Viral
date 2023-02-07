
namespace ZPViral.Items;

[Library("zpv_item_medkit"), Title( "Medkit" ), Category( "Item" )]
[EditorModel( "models/items/medkit.vmdl" )]
[HammerEntity]
public class Medkit : ItemBase
{
	public override Model WorldModel => Model.Load( "models/items/medkit.vmdl" );
	public override int ItemStock => 50;
	public override string UseSound => "medkit_use";

	public override void OnUseItem( SurvivorPawn player )
	{
		if ( player.Health >= 100 )
			return;

		int check = (int)player.Health;

		if( (check + Stock) > 100 )
		{
			int remain = 100 - check;

			player.Health += remain;
			Stock -= remain;
		} 
		else if ( Stock >= ItemStock )
		{
			player.Health += 50;
			Stock -= 50;
		} 
		else
		{
			player.Health += Stock;
			Stock = 0;
		}

		player.Health = player.Health.Clamp( 0, 100 );

		base.OnUseItem( player );
	}
	
}
