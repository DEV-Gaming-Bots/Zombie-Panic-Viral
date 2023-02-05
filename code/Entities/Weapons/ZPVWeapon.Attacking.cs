namespace ZPViral.Weapons;

public partial class Weapon
{
	public TimeSince TimeUntilCanFire { get; protected set; }

	protected bool CanFire( PlayerPawn player )
	{
		if ( TimeSinceActivated < TimeToEquip ) return false;
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
		if ( Tags.Has( "unloading" ) ) return false;

		return TimeUntilCanFire >= FireDelay;
	}

	protected void Fire( PlayerPawn player )
	{
		player?.SetAnimParameter( "b_attack", true );

		if ( Game.IsServer )
		{
			if ( IsMelee )
			{
				var hit = SwingMelee( BulletForce, BulletSize, BaseDamage, BulletRange );

				if ( hit )
					player.PlaySound( AttackSound );
				else
					player.PlaySound( SwingSound );

				DoMeleeEffects( To.Single( Player.Client ), hit );
			} 
			else
			{
				if ( AmmoCount > 0 )
				{
					player.PlaySound( FireSound );
					DoParticleEffects( To.Single( player.Client ) );
				}
				else
					player.PlaySound( DryFireSound );

				DoShootEffects( To.Single( player ), AmmoCount <= 0 );
			}

		}

		TimeUntilCanFire = 0;

		if(!IsMelee)
		{
			if ( AmmoCount <= 0 )
				return;

			ShootBullet( BulletSpread, BulletForce, BulletSize, BulletCount, BulletRange );
			TakeAmmo( 1 );
			AddRecoil();
		} 
		else
		{
			AddRecoil();
		}

	}

	[ClientRpc]
	public static void DoMeleeEffects( bool hit )
	{
		Game.AssertClient();

		int maxHit = WeaponViewModel.Current.GetAnimParameterInt( "max_hits" );
		int maxMiss = WeaponViewModel.Current.GetAnimParameterInt( "max_misses" );

		int randHit = Game.Random.Int( 1, maxHit );
		int randMiss = Game.Random.Int( 1, maxMiss );

		WeaponViewModel.Current?.SetAnimParameter( "hit", hit );
		WeaponViewModel.Current?.SetAnimParameter( "hit_ints", randHit );

		WeaponViewModel.Current?.SetAnimParameter( "miss", !hit );
		WeaponViewModel.Current?.SetAnimParameter( "miss_ints", randMiss );

		ArmVM.Current?.SetAnimParameter( "hit", hit );
		ArmVM.Current?.SetAnimParameter( "hit_ints", randHit );

		ArmVM.Current?.SetAnimParameter( "miss", !hit );
		ArmVM.Current?.SetAnimParameter( "miss_ints", randMiss );
	}

	[ClientRpc]
	public static void DoParticleEffects()
	{
		Game.AssertClient();

		Particles.Create("particles/pistol_muzzleflash.vpcf", WeaponViewModel.Current, "muzzle" );
	}

	[ClientRpc]
	public static void DoShootEffects( bool isEmpty )
	{
		Game.AssertClient();

		int maxFires = WeaponViewModel.Current.GetAnimParameterInt( "max_ints" );

		if( maxFires != 0 )
		{
			int randFire = Game.Random.Int( 1, maxFires );

			WeaponViewModel.Current?.SetAnimParameter( "fire_ints", randFire );
			ArmVM.Current?.SetAnimParameter( "fire_ints", randFire );
		}

		WeaponViewModel.Current?.SetAnimParameter( "fire", true );
		ArmVM.Current?.SetAnimParameter( "fire", true );

		WeaponViewModel.Current?.SetAnimParameter( "empty", isEmpty );
		ArmVM.Current?.SetAnimParameter( "empty", isEmpty );
	}


	protected TraceResult DoTraceAttack( Vector3 start, Vector3 end, float radius )
	{
		return Trace.Ray( start, end )
		.UseHitboxes()
		.WithAnyTags( "solid", "zombie", "glass" )
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

		var tr = DoTraceAttack( start, end, radius );
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

	public bool SwingMelee(float force, float hitSize, float damage, float range)
	{
		Game.SetRandomSeed( Time.Tick );

		var tr = DoTraceAttack( Player.AimRay.Position, Player.AimRay.Position + Player.AimRay.Forward * range, hitSize );
		
		if ( !tr.Hit ) return false;

		tr.Surface.DoBulletImpact( tr );

		if ( !Game.IsServer ) return false;
		if ( !tr.Entity.IsValid() ) return false;

		var damageInfo = DamageInfo.FromBullet( tr.EndPosition, Player.AimRay.Forward * 100 * force, damage )
				.UsingTraceResult( tr )
				.WithAttacker( Player )
				.WithWeapon( this );

		tr.Entity.TakeDamage( damageInfo );

		return true;
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
