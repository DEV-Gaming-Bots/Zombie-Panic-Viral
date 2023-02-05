namespace ZPViral.Weapons.FireArms;

[Library( "zpv_weapon_machete" ), Title( "Machete" ), Category( "Weapon" )]
[EditorModel( "models/weapons/firearms/w_machete.vmdl" )]
[HammerEntity]
public partial class Machete : Weapon
{
	public override Model ViewModel => Model.Load( "models/weapons/melee/v_machete.vmdl" );
	public override Model WorldModel => Model.Load( "models/weapons/melee/w_machete.vmdl" );
	public override float BaseDamage => 40.0f;
	public override float BulletRange => 70.0f;
	public override int BulletCount => 1;
	public override float BulletForce => 0.35f;
	public override float BulletSize => 0.8f;
	public override float BulletSpread => 0.05f;
	public override float FireDelay => 0.5f;
	public override bool IsAutomatic => true;
	public override bool IsMelee => true;


	//Ammo + Reload
	public override bool AllowChamber => false;
	public override int DefaultAmmo => -1;
	public override int MaximumAmmo => -1;
	public override float ReloadTime => -1.0f;

	//Setup
	public override float TimeToEquip => 0.65f;

	//Sounds
	public override string SwingSound => "miss_swing";
	public override string AttackSound => "machete_hit";

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
}
