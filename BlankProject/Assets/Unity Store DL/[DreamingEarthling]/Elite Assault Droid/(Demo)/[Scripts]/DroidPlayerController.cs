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
using UnityEngine.UI;

namespace DreamingEarthling
{
    public class DroidPlayerController : MonoBehaviour
    {

        [Header("Camera")]
        public DroidPlayerCamera _CamController;

        [Header("States")]
        public bool         _IsAlive;
        public bool         _IsWalkingForward;
        public bool         _IsWalkingStrafe;
        public bool         _IsAiming;

        [Header("Attributes")]
        public Faction      _UnitFaction;
        public float        _MovingSpeed;
        public float        _CharacterTurnSpeed;
        public float        _MouseLookSpeed;
        private float       _MouseLookUpDown;
        private float       _MouseLookAround;
        public float        _AimingSpeed;

        [Header("Weapon")]
        public DroidWeapon  _Weapon;
        public float        _ShootingSpeed;
        
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

        [Header("Health System")]
        public float    _HealthMax = 100f;
        public float    _HealthCurrent;
        public Image    _HealthUIIndicator;

        [Header("VFX")]
        public ParticleSystem _DeathSmoke;

        [Header("SFX")]
        public AudioClip    _SFX_Footstep;












        Animator            _Animator;
        Rigidbody           _RigidBody;
        AudioSource         _Audio;


        private float   _InputXRaw;     // Player's mouse input for x axis
        private float   _InputYRaw;     // Player's mouse input for y axis
        private bool    _InputForward;  // Player's input for character movement: default "W"
        private bool    _InputBackward; // Player's input for character movement: default "S"
        private bool    _InputRight;    // Player's input for character movement: default "D"
        private bool    _InputLeft;     // Player's input for character movement: default "A"
        private bool    _InputAiming;
        private bool    _InputFire;






        private void OnEnable()
        {
            _HealthCurrent = _HealthMax;
            _DeathSmoke.Pause();
        }
        private void Awake()
        {
            _Animator = GetComponent<Animator>();
            _RigidBody = GetComponent<Rigidbody>();
            _Audio = GetComponent<AudioSource>();
        }
        // Use this for initialization
        void Start()
        {
            Invoke("Configure", 0.02f);
        }
        void Configure()
        {
            LevelManager._LevelManagerInScene.ReportToLevelMananger( gameObject, _UnitFaction, UnitState.Created );
            LevelManager._LevelManagerInScene._AllyCount ++;

            _IsAlive = true;
            _CamController._Player = gameObject;
            _CamController._CharacterHead = _Animator.GetBoneTransform(HumanBodyBones.Head);
        }



        private void Update()
        {
            // Mouse Cursor
            // keep confined to center of screen
            Cursor.lockState = CursorLockMode.Locked;   

            // Reading all user inputs
            _InputXRaw = Input.GetAxis( "Mouse X" ); // Horizontal for rotation
            _InputYRaw = Input.GetAxis( "Mouse Y" ); // Vertical for rotation


            _InputForward   = Input.GetKey( KeyCode.W );
            _InputBackward  = Input.GetKey( KeyCode.S );
            _InputRight     = Input.GetKey( KeyCode.D );
            _InputLeft      = Input.GetKey( KeyCode.A );

            _InputAiming    = Input.GetKey( KeyCode.Mouse1 );
            _InputFire      = Input.GetKeyDown( KeyCode.Mouse0 );





            Control_CameraRotating();

            // Disable the following functions when the player is down
            if( !_IsAlive )
                return;

            Control_CharacterMoving();
            Control_CharacterRotating();
            Control_Aiming();
            Footsteps();
        }

        private void LateUpdate()
        {
            _CamController.transform.position = new Vector3( transform.position.x, _CamController._CamPivotRigHeight, transform.position.z );
        }














