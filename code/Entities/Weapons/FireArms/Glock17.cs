namespace ZPViral.Weapons.FireArms;

[Library( "zpv_weapon_glock" ), Title( "Glock 17" ), Category( "Weapon" )]
[EditorModel( "models/weapons/firearms/w_glock.vmdl" )]
[HammerEntity]
public partial class Glock17 : Weapon
{
	public override Model ViewModel => Model.Load( "models/weapons/firearms/v_glock.vmdl" );
	public override Model WorldModel => Model.Load( "models/weapons/firearms/w_glock.vmdl" );
	public override float BaseDamage => 14.0f;
	public override float BulletRange => 4999.0f;
	public override int BulletCount => 1;
	public override float BulletForce => 0.35f;
	public override float BulletSize => 0.8f;
	public override float BulletSpread => 0.05f;
	public override float FireDelay => 0.195f;
	public override bool IsAutomatic => false;

	//Ammo + Reload
	public override bool AllowChamber => true;
	public override int DefaultAmmo => 17;
	public override int MaximumAmmo => 17;
	public override float ReloadTime => 2.65f;
	public override float UnloadTime => 3.75f;
	public override AmmoEnum AmmoType => AmmoEnum.Pistol;

	//Setup
	public override float TimeToEquip => 0.75f;

	//Sounds	
	public override string DrawSound => "usp_draw";
	public override string FireSound { get; set; } = "usp_fire";
	public override string DryFireSound { get; set; } = "usp_dryfire";

	//Offsets + Scale
	public override float VelocityScale => 0.35f;
	public override Vector3 GlobalPositionOffset => new Vector3( 0, 0, 0 );
	public override Angles GlobalAngleOffset => new Angles( 0, 0, 0 );

	//Walk/Run Style 
	public override Vector3 WalkCycleOffset => new Vector3( -5.5f, 0, 0 );
	public override Vector2 BobAmount => new Vector2( 5, 0 );
	public override float AccelerationDamping => 1.5f;
	public override float WeightDamping => 0.35f;
	public override float OverallWeight => 0.01f;
	//Recoil
	public override Vector2 Recoil => new Vector2( 0, 135 );
	public override float DecayFactor => 17.5f;
	public override float TimeRecoilRecovery => 0.25f;
	public override float MaxRecoil => 12.0f;

	//Misc
	public override Vector3 AvoidancePositionOffset => new Vector3( 0, 0, -1.25f );
	public override Angles AvoidanceAngleOffset => new Angles( 10, 0, 0 );
	public override Vector3 CrouchPositionOffset => new Vector3( -5, 0, 0 );
	public override Angles CrouchAngleOffset => new Angles( 0, 0, 0 );
	public override TypeEnum WeaponType => TypeEnum.Pistol;
}
