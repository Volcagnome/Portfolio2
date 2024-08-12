/*	Used to Fake Accuracy (current version not applied)	*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
    public class DroidAimReference : MonoBehaviour
    {
        // Less these value are, more accurate player aims
        public float    _DraftingFactor;
        public float    _DraftingSpeed;

        public Transform _AimReferenceDraftingSimulator;

        private float   _Timer;
        private Vector3 _DraftingDestination;






        // Use this for initialization
        void Start()
        {

        }


        private void FixedUpdate()
        {
            if ( _Timer < 5f )
            {
                _AimReferenceDraftingSimulator.position = Vector3.MoveTowards( 
                    _AimReferenceDraftingSimulator.position, _DraftingDestination, _DraftingSpeed * Time.deltaTime );

                _Timer += Time.deltaTime;
            }
            else
            {
                _Timer = 0;
                _DraftingDestination = 
                    _AimReferenceDraftingSimulator.TransformPoint( 
                    new Vector3( Random.Range( -_DraftingFactor, _DraftingFactor ), Random.Range( -_DraftingFactor, _DraftingFactor ), 0 ) );
            }
        }


        public Vector3 GetAimReferencePosition()
        {
            return _AimReferenceDraftingSimulator.position;
        }


        public void SetAimReferencePosition( Vector3 pos )
        {
            transform.position = pos;
            _AimReferenceDraftingSimulator.position = pos;
        }


    }
}