        /// <summary>
        /// Character IK stuff
        /// </summary>
        /// <param name="layerIndex"></param>
        void OnAnimatorIK( int layerIndex )
        {

            if ( _InputAiming )
            {
                _IK_WeaponHolder.position = _Animator.GetBoneTransform( HumanBodyBones.RightShoulder ).position;
                var directionTowardsTarget = _AimReference.position - transform.position;
                var angle = Vector3.Angle( transform.forward, directionTowardsTarget );
                if( angle > 60f )
                {
                    return;
                }

                // Look At Target
                // TODO:: Combine with Target Facing Validation
                _Animator.SetLookAtWeight( _IK_LookWeight, _IK_BodyWeight, _IK_HeadWeight, _IK_EyesWeight, _IK_ClampWeight );
                _Animator.SetLookAtPosition( _AimReference.position );

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














        

        void Control_CameraRotating()
        {
            _MouseLookAround += _InputXRaw * _MouseLookSpeed * Time.deltaTime;
            _MouseLookUpDown -= _InputYRaw * _MouseLookSpeed * Time.deltaTime;
            
            // Clamping Look Up and Down
            _MouseLookUpDown = Utilities.ClampAngle( _MouseLookUpDown, -30f, 20f );
            
            Quaternion toRotation = Quaternion.Euler( _MouseLookUpDown, _MouseLookAround, 0 );
            _CamController.transform.rotation = toRotation;
        }

        void Control_CharacterMoving()
        {
            var speedApplied = _MovingSpeed;

            // Reset
            _Animator.SetFloat( "InputForward", 0 );
            _Animator.SetFloat( "InputSidewalk", 0 );
            _Animator.SetBool( "Walking", false );

            // Equalize the moving speed when character in 45 degree direction walk
            if( (_InputForward || _InputBackward) && (_InputRight || _InputLeft) )
            {
                speedApplied /= 2f;
            }

            // Forward
            if ( _InputForward )
            {
                transform.Translate ( transform.forward * (speedApplied) * Time.deltaTime, Space.World);
                _Animator.SetFloat("InputForward", 1f);
                _Animator.SetBool( "Walking", true );
                _IsWalkingForward = true;
            }

            // Backward
            if ( _InputBackward )
            {
                transform.Translate ( transform.forward * (-speedApplied) * Time.deltaTime, Space.World);
                _Animator.SetFloat("InputForward", -1f);
                _Animator.SetBool( "Walking", true );
                _IsWalkingForward = true;
            }

            // Strafe Right (aiming walk)
            if( _InputAiming && _InputRight )
            {
                transform.Translate ( transform.right * (speedApplied) * Time.deltaTime, Space.World);
                _Animator.SetFloat("InputSidewalk", 1f);
                _Animator.SetBool( "Walking", true );
                _IsWalkingStrafe = true;
            }

            // Strafe Left (aiming walk)
            if( _InputAiming && _InputLeft )
            {
                transform.Translate ( - transform.right * (speedApplied) * Time.deltaTime, Space.World);
                _Animator.SetFloat("InputSidewalk", -1f);
                _Animator.SetBool( "Walking", true );
                _IsWalkingStrafe = true;
            }

            // Side Walk (free walk)
            if( !_InputAiming && ( _InputRight || _InputLeft ) )
            {
                // 45 degree backward walking
                if ( _InputBackward )
                {
                }
                else
                {
                    // 90 degree right/left side walk + 45 degree right/left walk
                    transform.Translate( transform.forward * ( speedApplied ) * Time.deltaTime, Space.World );
                    _Animator.SetFloat( "InputForward", 1f );
                }
                _Animator.SetBool( "Walking", true );
            }

        }

        void Control_CharacterRotating()
        {
            var toDirection = Vector3.zero;
            if ( _InputForward || ( _InputBackward && _InputAiming ) || ( _InputAiming && _InputRight ) || ( _InputAiming && _InputLeft ) )
            {
                toDirection = (
                new Vector3( _CamController._CamPivotForward.position.x, transform.position.y, _CamController._CamPivotForward.position.z )
                - transform.position ).normalized;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation( toDirection ), Time.deltaTime * _CharacterTurnSpeed );
            }

            // Side Walk (while in free walk)
            if( !_InputAiming )
            {
                if ( _InputRight )
                {
                    toDirection = ( new Vector3( _CamController._CamPivotRight.position.x, transform.position.y, _CamController._CamPivotRight.position.z )
                        - transform.position ).normalized;
                    transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( toDirection ), Time.deltaTime * _CharacterTurnSpeed );
                }
                if ( _InputLeft )
                {
                    toDirection = ( new Vector3( _CamController._CamPivotLeft.position.x, transform.position.y, _CamController._CamPivotLeft.position.z )
                        - transform.position ).normalized;
                    transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( toDirection ), Time.deltaTime * _CharacterTurnSpeed );
                }
            }

        }


