using UnityEngine;

/// <summary>
/// Lesson 7 of the Game Architecture course (1h 42min)
/// replacing Shoot Interactors for bullets and rockets with a Shoot Strategy
/// </summary>

public class BulletShootStrategy : IShootStrategy
{
    
    ShootInteractor _interactor;
    Transform shootPoint;

    public BulletShootStrategy(ShootInteractor interactor)
    {
        Debug.Log("Switched to bullet mode"); 
        _interactor = interactor;
        shootPoint = interactor.GetShootPoint();

        // change color (or model) of the gun to indicate bullet mode
        interactor.gunRender.material.color = interactor.bulletGunColor;
    }
    public void Shoot()
    {
        PooledObject pooledBulletObject = _interactor.bulletPool.GetPooledObject();
        pooledBulletObject.gameObject.SetActive(true);

        Rigidbody bullet = pooledBulletObject.GetComponent<Rigidbody>();
        bullet.transform.position = shootPoint.position;
        bullet.transform.rotation = shootPoint.rotation;


        //Rigidbody bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        bullet.linearVelocity = shootPoint.forward * _interactor.GetShootVelocity();
        //Destroy(bullet.gameObject, 5f); // Destroy bullet after 5 seconds
        _interactor.bulletPool.DestroyPooledObject(pooledBulletObject, 5f); // Return bullet to pool after 5 seconds

    }

    public void AlternateShoot()
    {
        // Implement alternate shoot behavior if needed
        Debug.Log("Alternate shoot not implemented for BulletShootStrategy");
    }
}
