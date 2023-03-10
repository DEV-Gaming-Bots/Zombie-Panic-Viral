namespace ZPViral.Weapons.FireArms;

[Library("zpv_weapon_ak47"), Title("AK-47"), Category("Weapon")]
[EditorModel( "models/weapons/firearms/w_ak47.vmdl" )]
[HammerEntity]
public partial class AK47 : Weapon
{
	public override Model ViewModel => Model.Load( "models/weapons/firearms/v_ak47.vmdl" );
	public override Model WorldModel => Model.Load( "models/weapons/firearms/w_ak47.vmdl" );
	public override float BaseDamage => 21.0f;
	public override float BulletRange => 4999.0f;
	public override int BulletCount => 1;
	public override float BulletForce => 0.45f;
	public override float BulletSize => 0.85f;
	public override float BulletSpread => 0.10f;
	public override float FireDelay => 0.09f;
	public override bool IsAutomatic => true;

	//Ammo + Reload
	public override bool AllowChamber => true;
	public override int DefaultAmmo => 30;
	public override int MaximumAmmo => 30;
	public override float ReloadTime => 3.25f;
	public override float EmptyReloadTime => 4.25f;
	public override float UnloadTime => 5.75f;
	public override AmmoEnum AmmoType => AmmoEnum.Rifle;

	//Setup
	public override float TimeToEquip => 1.15f;

	//Sounds
	public override string DrawSound => "ak47_draw";
	public override string FireSound => "ak47_fire";
	public override string DryFireSound => "ak47_dryfire";

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
	public override TypeEnum WeaponType => TypeEnum.Rifle;

	//Inventory
	public override int SlotUsage => 2;
}
