using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPViral.Weapons;

public partial class Weapon : AnimatedEntity
{
	//Meta Info
	public virtual Model ViewModel => Model.Load( "weapons/rust_pistol/v_rust_pistol.vmdl" );
	public virtual Model WorldModel => Model.Load( "weapons/rust_pistol/rust_pistol.vmdl" );

	//Bullet 
	public virtual float BaseDamage => 5.0f;
	public virtual float BulletRange => 750.0f;
	public virtual int BulletCount => 1;
	public virtual float BulletForce => 2.5f;
	public virtual float BulletSize => 1.0f;
	public virtual float BulletSpread => 0.1f;
	public virtual float FireDelay => 0.175f;
	public virtual bool IsAutomatic => false;

	//Ammo + Reload
	public virtual bool IsMelee => false;
	public virtual bool AllowChamber => true;
	public virtual int DefaultAmmo => 9;
	public virtual int MaximumAmmo => 9;
	public virtual float ReloadTime => 2.0f;
	public virtual float EmptyReloadTime => 2.5f;
	public virtual float UnloadTime => 2.5f;
	public virtual float ShellReload => 1.0f;
	public virtual float ShellUnload => 1.0f;

	public enum AmmoEnum
	{
		None,
		Pistol,
		Shotgun,
		Rifle,
		Magnum,
		Misc
	}
	public virtual AmmoEnum AmmoType => AmmoEnum.None;

	//Setup
	public virtual float TimeToEquip => 1.0f;

	//Sounds
	public virtual string DrawSound { get; set; }
	public virtual string FireSound { get; set; }
	public virtual string DryFireSound { get; set; }
	public virtual string SwingSound { get; set; }
	public virtual string AttackSound { get; set; }

	//Offsets + Scale
	public virtual float VelocityScale => 0.1f;
	public virtual Vector3 GlobalPositionOffset => new Vector3( 0, 0, 0 );
	public virtual Angles GlobalAngleOffset => new Angles( 0, 0, 0 );
	public virtual float WeightReturnForce => 0.6f;
	public virtual float OverallWeight => 0.15f;

	//Walk/Run Style 
	public virtual Vector3 WalkCycleOffset => new Vector3( 125, 50, 0 );
	public virtual Vector2 BobAmount => new Vector2( 12, 0 );
	public virtual float AccelerationDamping => 0.25f;
	public virtual float WeightDamping => 0.35f;

	//Recoil
	public virtual Vector2 Recoil => new Vector2( 0, 150 );
	public virtual float DecayFactor => 2.75f;
	public virtual float TimeRecoilRecovery => 0.25f;
	public virtual float MaxRecoil => 5.0f;

	//Misc
	public virtual Vector3 AvoidancePositionOffset => new Vector3( 0, 0, -5 ); 
	public virtual Angles AvoidanceAngleOffset => new Angles( 10, 0, 0 );
	public virtual Vector3 CrouchPositionOffset => new Vector3( -5, 0, 0 );
	public virtual Angles CrouchAngleOffset => new Angles( 0, 0, 0 );

	public enum TypeEnum
	{
		Melee,
		Pistol,
		Shotgun,
		Rifle,
		Magnum
	}

	public virtual TypeEnum WeaponType => TypeEnum.Melee;

	//Inventory
	public virtual int SlotUsage => 1;
}
