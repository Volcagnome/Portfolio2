/*	Shoot the bullet and make the sound	*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class DroidWeapon : MonoBehaviour 
	{
        public bool _Debug;

        public Transform    _AimPivot;

        public GameObject   _Bullet;



        [Header("IK")]
        public Transform _IK_LeftHand;





        AudioSource _Audio;






        private void Awake()
        {
            _Audio = GetComponent<AudioSource>();
        }
        // Use this for initialization
        void Start () 
		{
		
		}

        private void Update()
        {
            if( _Debug )
            {
                Debug.DrawRay( transform.position, transform.forward * 10f, Color.red );
            }
        }


        public void Shoot()
        {

            Instantiate( _Bullet , _AimPivot.position, _AimPivot.rotation );

            _Audio.pitch = Random.Range( 0.80f, 1.20f );
            _Audio.Play();
        }





	
	}
}
