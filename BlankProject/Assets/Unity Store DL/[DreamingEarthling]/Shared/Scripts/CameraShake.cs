/* A simple camera shaking implementation */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class CameraShake : MonoBehaviour 
	{

		// Use this for initialization
		void Start () 
		{
		
		}





        public void Shake( float duration, float magnitude )
        {
            if( !isShaking )
                StartCoroutine( Thread_Shake(duration, magnitude) );
        }



        private bool isShaking = false;
        IEnumerator Thread_Shake( float duration, float magnitude )
        {
            isShaking = true;

            var originalPos = transform.localPosition;


            float elapsed = 0f;

            while ( elapsed < duration )
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3( x, y, originalPos.z );
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localEulerAngles = originalPos;

            isShaking = false;
        }








	
	}
}
