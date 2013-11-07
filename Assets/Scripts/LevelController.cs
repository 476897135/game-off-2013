using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {
    private static bool mCheckpointActive = false;
    private static Vector3 mCheckpoint;

    public static void CheckpointApplyTo(Transform target) {
        if(mCheckpointActive) {
            target.position = mCheckpoint;
        }
    }

    public static void CheckpointSet(Vector3 pos) {
        mCheckpointActive = true;
        mCheckpoint = pos;
    }

    /// <summary>
    /// Call this in gameover, level complete, and level select
    /// </summary>
    public static void ResetData(bool resetLives) {
        mCheckpointActive = false;

        if(resetLives)
            PlayerStats.curLife = PlayerStats.defaultNumLives;

        Weapon.ResetEnergies();
    }
}
