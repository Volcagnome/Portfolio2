/*			*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class LevelLoader : MonoBehaviour 
	{

        public GameObject   _StartButton;
        public GameObject   _WelcomeScreen;
        public GameObject[] _ObjsHideBeforeLevelStart;






        private void Awake()
        {
            _WelcomeScreen.SetActive(true);
            for ( int i = 0; i < _ObjsHideBeforeLevelStart.Length; i++ )
            {
                _ObjsHideBeforeLevelStart[i].SetActive(false);
            }
        }
        // Use this for initialization
        void Start () 
		{
            Invoke( "ShowStartButton", 2f );
		}
        void ShowStartButton()
        {
            _StartButton.SetActive(true);
        }



        public void API_Event_LevelLoaded()
        {
            for ( int i = 0; i < _ObjsHideBeforeLevelStart.Length; i++ )
            {
                _ObjsHideBeforeLevelStart[i].SetActive(true);
            }
            _WelcomeScreen.SetActive(false);
            LevelManager._LevelManagerInScene._Level_Started = true;
        }

	
	}
}
