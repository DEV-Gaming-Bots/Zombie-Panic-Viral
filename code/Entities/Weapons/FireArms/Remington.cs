namespace ZPViral.Weapons.FireArms;

[Library("zpv_weapon_870"), Title( "870 Remington" ), Category("Weapon")]
[EditorModel( "models/weapons/firearms/w_remington.vmdl" )]
[HammerEntity]
public partial class Remington : Weapon
{
	public override Model ViewModel => Model.Load( "models/weapons/firearms/v_remington.vmdl" );
	public override Model WorldModel => Model.Load( "models/weapons/firearms/w_remington.vmdl" );
	public override float BaseDamage => 11.25f;
	public override float BulletRange => 4999.0f;
	public override int BulletCount => 5;
	public override float BulletForce => 0.85f;
	public override float BulletSize => 0.75f;
	public override float BulletSpread => 0.45f;
	public override float FireDelay => 0.90f;
	public override bool IsAutomatic => false;

	//Ammo + Reload
	public override bool AllowChamber => false;
	public override int DefaultAmmo => 6;
	public override int MaximumAmmo => 6;
	public override float ReloadTime => 0.75f;
	public override float EmptyReloadTime => 1.25f;
	public override float UnloadTime => 0.75f;
	public override AmmoEnum AmmoType => AmmoEnum.Shotgun;
	public override float ShellReload => 1.25f;
	public override float ShellUnload => 1.10f;

	//Setup
	public override float TimeToEquip => 0.80f;

	//Sounds
	public override string DrawSound => "870_draw";
	public override string FireSound => "870_fire";
	public override string DryFireSound => "870_dryfire";

	//Offsets + Scale
	public override float VelocityScale => 0.5f;
	public override Vector3 GlobalPositionOffset => new Vector3( 0, 0, 0 );
	public override Angles GlobalAngleOffset => new Angles( 0, 0, 0 );

	//Walk/Run Style 
	public override Vector3 WalkCycleOffset => new Vector3( -5.5f, 0, 0 );
	public override Vector2 BobAmount => new Vector2( 5, 0 );
	public override float AccelerationDamping => 1.5f;
	public override float WeightDamping => 0.35f;
	public override float OverallWeight => 0.01f;
	//Recoil
	public override Vector2 Recoil => new Vector2( 55, 325 );
	public override float DecayFactor => 17.5f;
	public override float TimeRecoilRecovery => 0.25f;
	public override float MaxRecoil => 12.0f;

	//Misc
	public override Vector3 AvoidancePositionOffset => new Vector3( 0, 0, -1.25f );
	public override Angles AvoidanceAngleOffset => new Angles( 10, 0, 0 );
	public override Vector3 CrouchPositionOffset => new Vector3( -5, 0, 0 );
	public override Angles CrouchAngleOffset => new Angles( 0, 0, 0 );
	public override TypeEnum WeaponType => TypeEnum.Shotgun;    
	
	//Inventory
	public override int SlotUsage => 2;
}
