namespace ZPViral.Weapons;

public partial class Weapon : AnimatedEntity
{
	public TimeUntil TimeUntilCanFire { get; set; }

	protected bool CanFire( PlayerPawn player )
	{
		if ( TimeUntilCanFire > 0 ) return false;
		if ( TimeUntilReloaded > 0 ) return false;

		if( IsAutomatic )
		{
			if ( !Input.Down( InputButton.PrimaryAttack ) ) return false;
		} 
		else if ( !IsAutomatic )
		{
			if ( !Input.Pressed( InputButton.PrimaryAttack ) ) return false;
		}

		if ( Tags.Has( "reloading" ) ) return false;

		return TimeSinceActivated > FireDelay;
	}

	protected void Fire( PlayerPawn player )
	{
		player?.SetAnimParameter( "b_attack", true );

		if ( Game.IsServer )
		{
			if ( AmmoCount > 0 )
				player.PlaySound( FireSound );
			else
				player.PlaySound( DryFireSound );

			DoShootEffects( To.Single( player ), AmmoCount <= 0 );
		}

		TimeSinceActivated = 0;

		if ( AmmoCount <= 0 )
			return;

		TakeAmmo( 1 );
		AddRecoil();
		ShootBullet( BulletSpread, BulletForce, BulletSize, BulletCount, BulletRange );
	}

	[ClientRpc]
	public static void DoShootEffects(bool isEmpty)
	{
		Game.AssertClient();

		WeaponViewModel.Current?.SetAnimParameter( "empty", isEmpty );
		WeaponViewModel.Current?.SetAnimParameter( "fire", true );

		ArmVM.Current?.SetAnimParameter( "fire", true );
		ArmVM.Current?.SetAnimParameter( "empty", isEmpty );
	}


	protected TraceResult DoTraceBullet( Vector3 start, Vector3 end, float radius )
	{
		return Trace.Ray( start, end )
		.UseHitboxes()
		.WithAnyTags( "solid", "player", "glass" )
		.Ignore( Owner )
		.Size( radius )
		.Run();
	}

	protected Vector3 CalculateRicochetDirection( TraceResult tr, ref float hits )
	{
		if ( tr.Entity is GlassShard )
		{
			// Allow us to do another hit
			hits--;
			return tr.Direction;
		}

		return Vector3.Reflect( tr.Direction, tr.Normal ).Normal;
	}

	public IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius, ref float damage )
	{
		float curHits = 0;
		var hits = new List<TraceResult>();

		var tr = DoTraceBullet( start, end, radius );
		if ( tr.Hit )
		{
			if ( curHits > 1 )
				damage *= 0.5f;
			hits.Add( tr );
		}

		var dist = tr.Distance.Remap( 0, BulletRange, 1, 0.5f ).Clamp( 0.5f, 1f );
		damage *= dist;

		return hits;
	}

	public void ShootBullet( float spread, float force, float bulletSize, int bulletCount = 1, float bulletRange = 5000f )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Game.SetRandomSeed( Time.Tick );

		for ( int i = 0; i < bulletCount; i++ )
		{
			var rot = Rotation.LookAt( Player.AimRay.Forward );
			rot *= Rotation.From( new Angles( -CurrentRecoil.y, CurrentRecoil.x, 0 ) );

			var forward = rot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			var damage = BaseDamage;

			foreach ( var tr in TraceBullet( Player.AimRay.Position, Player.AimRay.Position + forward * bulletRange, bulletSize, ref damage ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !Game.IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Player )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}
}
