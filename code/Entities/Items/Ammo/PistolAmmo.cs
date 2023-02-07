namespace ZPViral.Items;

[Library( "zpv_item_ammo_pistol" ), Title( "Pistol Ammo" ), Category( "Item" )]
[EditorModel( "models/items/pistol_ammobox.vmdl" )]
[HammerEntity]
public class PistolAmmo : ItemBase
{
	public override Model WorldModel => Model.Load( "models/items/pistol_ammobox.vmdl" );
	public override int ItemStock => 15;
	public override string UseSound => "ammo_pickup";

	public override void OnUseItem( SurvivorPawn player )
	{
		if ( !player.Inventory.CanAdd() )
			return;

		if( player.Inventory.CheckWeight(Inventory.AmmoEnum.Pistol, Stock) )
		{
			player.Inventory.AddAmmo( Inventory.AmmoEnum.Pistol, Stock );
			Stock -= 15;
		} 
		else
			return;

		base.OnUseItem( player );
	}
}
