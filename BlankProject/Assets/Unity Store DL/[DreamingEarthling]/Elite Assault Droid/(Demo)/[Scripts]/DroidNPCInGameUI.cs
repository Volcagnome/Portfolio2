/*	This script displays the remaining health of its NPC */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class DroidNPCInGameUI : MonoBehaviour 
	{
        public Transform    _HealthBar;
        public Image        _HealthAmountImage;








        private void OnEnable()
        {
            // Reset the health to full
            _HealthAmountImage.fillAmount = 1;
        }
        private void Awake()
        {
            
        }

        // Use this for initialization
        void Start () 
		{
		
		}

        private void LateUpdate()
        {
        
            transform.LookAt( LevelManager._LevelManagerInScene._CamRigInScene._Cam.transform );
                
        }






        public void RefreshUI( float currentHealth, float maxHealth )
        {
            // Debug.Log( "Health Percentage: " + (currentHealth / maxHealth) );
            _HealthAmountImage.fillAmount = (currentHealth / maxHealth);
        }






        

    }
}
