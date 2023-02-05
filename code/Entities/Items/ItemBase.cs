namespace ZPViral.Items;

public class ItemBase : ModelEntity, IUse
{
	public virtual Model WorldModel => null;

	public virtual string UseSound => "";

	//How much stock is left over
	//E.G, Kevlar is 50, player takes 25 armor, leftover stock is 25
	public virtual int ItemStock => 1;

	public int Stock;

	public override void Spawn()
	{
		base.Spawn();
		Stock = ItemStock;

		Model = WorldModel;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	public virtual void OnUseItem( SurvivorPawn player )
	{
		player.PlaySound( UseSound );

		if ( Stock <= 0 )
			Delete();
	}

	public bool IsUsable( Entity user )
	{
		if ( user is SurvivorPawn ) return true;

		return false;
	}

	public bool OnUse( Entity user )
	{
		if(!IsUsable(user)) return false;

		OnUseItem( user as SurvivorPawn );

		return false;
	}
}

