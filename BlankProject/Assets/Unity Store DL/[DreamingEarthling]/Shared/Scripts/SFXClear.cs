/* Simple utility class */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class SFXClear : MonoBehaviour 
	{
        public AudioSource  _Audio;
        public AudioClip    _Clip;  // Assigned at runtime by who generates the sound

        public bool     _IsPlayOnAwake;
        public bool     _Is3D;
        public float    _Duration;


        


        private void Awake()
        {

            if( _Is3D )
                _Audio.spatialBlend = 1f;

            if( _IsPlayOnAwake )
                _Audio.playOnAwake = true;

        }
        // Use this for initialization
        void Start () 
		{
            _Audio.pitch = Random.Range( .75f, 1.25f );

		    _Audio.clip = _Clip;
            _Audio.Play();

            Destroy( gameObject, _Duration );
		}
	
	}
}
