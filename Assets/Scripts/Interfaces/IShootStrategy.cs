using UnityEngine;

/// <summary>
/// created during Lesson 7 of the Game Architecture course (1h 42min)
/// replacing Shoot Interactors for bullets and rockets with a Shoot Strategy
/// </summary>

public interface IShootStrategy
{

    void Shoot();
    void AlternateShoot(); // Optional, if melee attacks are implemented

}


