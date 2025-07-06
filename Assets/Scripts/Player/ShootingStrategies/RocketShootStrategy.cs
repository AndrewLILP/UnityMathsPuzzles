using UnityEngine;

/// <summary>
/// Lesson 7 of the Game Architecture course (1h 42min)
/// replacing Shoot Interactors for bullets and rockets with a Shoot Strategy
/// </summary>

public class RocketShootStrategy : IShootStrategy
{
    ShootInteractor _interactor;
    Transform shootPoint;
    float rocketVelocity = 0.4f; // Adjust this value as needed for rocket speed

    public RocketShootStrategy(ShootInteractor interactor)
    {
        Debug.Log("Switched to rocket mode");
        _interactor = interactor;
        shootPoint = interactor.GetShootPoint();

        // change color (or model) of the gun to indicate rocket mode
        interactor.gunRender.material.color = interactor.rocketGunColor;
    }
    public void Shoot()
    {
        PooledObject pooledRocketObject = _interactor.rocketPool.GetPooledObject();
        pooledRocketObject.gameObject.SetActive(true);

        Rigidbody rocket = pooledRocketObject.GetComponent<Rigidbody>();
        rocket.transform.position = shootPoint.position;
        rocket.transform.rotation = shootPoint.rotation;
        rocket.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);


        //Rigidbody rocket = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        rocket.linearVelocity = shootPoint.forward * _interactor.GetShootVelocity() * rocketVelocity;
        //Destroy(rocket.gameObject, 5f); // Destroy rocket after 5 seconds
        _interactor.rocketPool.DestroyPooledObject(pooledRocketObject, 5f); // Return rocket to pool after 5 seconds

    }


    public void AlternateShoot()
    {
        // Implement alternate shoot behavior if needed
        Debug.Log("Alternate shoot not implemented for RocketShootStrategy");
    }
}
