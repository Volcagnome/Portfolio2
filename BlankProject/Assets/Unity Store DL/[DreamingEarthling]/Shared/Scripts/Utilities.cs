/* Some utility function */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class Utilities : MonoBehaviour 
	{


        public bool     _RunAtBackground;


        
        
        
        
        
        // Use this for initialization
        void Start()
        {


            Application.runInBackground = _RunAtBackground;

        }
        private void Update()
        {

            // Pause the Game in Editor
            if ( Input.GetKeyDown( KeyCode.P ) )
            {
                Debug.Break();
            }

            // Reload the current level
            if ( Input.GetKeyDown( KeyCode.F9 ) )
            {
                ReloadActiveScene();
            }

        }








        //clamp angle from before
        public static float ClampAngle( float angle, float min, float max )
        {
            if ( angle < -360F )
                angle += 360F;
            if ( angle > 360F )
                angle -= 360F;
            return Mathf.Clamp( angle, min, max );
        }


        // (Synchronous) Re-load current active scene
        public static void ReloadActiveSceneStatic()
        {
            SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
        }
        public void ReloadActiveScene()
        {
            SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
        }

	
	}
}