        float shootingTimer;
        void Control_Aiming()
        {

            if ( !_InputAiming )
            {
                _AimReference.position = _CamController._CamPivotForward.position;
                _Animator.SetBool( "Aiming", false );
                return;
            }

            _Animator.SetBool( "Aiming", true );

            shootingTimer += Time.deltaTime;

            Debug.DrawRay( _CamController._Cam.transform.position, _CamController._Cam.transform.forward * 99f, Color.black );

            // Raycast based Aiming Reference
            Vector3 rayOrigin = _CamController._Cam.transform.position;
            RaycastHit hit;
            if ( Physics.Raycast( rayOrigin, _CamController._Cam.transform.forward, out hit, 100f ) &&
                 transform.InverseTransformPoint( hit.collider.gameObject.transform.position ).z > 1f )
            {
                _AimReference.position = hit.point;
            }
            else
            {
                // No valid aiming target, just pointing forward
                _AimReference.position = _CamController._Cam.transform.TransformPoint( new Vector3( 0, 0, 999f ) );
            }

            // Shoot
            if ( _InputFire && shootingTimer > _ShootingSpeed )
            {
                _Animator.SetTrigger( "ShootSingle" );
                shootingTimer = 0;
                _Weapon.Shoot();
            }
        }













        public void Health_Damaged( float damagePoints )
        {
            if( _HealthCurrent == 0 )
                return;

            // Simple camera shaking to indicate damage
            _CamController._Cam.gameObject.GetComponent<CameraShake>().Shake( .28f, .04f );

            if( damagePoints < _HealthCurrent )
            {
                _HealthCurrent -= damagePoints;
            }
            else
            {
                _HealthCurrent = 0;
                _IsAlive = false;
                _IsAiming = false;
                _Animator.SetFloat( "DeathIndex", (int)Random.Range( 1, 3 ) );
                _HealthUIIndicator.gameObject.SetActive(false);
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                gameObject.GetComponent<Collider>().enabled = false;
                // Show death smoke
                StartCoroutine(SynchronizeDeathSmokeToNPC(_Animator.GetBoneTransform( HumanBodyBones.UpperChest )));
                _DeathSmoke.Play();
                LevelManager._LevelManagerInScene.ReportToLevelMananger( gameObject, Faction.Ally, UnitState.Destroyed );
            } 

           _HealthUIIndicator.fillAmount = ( _HealthCurrent / _HealthMax );
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











        // This function is a simple implementation for generating footstep sound.
        // For more accurate implementation, you may want to use animation events.
        void Footsteps()
        {
            AnimatorStateInfo currentState      = _Animator.GetCurrentAnimatorStateInfo( 0 );
            AnimatorClipInfo[] currentClipInfo  = _Animator.GetCurrentAnimatorClipInfo( 0 );

            float playbackTime = currentState.normalizedTime % 1;
            if ( currentClipInfo[0].clip.name == "Armature|Walk" 
                || currentClipInfo[0].clip.name == "Armature|Strafe-Left"
                || currentClipInfo[0].clip.name == "Armature|Strafe-Left-45D" )
            {
                if ( playbackTime > 0.51f && playbackTime < 0.53f )
                    PlayFootstepSound();

                if( playbackTime > .97f && playbackTime < 0.99f )
                    PlayFootstepSound();
            }
        }
        void PlayFootstepSound()
        {
            _Audio.clip = _SFX_Footstep;
            _Audio.pitch = Random.Range( .75f, 0.9f );
            _Audio.volume = Random.Range( .1f, .2f );
            _Audio.Play();
        }






    }
}

