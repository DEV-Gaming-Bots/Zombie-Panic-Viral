
namespace ZPViral.Items;

public class Kevlar : ItemBase
{
	public override Model WorldModel => Model.Load( "models/items/kevlar.vmdl" );
	public override int ItemStock => 50;
	public override string UseSound => "kevlar_use";

	public override void OnUseItem( SurvivorPawn player )
	{
		if ( player.Armor >= 100 )
			return;

		int check = (int)player.Armor;

		if ( (check + Stock) > 100 )
		{
			int remain = 100 - check;

			player.Armor += remain;
			Stock -= remain;
		}
		else if ( Stock > ItemStock )
		{
			player.Armor += 50;
			Stock -= 50;
		}
		else
		{
			player.Armor += Stock;
			Stock = 0;
		}

		player.Armor = player.Armor.Clamp( 0, 100 );

		base.OnUseItem( player );
	}
	
}
