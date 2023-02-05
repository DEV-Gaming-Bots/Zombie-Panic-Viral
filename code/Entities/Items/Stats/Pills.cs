
namespace ZPViral.Items;

public class Pills : ItemBase
{
	public override Model WorldModel => Model.Load( "models/items/pills.vmdl" );
	public override int ItemStock => 25;
	public override string UseSound => "pills_use";

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
		else if ( Stock > ItemStock )
		{
			player.Health += 25;
			Stock -= 25;
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
