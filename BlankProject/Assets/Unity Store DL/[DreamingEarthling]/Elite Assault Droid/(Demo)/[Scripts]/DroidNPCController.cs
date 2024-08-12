/*	
 *	
 *	DroidNPCController is very similar with DroidPlayerController,
 *	the main difference is DroidPlayerController needs to handle user's input and 
 *	DroidNPCController has AI functionality like searching enemies and level navigation.
 *	
 *	
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


using DreamingEarthling;



namespace DreamingEarthling
{
	public class DroidNPCController : MonoBehaviour 
	{
        // To which faction this NPC belongs (used to identify enemy with ally)
        public Faction  _Faction;


        // Internal components
        Animator        _Animator;
        CapsuleCollider _BodyCollider;
        NavMeshAgent    _NavMeshAgent;
        AudioSource     _Audio;


        [Header("States")]
        public bool     _IsReady;           // Unit is ready to fight. Set to true after unit's configuration
        public bool     _IsAlive = true;    // Other components use this variable to know whether this NPC is alive (valid target)
        public bool     _IsAiming;          // Set to TRUE when this NPC has a target and also it's in the combat range
        public bool     _IsPursuing;        // Is this NPC currently under the control of the NavMesh Agent

        [Header("Attributes")]
        public float    _MovingSpeed;       // How fast this NPC moves
        public int      _CombatRange;       // NPC enters combat sequence when in this range
        public float    _MaxShootingAngle;  // NPC will continue to shoot as long as the angle to its target is smaller than this value

        [Header("Health System")]
        public DroidNPCInGameUI _HealthBarUI;   // The in-game UI displaying the health of this NPC
        public float    _HealthMax = 100f;  // NPC full health points
        public float    _HealthCurrent;     // NPC realtime health points


        [Header("VFX")]
        public ParticleSystem _DeathSmoke;  // Activated after NPC's death

        [Header("SFX")]
        public AudioClip    _SFX_Footstep;  // Sound of footsteps

        [Header("Weapon")]
        public DroidWeapon  _Weapon;        // The weapon script this NPC holds
        public float        _ShootingSpeed; // The base shooting frequency of this weapon



        // These are the IK stuff needed for the NPC target looking and shooting functionality
        [Header("IK")]
        public Transform _Target;
        public float _IK_LookWeight;
        public float _IK_BodyWeight;
        public float _IK_HeadWeight;
        public float _IK_EyesWeight;
        public float _IK_ClampWeight;
        public Transform _AimReference;
        public Transform _IK_WeaponHolder;
        public Transform _IK_RightHandTarget;
        public Transform _IK_LeftHandTarget;

















        private void OnEnable()
        {
            _IsReady = false;
            _HealthCurrent = _HealthMax;
            _DeathSmoke.Pause();
        }
        private void Awake()
        {
            _Animator = GetComponent<Animator>();
            _BodyCollider = GetComponent<CapsuleCollider>();
            _NavMeshAgent = GetComponent<NavMeshAgent>();
            _Audio = GetComponent<AudioSource>();
        }
        // Use this for initialization
        void Start () 
		{
            Invoke("Configure", 1.4f);
		}
        void Configure()
        {
            // To randomize the unit's combat appearance (to fake NPC behaving slightly differently)
            _ShootingSpeed += Random.Range( -.5f, .5f );

            // Report to the level manager one NPC with the specified faction is spawned into the scene
            LevelManager._LevelManagerInScene.ReportToLevelMananger( gameObject, _Faction, UnitState.Created );

            // NPC is ready
            _IsReady = true;
        }
        private void Update()
        {
            if( !_IsReady || !_IsAlive )
                return;

            SearchTarget();
            ManeuverSequence();
            CombatSequence();
            Footsteps();
        }












        public void Health_Damaged( float damagePoints )
        {
            if( _HealthCurrent == 0 )
                return;

            if( damagePoints < _HealthCurrent )
            {
                _HealthCurrent -= damagePoints;
            }
            else
            {
                _HealthCurrent = 0;

                // NPC health reaches zero. 
                // Starting death sequence, disable everything shouldn't be running anymore.
                _IsAlive = false;
                _IsAiming = false;
                _IsPursuing = false;
                _NavMeshAgent.isStopped = true;
                _NavMeshAgent.enabled = false;
                ResetAnimator();
                _Animator.SetFloat( "DeathIndex", (int)Random.Range( 1, 3 ) );
                _HealthBarUI.gameObject.SetActive(false);
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                _BodyCollider.enabled = false;
                // Activate death smoke effect
                StartCoroutine(SynchronizeDeathSmokeToNPC(_Animator.GetBoneTransform( HumanBodyBones.UpperChest )));
                _DeathSmoke.Play();
                // Report to the level manager that this NPC is down
                LevelManager._LevelManagerInScene.ReportToLevelMananger( gameObject, _Faction, UnitState.Destroyed );
            } 

            _HealthBarUI.RefreshUI( _HealthCurrent, _HealthMax );
        }

        IEnumerator SynchronizeDeathSmokeToNPC( Transform bone )
        {
            var timePassed = 0f;
            while ( timePassed < 10f )
            {
                _DeathSmoke.gameObject.transform.position = bone.position;
                yield return new WaitForSeconds( .02f );
                timePassed += Time.deltaTime;
            }
        }























        /// <summary>
        /// Character IK Stuff
        /// </summary>
        /// <param name="layerIndex"></param>
        void OnAnimatorIK( int layerIndex )
        {
            
            if ( _IsAiming && _IsAlive )
            {
                // Look At Target
                _Animator.SetLookAtWeight( _IK_LookWeight, _IK_BodyWeight, _IK_HeadWeight, _IK_EyesWeight, _IK_ClampWeight );
                _Animator.SetLookAtPosition( _AimReference.position );

                _IK_WeaponHolder.position = _Animator.GetBoneTransform( HumanBodyBones.RightShoulder ).position;
                var directionTowardsTarget = _AimReference.position - transform.position;
                var angle = Vector3.Angle( transform.forward, directionTowardsTarget );
                if( angle > 60f )
                {
                    return;
                }

                _IK_WeaponHolder.LookAt( _AimReference );
                _Weapon.transform.LookAt( _AimReference );
                // The key for right hand IK is to let its parent's transform looking at the target,
                // and to rotate its self for the correct weapon holding rotation
                // (if you directly rotate right hand bone, the rotation will look weired)
                _IK_RightHandTarget.parent.transform.LookAt( _AimReference );

                // Left Hand
                _Animator.SetIKRotation( AvatarIKGoal.LeftHand, _Weapon._IK_LeftHand.rotation );
                _Animator.SetIKPosition( AvatarIKGoal.LeftHand, _Weapon._IK_LeftHand.position );
                _Animator.SetIKPositionWeight( AvatarIKGoal.LeftHand, 1f );
                _Animator.SetIKRotationWeight( AvatarIKGoal.LeftHand, 1f );

                // Right Hand
                _Animator.SetIKRotation( AvatarIKGoal.RightHand, _IK_RightHandTarget.rotation );
                _Animator.SetIKRotationWeight( AvatarIKGoal.RightHand, 1f );

                return;
            }
        }


















        void SearchTarget()
        {
            if( _Target == null )
            {
                // Ask the level manager to give this NPC a valid target (from the opposite faction) in the scene
                _Target = LevelManager._LevelManagerInScene.ReturnRandomHostilelUnit( _Faction );
            }

            // If this NPC already has a target, it requires target validation check
            if( _Target != null )
            {
                if ( _Target.gameObject.GetComponent<DroidNPCController>() != null && !_Target.gameObject.GetComponent<DroidNPCController>()._IsAlive )
                {
                    _Target = null;
                    return;
                }
                if ( _Target.gameObject.GetComponent<DroidPlayerController>() != null && !_Target.gameObject.GetComponent<DroidPlayerController>()._IsAlive )
                {
                    _Target = null;
                    return;
                }
            }
        }





        float distanceToTarget;
        float angleToTarget;
        void ManeuverSequence()
        {
            if ( _IsAlive )
            {
                if ( _Target != null )
                { 
                    // Angle to target
                    Vector3 targetDirection = _Target.position - transform.position;
                    angleToTarget = Vector3.Angle(targetDirection, transform.forward);
                    
                    // Distance to target
                    distanceToTarget = Vector3.Distance( transform.position, _Target.position );

                    // (1) Target in range
                    if ( distanceToTarget < _CombatRange )
                    {
                        // Target in unit's combat range
                        _IsPursuing = false; 
                        // Stop NavMeshAgent
                        _NavMeshAgent.isStopped = true;
                        // Reset animator, prepare for circling
                        ResetAnimator();

                        if ( !IsCirclingTargetInMotion )
                        {
                            IsCirclingTargetInMotion = true;
                            StartCoroutine( CirclingTarget() );
                        }
                    }

                    // (2) Target is out of range
                    var graceOffset = 1f;
                    // Start to pursuit
                    if ( distanceToTarget > _CombatRange + graceOffset )
                    {
                        _IsPursuing = true;
                        _NavMeshAgent.isStopped = false;
                        _NavMeshAgent.SetDestination( _Target.position );
                        _Animator.SetFloat( "InputForward", 1f );
                        _Animator.SetFloat( "InputSidewalk", 0 );
                        _Animator.SetBool( "Walking", true );
                    }

                    // (3) Target is almost in range
                    if ( distanceToTarget < _CombatRange + graceOffset && distanceToTarget > _CombatRange )
                    {
                        // Let the target facing the target
                        if ( angleToTarget > _MaxShootingAngle )
                        {
                            _IsPursuing = true;
                            _NavMeshAgent.isStopped = false;
                            _NavMeshAgent.SetDestination( _Target.position );
                            _Animator.SetFloat( "InputForward", 1f );
                            _Animator.SetFloat( "InputSidewalk", 0 );
                            _Animator.SetBool( "Walking", true );
                        }
                    }
                }
            }
            
            // Default state
            if( !_IsAlive || _Target == null )
            {
                // Unit is down or there isn't any target, disable everything
                ResetAnimator();
                _NavMeshAgent.isStopped = true;
                _IsPursuing = false;
            }

        }

        // If the target is in this NPC's combat range, this NPC will simply circle the target while shoot
        private bool IsCirclingTargetInMotion = false;
        IEnumerator CirclingTarget()
        {
            var speedApplied = _MovingSpeed;
            speedApplied /= 2f;
            var sideWalkingTimer = Random.Range( 2f, 5f );
            float t = 0;
            int direction = 1;
            if ( Random.value < .5f )
            {
                direction = -1;
            }
            while ( t < sideWalkingTimer )
            {
                // Break conditions
                if( !_IsAlive || _IsPursuing )
                    break;

                transform.Translate( transform.right * ( speedApplied * direction ) * Time.deltaTime, Space.World );
                _Animator.SetFloat( "InputSidewalk", 1f * direction );
                _Animator.SetBool( "Walking", true );

                if ( _Target != null )
                {
                    Vector3 targetPostition = new Vector3( _Target.position.x, transform.position.y, _Target.position.z ) ;
                    transform.LookAt( targetPostition );
                }

                yield return null;
                t += Time.deltaTime;
            }

            ResetAnimator();

            // Indicates the current circling maneuver is finished.
            IsCirclingTargetInMotion = false;
        }

        private void ResetAnimator()
        {
            _Animator.SetFloat( "InputForward", 0 );
            _Animator.SetFloat( "InputSidewalk", 0 );
            _Animator.SetBool( "Walking", false );
        }



        private float   shootingTimer;
        private Vector3 shootingSpotOffset;
        void CombatSequence()
        {

            if ( _IsAiming && _IsAlive && _Target != null && !_IsPursuing )
            {
                if( angleToTarget > _MaxShootingAngle )
                {
                    return;
                }

                // the default shooting spot -> enemy's chest
                var shootingSpot = ( _Target.gameObject.GetComponent<Animator>() ).GetBoneTransform( HumanBodyBones.UpperChest );
                
                // Let unit's aim reference moving to the shooting spot (with offset) to fake aiming process
                _AimReference.position = Vector3.MoveTowards( _AimReference.position, shootingSpotOffset, Time.deltaTime );

                shootingTimer += Time.deltaTime;
                if ( shootingTimer > _ShootingSpeed )
                {
                    // Precision modification to avoid 100% accuracy
                    float shootingSpotOffsetScale = .15f;
                    shootingSpotOffset = new Vector3(
                        shootingSpot.position.x + Random.Range( -shootingSpotOffsetScale, shootingSpotOffsetScale ),
                        shootingSpot.position.y + Random.Range( -shootingSpotOffsetScale, shootingSpotOffsetScale ),
                        shootingSpot.position.z + Random.Range( -shootingSpotOffsetScale, shootingSpotOffsetScale ) );

                    // Invoke the shooting animation clip
                    _Animator.SetTrigger( "ShootSingle" );
                    shootingTimer = 0;
                    _Weapon.Shoot();
                }
            }

        }

















        // This function is a simple implementation for generating footstep sound.
        // For more accurate implementation, you may want to use animation events.
        void Footsteps()
        {
            AnimatorStateInfo currentState      = _Animator.GetCurrentAnimatorStateInfo( 0 );
            AnimatorClipInfo[] currentClipInfo  = _Animator.GetCurrentAnimatorClipInfo( 0 );
            float playbackTime = currentState.normalizedTime % 1;
            // This is a quick way to get things going. You should consider to use animation events.
            if ( currentClipInfo[0].clip.name == "Armature|Walk" 
                || currentClipInfo[0].clip.name == "Armature|Strafe-Left"
                || currentClipInfo[0].clip.name == "Armature|Strafe-Left-45D" )
            {
                // Estimate the time (percentage) of foot being put down which corresponds the walking animation clips.
                // Of course, the time values are different if you use different walking animation clips. 
                // (1) First foot down
                if ( playbackTime > 0.51f && playbackTime < 0.53f )
                    PlayFootstepSound();

                // (2) Second foot down
                if( playbackTime > .97f && playbackTime < 0.99f )
                    PlayFootstepSound();
            }
        }
        void PlayFootstepSound()
        {
            _Audio.clip = _SFX_Footstep;
            // Randomize the sound a bit
            _Audio.pitch = Random.Range( .75f, 0.9f );
            _Audio.volume = Random.Range( .05f, .15f );
            _Audio.Play();
        }







	
	}
}
