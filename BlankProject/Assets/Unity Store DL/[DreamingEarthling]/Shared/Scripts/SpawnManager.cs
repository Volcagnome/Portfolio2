/* Spawn units at predefined locations */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{
	public class SpawnManager : MonoBehaviour 
	{
        public GameObject   _UnitSpawnedFX;

        [Header("Enemy")]
        public GameObject   _EnemyPrefab;
        public int          _EnemyPerWaveLimit;     // The total number of enemy units should be spawned
        public float        _EnemySpawnRate;        // How fast enemy units have been spawned
        private int         _EnemySpawnedCount;     // The amount of enemy units have been spawned
        public Transform[]  _EnemySpawnLocations;   // The predefined locations at where enemy units enter the scene

        [Header("Ally")]
        public GameObject   _AllyPrefab;
        public int          _AllyPerWaveLimit;
        public float        _AllySpawnRate;
        private int         _AllySpawnedCount;
        public Transform[]  _AllySpawnLocations;





		// Use this for initialization
		void Start () 
		{
		    InvokeRepeating( "SpawnEnemyUnit",  2f, _EnemySpawnRate );
            InvokeRepeating( "SpawnAllyUnit",   2f, _AllySpawnRate );
		}



        void SpawnEnemyUnit()
        {
            if( !LevelManager._LevelManagerInScene._Level_Started )
                return;

            if ( _EnemySpawnedCount >= _EnemyPerWaveLimit )
            {
                return;
            }

            var index = Random.Range(0, _EnemySpawnLocations.Length);
            var spawnSpot = _EnemySpawnLocations[index].position;
            var spawnRotation = _EnemySpawnLocations[index].rotation;
            Instantiate( _UnitSpawnedFX, spawnSpot, spawnRotation, null );
            Instantiate( _EnemyPrefab, spawnSpot, spawnRotation, null );
            LevelManager._LevelManagerInScene._EnemyCount ++;
            _EnemySpawnedCount ++;

            // SFX
            _EnemySpawnLocations[index].gameObject.GetComponent<AudioSource>().Play();
        }

        void SpawnAllyUnit()
        {
            if( !LevelManager._LevelManagerInScene._Level_Started )
                return;

            if( _AllySpawnedCount >= _AllyPerWaveLimit )
                return;

            var index = Random.Range(0, _AllySpawnLocations.Length);
            var spawnSpot = _AllySpawnLocations[index].position;
            var spawnRotation = _AllySpawnLocations[index].rotation;
            Instantiate( _UnitSpawnedFX, spawnSpot, spawnRotation, null );
            Instantiate( _AllyPrefab, spawnSpot, spawnRotation, null );
            LevelManager._LevelManagerInScene._AllyCount ++;
            _AllySpawnedCount ++;

            // SFX
            _AllySpawnLocations[index].gameObject.GetComponent<AudioSource>().Play();
        }











        public bool API_HostileSpawnedCompleted()
        {
            if ( _EnemySpawnedCount >= _EnemyPerWaveLimit )
                return true;
            else
                return false;
        }

	
	}
}
