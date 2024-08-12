

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class DroidPlayerCamera : MonoBehaviour 
	{

        public Camera       _Cam;
        public Transform    _CamHolder; // Camera's default position

        public float        _CamPivotRigHeight;
        public Transform    _CamPivotForward;
        public Transform    _CamPivotRight;
        public Transform    _CamPivotLeft;

        [Header("Cam Clipping")]
        public Transform    _CamClippingHolder; // Move camera here when camera clipping with environment in its default position
        public LayerMask    _LayerMask;
        public Transform    _CharacterHead;
        public bool         _EnableCamClipping; // If you want to prevent camera clipping with environment, set this to FALSE



        [HideInInspector]
        public GameObject _Player;



		// Use this for initialization
		void Start () 
		{
            _Cam.transform.position = _CamHolder.position;
            _Cam.transform.rotation = _CamHolder.rotation;
            _Cam.transform.parent   = _CamHolder;


            
            
		}


        private void LateUpdate()
        {
            PreventCameraClipping();
        }



        void PreventCameraClipping()
        {
            if( _EnableCamClipping )
                return;

            var cameraSwitchingSpeed = Time.deltaTime * 10f ;
            var _DirectionToCameraDefaultPos = _CamHolder.position - transform.position;


            RaycastHit hitFirstCheck;
            if ( Physics.Raycast( transform.position, _DirectionToCameraDefaultPos, out hitFirstCheck, 100f, _LayerMask ) )
            {
                if ( hitFirstCheck.collider.transform.root.gameObject != gameObject )
                {
                    // _Cam.transform.position = _CamClippingHolder.position;
                    // _Cam.transform.rotation = _CamClippingHolder.rotation;
                    _Cam.transform.parent = _CamClippingHolder;

                    _Cam.transform.position = Vector3.Lerp( _Cam.transform.position, _CamClippingHolder.position, cameraSwitchingSpeed );
                    _Cam.transform.localEulerAngles = Vector3.Lerp( _Cam.transform.localEulerAngles, _CamClippingHolder.localEulerAngles, cameraSwitchingSpeed );

                    Helper_SetCharacterPartsVisibility( true );
                }
                else
                {
                    _Cam.transform.position = Vector3.Lerp( _Cam.transform.position, _CamHolder.position, cameraSwitchingSpeed );
                    _Cam.transform.rotation = Quaternion.Lerp( _Cam.transform.rotation, _CamHolder.rotation, cameraSwitchingSpeed );
                    _Cam.transform.parent = _CamHolder;

                    Helper_SetCharacterPartsVisibility( false );
                }

                // Debug.Log( "Camera clipping with " + hitFirstCheck.collider.gameObject.name );
                Debug.DrawLine( transform.position, _Cam.transform.position, Color.white, .001f );
            }
        }






        /// <summary>
        /// Used to hide/show head mesh of the character when necessary.
        /// Reason: When camera in its second holder position (inside the character' head), head mesh may cause weird view artifacts.
        /// </summary>
        /// <param name="hideHead"></param>
        void Helper_SetCharacterPartsVisibility( bool hideHead )
        {
            if( _CharacterHead == null )
            {
                // Debug.Log( "Head not set" );
                return;
            }


            if ( hideHead )
            {
                if( _CharacterHead.localScale.x > 0.1f )
                {
                    _CharacterHead.localScale -= new Vector3( 0.1f, 0.1f, 0.1f );
                }
                else        
                    _CharacterHead.localScale = new Vector3( 0.1f, 0.1f, 0.1f );
            }
            else
            {
                if( _CharacterHead.localScale.x < 1f )
                {
                    _CharacterHead.localScale += new Vector3( 0.1f, 0.1f, 0.1f );
                }
                else
                    _CharacterHead.localScale = new Vector3( 1f, 1f, 1f );
            }
        }
        



    }
}
