namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// Represents a musical note
    /// </summary>
    public enum Pitch : int
    {
        /// <summary>
        /// The C note
        /// </summary>
        C = 0,
        /// <summary>
        /// The C sharp note
        /// </summary>
        CSharp = 1,
        /// <summary>
        /// The enharmonic equivalent of C sharp
        /// </summary>
        DFlat = 1,
        /// <summary>
        /// The D note
        /// </summary>
        D = 2,
        /// <summary>
        /// The D sharp note
        /// </summary>
        DSharp = 3,
        /// <summary>
        /// The enharmonic equivalent of D sharp
        /// </summary>
        EFlat = 3,
        /// <summary>
        /// The E note
        /// </summary>
        E = 4,
        /// <summary>
        /// The F note
        /// </summary>
        F = 5,
        /// <summary>
        /// The F sharp note
        /// </summary>
        FSharp = 6,
        /// <summary>
        /// The enharmonic equivalent of F sharp
        /// </summary>
        GFlat = 6,
        /// <summary>
        /// The G note
        /// </summary>
        G = 7,
        /// <summary>
        /// The G sharp note
        /// </summary>
        GSharp = 8,
        /// <summary>
        /// The enharmonic equivalent of G sharp
        /// </summary>
        AFlat = 8,
        /// <summary>
        /// The A note
        /// </summary>
        A = 9,
        /// <summary>
        /// The A sharp note
        /// </summary>
        ASharp = 10,
        /// <summary>
        /// The enharmonic equivalent of A sharp
        /// </summary>
        BFlat = 10,
        /// <summary>
        /// The B note
        /// </summary>
        B = 11,
        /// <summary>
        /// Represents a rest note
        /// </summary>
        Rest
    }

    /// <summary>
    /// Represents the duration of a musical note
    /// </summary>
    public enum NoteDuration
    {
        /// <summary>
        /// A whole note
        /// </summary>
        Whole = 4000,
        /// <summary>
        /// A half note
        /// </summary>
        Half = 2000,
        /// <summary>
        /// A quarter note
        /// </summary>
        Quarter = 1000,
        /// <summary>
        /// An eighth note
        /// </summary>
        Eighth = 500,
        /// <summary>
        /// A sixteenth note
        /// </summary>
        Sixteenth = 250,
        /// <summary>
        /// A thirty-second note
        /// </summary>
        ThirtySecond = 125,
        /// <summary>
        /// A whole note triplet
        /// </summary>
        WholeTriplet = 6000,
        /// <summary>
        /// A dotted half note
        /// </summary>
        DottedHalf = 3000
    }

    /// <summary>
    /// Represents the sound effects that can be played by the <see cref="SystemSounds"/> class
    /// </summary>
    public enum SystemSoundEffect
    {
        /// <summary>
        /// An alarm or emergency sound effect
        /// </summary>
        Alarm,
        /// <summary>
        /// An alert or notification sound effect
        /// </summary>
        Alert,
        /// <summary>
        /// A simple beep sound effect
        /// </summary>
        Beep,
        /// <summary>
        /// A buzzing or vibrating sound effect
        /// </summary>
        Buzz,
        /// <summary>
        /// A chime or bell sound effect
        /// </summary>
        Chime,
        /// <summary>
        /// A short click sound effect
        /// </summary>
        Click,
        /// <summary>
        /// A failure or error sound effect
        /// </summary>
        Failure,
        /// <summary>
        /// A fanfare or celebratory sound effect
        /// </summary>
        Fanfare,
        /// <summary>
        /// A notification sound effect
        /// </summary>
        Notification,
        /// <summary>
        /// A popping sound effect
        /// </summary>
        Pop,
        /// <summary>
        /// A power-up sound effect
        /// </summary>
        PowerUp,
        /// <summary>
        /// A power-down sound effect
        /// </summary>
        PowerDown,
        /// <summary>
        /// A success or positive feedback sound effect
        /// </summary>
        Success,
        /// <summary>
        /// A short tick or click sound effect
        /// </summary>
        Tick,
        /// <summary>
        /// A warning or caution sound effect
        /// </summary>
        Warning,
    }

    /// <summary>
    /// Represents the sound effects that can be played by the <see cref="GameSounds"/> class
    /// </summary>
    public enum GameSoundEffect
    {
        /// <summary>
        /// A sound effect indicating the activation or use of an item or power-up
        /// </summary>
        Activation,
        /// <summary>
        /// A simple blip sound effect
        /// </summary>
        Blip,
        /// <summary>
        /// A sound effect indicating a boss battle or end challenge
        /// </summary>
        BossBattle,
        /// <summary>
        /// A button press or selection sound effect
        /// </summary>
        ButtonPress,
        /// <summary>
        /// A coin or currency collection sound effect
        /// </summary>
        Coin,
        /// <summary>
        /// A sound effect indicating the collection of an item or bonus
        /// </summary>
        Collectible,
        /// <summary>
        /// A countdown or timer sound effect
        /// </summary>
        Countdown,
        /// <summary>
        /// A sound effect indicating the death or defeat of an enemy
        /// </summary>
        EnemyDeath,
        /// <summary>
        /// An explosion or destruction sound effect
        /// </summary>
        Explosion,
        /// <summary>
        /// A sound effect indicating a footstep or movement
        /// </summary>
        Footstep,
        /// <summary>
        /// A game over or failure sound effect
        /// </summary>
        GameOver,
        /// <summary>
        /// A sound effect indicating a health pickup or healing
        /// </summary>
        Health,
        /// <summary>
        /// A hit or damage sound effect
        /// </summary>
        Hit,
        /// <summary>
        /// A jump or hop sound effect
        /// </summary>
        Jump,
        /// <summary>
        /// A laser or projectile firing sound effect
        /// </summary>
        Laser,
        /// <summary>
        /// A sound effect indicating the completion of a level or challenge
        /// </summary>
        LevelComplete,
        /// <summary>
        /// A menu navigation or selection sound effect
        /// </summary>
        MenuNavigate,
        /// <summary>
        /// A power-up or item pick-up sound effect
        /// </summary>
        PowerUp,
        /// <summary>
        /// A power-down or failure sound effect
        /// </summary>
        PowerDown,
        /// <summary>
        /// A sound effect indicating the discovery of a secret or hidden item
        /// </summary>
        SecretFound,
        /// <summary>
        /// A sound effect indicating a splash in water
        /// </summary>
        Splash,
        /// <summary>
        /// A sound effect indicating a teleport or warp
        /// </summary>
        Teleport,
        /// <summary>
        /// A victory or success sound effect
        /// </summary>
        Victory,
        /// <summary>
        /// A warning or alarm sound effect
        /// </summary>
        Warning,
        /// <summary>
        /// A sound effect indicating a weapon or tool switch
        /// </summary>
        WeaponSwitch,
    }
}