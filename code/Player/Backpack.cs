
public partial class Inventory : EntityComponent<PlayerPawn>, ISingletonComponent
{
	[Net] protected IList<Weapon> Weapons { get; set; }
	[Net, Predicted] public Weapon ActiveWeapon { get; set; }

	public int GetWeaponSlotUsage()
	{
		return 6;
	}

	public bool AddWeapon( Weapon weapon, bool makeActive = true )
	{
		if ( Weapons.Count >= GetWeaponSlotUsage() ) return false;

		Weapons.Add( weapon );

		weapon.EnableAllCollisions = false;
		weapon.Position = Entity.Position;
		weapon.SetParent( Entity );
		weapon.EnableDrawing = false;

		if ( makeActive )
			SetActiveWeapon( weapon );

		return true;
	}

	public bool RemoveWeapon( Weapon weapon, bool drop = false )
	{
		var success = Weapons.Remove( weapon );

		if ( success && drop )
		{
			weapon.OnDrop( Entity );
			weapon.Position = Entity.EyePosition;
			weapon.PhysicsGroup.Velocity = Entity.Velocity + (Entity.EyeRotation.Forward + Entity.EyeRotation.Up) * 150;

			if( Weapons.Count() > 0)
				SetActiveWeapon( Weapons.OrderBy( x => Guid.NewGuid() ).FirstOrDefault() );
		}

		return success;
	}

	public bool Clear()
	{
		foreach ( var weapon in Weapons.ToArray() )
		{
			if ( weapon == ActiveWeapon )
				ActiveWeapon.Cleanup();

			weapon?.Delete();
		}

		Weapons.Clear();
		return true;
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		if ( weapon == null )
		{
			ActiveWeapon = null;
			return;
		}
		var currentWeapon = ActiveWeapon;
		if ( currentWeapon.IsValid() )
		{
			// Can reject holster if we're doing an action already
			if ( !currentWeapon.CanHolster( Entity ) )
			{
				return;
			}

			currentWeapon.OnHolster( Entity );
			ActiveWeapon = null;
		}

		// Can reject deploy if we're doing an action already
		if ( !weapon.CanDeploy( Entity ) )
		{
			return;
		}

		ActiveWeapon = weapon;

		weapon?.OnDeploy( Entity );
	}

	protected override void OnDeactivate()
	{
		if ( Game.IsServer )
		{
			Weapons.ToList()
				.ForEach( x => x.Delete() );
		}
	}

	public Weapon GetSlot( int slot )
	{
		return Weapons.ElementAtOrDefault( slot ) ?? null;
	}

	protected int GetSlotIndexFromInput( InputButton slot )
	{
		return slot switch
		{
			InputButton.Slot1 => 0,
			InputButton.Slot2 => 1,
			InputButton.Slot3 => 2,
			InputButton.Slot4 => 3,
			InputButton.Slot5 => 4,
			_ => -1
		};
	}

	protected void TrySlotFromInput( InputButton slot )
	{
		if ( Input.Pressed( slot ) )
		{
			Input.SuppressButton( slot );

			if ( GetSlot( GetSlotIndexFromInput( slot ) ) is Weapon weapon )
			{
				Entity.ActiveWeaponInput = weapon;
			}
		}
	}

	public void BuildInput()
	{
		TrySlotFromInput( InputButton.Slot1 );
		TrySlotFromInput( InputButton.Slot2 );
		TrySlotFromInput( InputButton.Slot3 );
		TrySlotFromInput( InputButton.Slot4 );
		TrySlotFromInput( InputButton.Slot5 );

		ActiveWeapon?.BuildInput();
	}

	public void Simulate( IClient cl )
	{
		if ( Input.Pressed( InputButton.Drop ) )
			RemoveWeapon( ActiveWeapon, true );

		if ( Entity.ActiveWeaponInput != null && ActiveWeapon != Entity.ActiveWeaponInput )
		{
			var equipWep = Entity.ActiveWeaponInput as Weapon;

			SetActiveWeapon( equipWep );
			equipWep.OnDeploy(Entity);
			Entity.ActiveWeaponInput = null;
		}

		ActiveWeapon?.Simulate( cl );
	}

	public void FrameSimulate( IClient cl )
	{
		ActiveWeapon?.FrameSimulate( cl );
	}
}
