// Simple utility class

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom includes
// set in \Editor\Data\Resources\ScriptTemplates
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using DG.Tweening;
// using PathologicalGames;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class FXClear : MonoBehaviour 
	{

        public float    _Duration;


        private void OnEnable ()
        {
            Invoke( "AutoDisable", _Duration );
        }

        private void OnDisable ()
        {
            Destroy( gameObject );
        }


        void AutoDisable ()
        {
            gameObject.SetActive( false );
        }



    }
}
