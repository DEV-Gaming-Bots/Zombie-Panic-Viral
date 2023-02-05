﻿namespace ZPViral.Player;

public partial class ZombiePawn : PlayerPawn
{
	public override void SetSpawnPosition()
	{
		var spawnpoint = All.OfType<SpawnPoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( spawnpoint != null )
		{
			var tx = spawnpoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 25.0f;
			Transform = tx;
			ResetInterpolation();
		}
	}
	public override void CreateHull()
	{
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		base.CreateHull();
	}

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		Tags.Add( "zombie" );

		RenderColor = Color.Red;
		LifeState = LifeState.Alive;
		
		if ( ZombieType == ZombieEnum.Standard )
			Health = 200;
		else
			Health = 250;

		Components.Create<Controller>();
		Components.Create<Inventory>();
		Components.Create<PlayerAnimator>();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetSpawnPosition();
		CreateHull();
	}

	public override void TakeDamage( DamageInfo info )
	{
		info.Damage *= ScaleHitDamage( info.Hitbox );

		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}
}