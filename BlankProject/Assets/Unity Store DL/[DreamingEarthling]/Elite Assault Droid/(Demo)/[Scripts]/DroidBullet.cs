/*	
 * This script controls what a bullet can hit,
 * and what SFX and VFX should be spawned.
 *			
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class DroidBullet : MonoBehaviour 
	{
        public float    _Speed; // How fast can bullets travel
        public float    _Life;  // The duration bullets exist before be destroyed if they don't hit anything.
        public float    _DamagePoints; // The amount of damage one bullet causes

        private bool        _HitTarget; // A flag to mark whether this bullet hits something

        public GameObject   _VFX_Hit_OnCharacter; // Visual effect to be spawned after a character is hit.
        public GameObject   _VFX_Hit_OnObstacles; // Visual effect to be spawned after a prop is hit.


        public GameObject   _SFX_MiniPlayer; // The object which plays the hitting sound
        public AudioClip    _SFX_Hit_OnCharacter; // Hitting sound on character
        public AudioClip    _SFX_Hit_OnObstacles; // Hitting sound on props







		// Use this for initialization
		void Start () 
		{
            Invoke( "Death", _Life);
		}
        void Death()
        {
            // Destroy this bullet after its lifespan expired
            Destroy( gameObject );
        }




        private void Update()
        {
            // If this bullet already hit something, it becomes invalid.
            if( _HitTarget )
                return;

            var bulletNextTravelDistance = _Speed * Time.deltaTime;

            var bulletHitObstacleDistance = 999f;

            // We use raycast instead of Collider.OnTriggerEnter() for better accuracy
            RaycastHit hit;
            if ( Physics.Raycast( transform.position, transform.forward, out hit, bulletNextTravelDistance + .2f ) )
            {
                // Checking which Hit FX will be spawned
                var fxPrefab = _VFX_Hit_OnObstacles;
                var sfxClip = _SFX_Hit_OnObstacles;
                
                // Note:: using string comparison is just a quick way to avoid adding custom layer settings.
                // For performance reason, you should consider comparing layers in actual development.
                if ( hit.collider.gameObject.name.Contains( "NPC" ) || hit.collider.gameObject.name.Contains( "Player" ) ) 
                {
                    fxPrefab = _VFX_Hit_OnCharacter;
                    sfxClip =_SFX_Hit_OnCharacter;
                }

                // Target NPC Health Damaged
                if ( hit.collider.transform.root.gameObject.GetComponent<DroidNPCController>() != null )
                {
                    hit.collider.transform.root.gameObject.GetComponent<DroidNPCController>().Health_Damaged( _DamagePoints );
                }
                // Target Player Health Damaged
                if ( hit.collider.transform.root.gameObject.GetComponent<DroidPlayerController>() != null )
                {
                    hit.collider.transform.root.gameObject.GetComponent<DroidPlayerController>().Health_Damaged( _DamagePoints );
                }

                // SFX
                var sfxPlayer = Instantiate( _SFX_MiniPlayer, transform.position, transform.rotation, null );
                sfxPlayer.GetComponent<SFXClear>()._Clip = _SFX_Hit_OnCharacter;

                // VFX
                bulletHitObstacleDistance = Vector3.Distance( transform.position, hit.point );
                var vfx = Instantiate( fxPrefab, hit.point, fxPrefab.transform.rotation );
                var vfxDirection = (vfx.transform.forward - hit.normal).normalized;
                vfx.transform.rotation
                    = Quaternion.FromToRotation( vfx.transform.forward, hit.normal ) * vfx.transform.rotation;

                _HitTarget = true;
                Invoke( "Death", .05f );
            }

            // Bullet doesn't hit anything in this frame, just moving forward
            var bulletTravelDestination = transform.forward * _Speed * Time.deltaTime;
            
            // Calculate the actual distance this bullet travels
            if( bulletHitObstacleDistance < bulletNextTravelDistance )
            {
                bulletTravelDestination = transform.TransformPoint( new Vector3( 0, 0, bulletHitObstacleDistance ) ); 
            }

            // Advance bullet's movement
            transform.Translate ( bulletTravelDestination, Space.World );
        }



















    }
}
