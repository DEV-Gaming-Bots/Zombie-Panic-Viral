namespace ZPViral.Items;

[Library( "zpv_item_ammo_buckshot" ), Title( "Shotgun Ammo" ), Category( "Item" )]
[EditorModel( "models/items/shotgun_ammobox.vmdl" )]
[HammerEntity]
public class ShotgunAmmo : ItemBase
{
	public override Model WorldModel => Model.Load( "models/items/shotgun_ammobox.vmdl" );
	public override int ItemStock => 8;
	public override string UseSound => "ammo_pickup";

	public override void OnUseItem( SurvivorPawn player )
	{
		if ( player.Inventory.CanAdd() )
		{
			player.Inventory.AddAmmo( Inventory.AmmoEnum.Shotgun, Stock );

			if ( player.Inventory.CheckWeight( Inventory.AmmoEnum.Shotgun, Stock ) )
			{
				Stock -= 8;
			} 
			else
			{
				Log.Info( player.Inventory.CalculateAmmo( Inventory.AmmoEnum.Shotgun, Stock ) );
				Stock -= player.Inventory.CalculateAmmo(Inventory.AmmoEnum.Shotgun, Stock);
				Log.Info( Stock );
			}
		}
		else
			return;

		base.OnUseItem( player );
	}
}
