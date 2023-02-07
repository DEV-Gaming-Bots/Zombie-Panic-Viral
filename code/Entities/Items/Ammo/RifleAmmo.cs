namespace ZPViral.Items;

[Library( "zpv_item_ammo_rifle" ), Title( "Rifle Ammo" ), Category( "Item" )]
[EditorModel( "models/items/rifle_ammobox.vmdl" )]
[HammerEntity]
public class RifleAmmo : ItemBase
{
	public override Model WorldModel => Model.Load( "models/items/rifle_ammobox.vmdl" );
	public override int ItemStock => 30;
	public override string UseSound => "ammo_pickup";

	public override void OnUseItem( SurvivorPawn player )
	{
		if ( !player.Inventory.CanAdd() )
			return;

		player.Inventory.AddAmmo( Inventory.AmmoEnum.Rifle, Stock );
		Stock -= 30;

		base.OnUseItem( player );
	}
}
