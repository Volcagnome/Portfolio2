/* Manage all UI related stuff for this level */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class UIManager : MonoBehaviour 
	{
        [Header("Player UI")]
        public GameObject   _PlayerUI;

        [Header("Welcome Screen")]
        public CanvasGroup  _WelcomeScreenCanvas;
        public GameObject   _EndingScreenWon;
        public GameObject   _EndingScreenLose;
        
        [Header("Pop-up Message")]
        public GameObject       _PopupPrefab_R; // Reinforcement
        public GameObject       _PopupPrefab_E; // Enemy down
        public GameObject       _PopupPrefab_A; // Ally down
        public RectTransform    _PopupDst;
        public RectTransform    _PopupSrc;



		// Use this for initialization
		void Start () 
		{
		    _EndingScreenWon.SetActive(false);
            _EndingScreenLose.SetActive(false);

            StartCoroutine(DisplayingWelcomeScreen());
		}





        IEnumerator DisplayingWelcomeScreen()
        {

            while( !LevelManager._LevelManagerInScene._Level_Started )
            {
                yield return null;
            }

            _WelcomeScreenCanvas.gameObject.SetActive(false);
            yield return new WaitForSeconds(.2f);


            _WelcomeScreenCanvas.alpha = 1f;
            _WelcomeScreenCanvas.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.2f);
            while( _WelcomeScreenCanvas.alpha > 0 )
            {
                _WelcomeScreenCanvas.alpha -= Time.deltaTime * 1f;
                yield return null;
            }
            _WelcomeScreenCanvas.gameObject.SetActive(false);
        }







        public void PopupMessage( string content, int popUpIndex )
        {
            if( popUpIndex == 1 )
                StartCoroutine( CreatePopupMessage(content, _PopupPrefab_R ) );
            if( popUpIndex == 2 )
                StartCoroutine( CreatePopupMessage(content, _PopupPrefab_E ) );
            if( popUpIndex == 3 )
                StartCoroutine( CreatePopupMessage(content, _PopupPrefab_A ) );
        }
        IEnumerator CreatePopupMessage( string content, GameObject prefab )
        {
            // SFX
            GetComponent<AudioSource>().pitch   = Random.Range( .85f, 1.15f );
            GetComponent<AudioSource>().volume  = Random.Range( .25f, 0.32f );
            GetComponent<AudioSource>().Play();

            var popUp = Instantiate( prefab, _PopupSrc.position, prefab.transform.rotation, prefab.transform.parent );
            popUp.GetComponentInChildren<Text>().text = content;

            var t = 0f;
            while( t < 1f )
            {
                popUp.transform.position = Vector3.Lerp( popUp.transform.position, _PopupDst.position, Time.deltaTime * 20f );
                t += Time.deltaTime;
                yield return new WaitForSeconds(.02f);
            }

            yield return new WaitForSeconds(1);
            Object.Destroy(popUp);
        }




	
	}
}
