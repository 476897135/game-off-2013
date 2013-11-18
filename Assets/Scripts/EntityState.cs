
public enum EntityState {
    Invalid = -1,
    Normal,
    Hurt,
    Dead,
    Stun,

    //for enemies
    BossEntry, //boss enters
    RespawnWait,
    Jump,

    // specific for player
    Lock 
}