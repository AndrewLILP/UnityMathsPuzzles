using UnityEngine;
using UnityEngine.Pool;

// updated in Lesson 7 of the Game Architecture course (1h 42min)

public class ShootInteractor : Interactor
{
    [SerializeField] private Input inputType;

    [Header("Gun")]
    public MeshRenderer gunRender;
    public Color bulletGunColor;
    public Color rocketGunColor;

    [Header("Object Pooling")]
    public ObjectPool bulletPool;
    public ObjectPool rocketPool;

    private IShootStrategy currentShootStrategy;

    //[SerializeField] private Rigidbody bulletPrefab;
    //[SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float shootVelocity;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private PlayerMovementBehaviour playerMovementBehaviour;

    private float finalShootVelocity;

    public override void Interact()
    {
        // Set a default shoot strategy if none is set
        if (currentShootStrategy == null)
        {
            currentShootStrategy = new BulletShootStrategy(this);
        }


        if (input.weapon1Pressed)
        {
            currentShootStrategy = new BulletShootStrategy(this);
        }
        if (input.weapon2Pressed)
        {
            currentShootStrategy = new RocketShootStrategy(this);
        }

        // Shoot using the current strategy
        if (input.primaryShootPressed && currentShootStrategy != null)
        {
            currentShootStrategy.Shoot();
        }
        if (input.secondaryShootPressed && currentShootStrategy != null)
        {
            currentShootStrategy.AlternateShoot();
        }
    }

    public float GetShootVelocity()
    {
        finalShootVelocity = playerMovementBehaviour.GetForwardSpeed() + shootVelocity;
        return finalShootVelocity;
    }

    public Transform GetShootPoint()
    {
        return shootPoint;
    }

    void Shoot()
    {
        finalShootVelocity = playerMovementBehaviour.GetForwardSpeed() + shootVelocity;

        PooledObject pooledbullet = bulletPool.GetPooledObject();
        pooledbullet.gameObject.SetActive(true);

        Rigidbody bullet = pooledbullet.GetComponent<Rigidbody>();
        bullet.transform.position = shootPoint.position;
        bullet.transform.rotation = shootPoint.rotation;


        //Rigidbody bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        bullet.linearVelocity = shootPoint.forward * finalShootVelocity;
        //Destroy(bullet.gameObject, 5f); // Destroy bullet after 5 seconds
        bulletPool.DestroyPooledObject(pooledbullet, 5f); // Return bullet to pool after 5 seconds

    }
}
