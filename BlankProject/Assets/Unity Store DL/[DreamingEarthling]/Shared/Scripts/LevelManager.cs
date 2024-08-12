/* LevelManager holds all references of spawned units. 
 NPCs should report to LevelManager when it enters the scene and when it's defeated.
 LevelManager also checks whether the level is won or lost.
 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamingEarthling;

namespace DreamingEarthling
{


    public enum UnitState
    {
        Created,
        Destroyed,
        NULL
    }



	public class LevelManager : MonoBehaviour 
	{
        public static LevelManager _LevelManagerInScene;

        [Header("Level Core Components")]
        public UIManager            _UIManager;
        public GameObject           _PlayerInScene;
        public DroidPlayerCamera    _CamRigInScene;
        public SpawnManager         _SpawnManager;

        [Header("Level states")]
        public bool     _Level_Won;
        public bool     _Level_Lose;
        [HideInInspector]
        public bool     _Level_Started;

        [Header("Units")]
        public GameObject[] _EnemyGroup;
        public int          _EnemyCount; // Show all alive enemies in the scene right now

        public GameObject[] _AllyGroup;
        public int          _AllyCount;



        private void Awake()
        {
            _LevelManagerInScene = this;

            _EnemyGroup = new GameObject[40];
            _AllyGroup  = new GameObject[20];


        }
        private void Start()
        {
            
            InvokeRepeating( "LevelHeartBeat", 10f, 4f );

        }
        /// <summary>
        /// Periodically check the state of the level
        /// </summary>
        private void LevelHeartBeat()
        {
            if( !_Level_Started || !_SpawnManager.API_HostileSpawnedCompleted() )
                return;

            // Level is already finished
            if( _Level_Won || _Level_Lose )
                return;

            // Checking whether the game is finished
            // (a) Assume the level is lost
            _Level_Lose = true;
            for ( int i = 0; i < _AllyGroup.Length; i++ )
            {
                if ( _AllyGroup[i] != null )
                {
                    // As long as there's at least one standing friendly unit and the level is not lost
                    _Level_Lose = false;
                    break;
                }
            }

            // (b) Assume the level is won
            _Level_Won = true;
            for ( int i = 0; i < _EnemyGroup.Length; i++ )
            {
                if ( _EnemyGroup[i] != null )
                {
                    // As long as there's at least one standing hostile unit and the level is not won
                    _Level_Won = false;
                    break;
                }
            }

            // Show the proper ending screen
            if( _Level_Won )
            {
                _UIManager._PlayerUI.SetActive(false);
                _UIManager._EndingScreenWon.SetActive(true);
            }

            if( _Level_Lose )
            {
                _UIManager._PlayerUI.SetActive(false);
                _UIManager._EndingScreenLose.SetActive(true);
            }

        }























        public void ReportToLevelMananger( GameObject obj, Faction faction, UnitState state )
        {
            if ( state == UnitState.Created )
            {
                if ( faction == Faction.Ally )
                {
                    for ( int i = 0; i < _AllyGroup.Length; i++ )
                    {
                        if ( _AllyGroup[i] == null )
                        {
                            _UIManager.PopupMessage( "reinforcement", 1 );

                            _AllyGroup[i] = obj;
                            break;
                        }
                    }
                }

                if ( faction == Faction.Enemy )
                {
                    for ( int i = 0; i < _EnemyGroup.Length; i++ )
                    {
                        if ( _EnemyGroup[i] == null )
                        {
                            _EnemyGroup[i] = obj;
                            break;
                        }
                    }
                }
            }

            if( state == UnitState.Destroyed )
            {
                if ( faction == Faction.Ally )
                {
                    _UIManager.PopupMessage( "ally down", 3 );
                    for ( int i = 0; i < _AllyGroup.Length; i++ )
                    {
                        if ( _AllyGroup[i] == obj )
                        {
                            _AllyGroup[i] = null;
                            break;
                        }
                    }
                }

                if ( faction == Faction.Enemy )
                {
                    _UIManager.PopupMessage( "enemy down", 2 );
                    for ( int i = 0; i < _EnemyGroup.Length; i++ )
                    {
                        if ( _EnemyGroup[i] == obj )
                        {
                            _EnemyGroup[i] = null;
                            break;
                        }
                    }
                }
            }
        }

        public Transform ReturnRandomHostilelUnit( Faction faction )
        {
            if( faction == Faction.Ally )
            {
                var index = Random.Range(0, _EnemyCount);
                if( _EnemyGroup[ index ] != null )
                {
                    return _EnemyGroup[ index ].transform;
                }
            }

            if( faction == Faction.Enemy )
            {
                var index = Random.Range(0, _AllyCount);
                if( _AllyGroup[ index ] != null )
                {
                    return _AllyGroup[ index ].transform;
                }
            }

            return null;
        }



    }
}
