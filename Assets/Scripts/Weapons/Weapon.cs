using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(SphereCollider))]

public class Weapon : MonoBehaviour,IInteractable
{
    public int damage = 10;
    public int maxAmmo = 500;
    public int maxClip = 30;
    public float range = 10f;
    public float shootRate = .2f;
    public float lineDelay = .1f;
    public Transform shotOrigin;

    private int ammo = 0;
    private int clip = 0;
    private float shootTimer = 0f;
    private bool canShoot = false;

    private Rigidbody rigid;
    private BoxCollider boxCollider;
    private LineRenderer line;
    private SphereCollider sphereCollider;


    void GetReferences()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        line = GetComponent<LineRenderer>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Reset()
    {
        GetReferences();

        // Collect all bounds inside of children
        Renderer[] children = GetComponentsInChildren<MeshRenderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer rend in children)
        {
            bounds.Encapsulate(rend.bounds);
        }

        // Turn off line renderer
        line.enabled = false;

        // Turn off rigidbody
        rigid.isKinematic = false;

        // Apply bounds to box collider
        boxCollider.center = bounds.center - transform.position;
        boxCollider.size = bounds.size;
        //enable trigger
        sphereCollider.isTrigger = true;
        sphereCollider.center = boxCollider.center;
        sphereCollider.radius = boxCollider.size.magnitude * .5f;
    }

    void Awake()
    {
        GetReferences();    
    }

    // Update is called once per frame
    void Update()
    {
        //Increases Shoot timer
        shootTimer += Time.deltaTime;
        //If time reaches rate
        if(shootTimer >= shootRate)
        {
            //Able to shoot
            canShoot = true;
        }
    }
    public void Pickup()
    {
        rigid.isKinematic = true;
    }
    public void Drop()
    {
        //Enable Rigidbody
        rigid.isKinematic = false;

    }
    public string GetTitle()
    {
        return "Weapon";
    }
    IEnumerator ShotLine(Ray bulletRay, float lineDelay)
    {
        //Run logic before
        line.enabled = true;
        line.SetPosition(0, bulletRay.origin);
        line.SetPosition(1, bulletRay.origin + bulletRay.direction * range);
        yield return new WaitForSeconds(lineDelay);
        //Run logic after
        line.enabled = false;
    }
    public virtual void Reload()
    {
        //this is terrible, dont really use
        clip += ammo;
        ammo -= maxClip;
    }
    public virtual void Shoot()
    {
        if (canShoot)
        {
            //Create a bullet ray from shot origin to forward
            Ray bulletRay = new Ray(shotOrigin.position, shotOrigin.forward);
            RaycastHit hit;
            if (Physics.Raycast(bulletRay, out hit, range))
            {
                IKillable killable = hit.collider.GetComponent<IKillable>();
                if(killable != null)
                {
                    killable.TakeDamage(damage);
                }
            }
            //Show line
            StartCoroutine(ShotLine(bulletRay, lineDelay));
            //Reset timer
            shootTimer = 0;
            //cant shoot
            canShoot = false;
        }
    }

}
