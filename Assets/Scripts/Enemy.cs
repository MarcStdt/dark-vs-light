using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    GameObject target;
    Rigidbody rigidb;
    Animator animator;
    AudioSource audioSource;

    [SerializeField] List<AudioClip> soundFiles = default;

    int soundCounter = 0;
    bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player");
        audioSource = gameObject.GetComponentInChildren<AudioSource>();
        InvokeRepeating("DoSound", Random.value*3, (Random.value * 3) + 2);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!dead)
        {
            Vector3 dir = (transform.position - target.transform.position).normalized;

            //Our game is 2d
            dir.y = 0;
            dir.z = 0;
            dir.x *= -1;

            var newVelocity = rigidb.velocity;
            newVelocity.x = dir.x;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.2f);
            rigidb.velocity = newVelocity;
        }
        
    }

    public void Die()
    {
        rigidb.useGravity = false;
        //Stop silly rotating on death
        rigidb.constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<BoxCollider>().enabled = false;
        dead = true;
        animator.SetTrigger("Dead");
    }

    void DoSound()
    {
        if (Mathf.Abs((target.transform.position + transform.position).magnitude) > 20 || dead )
        {
            return;
        }
        audioSource.PlayOneShot(soundFiles[soundCounter]);
        if (soundCounter+1 == soundFiles.Count)
        {
            soundCounter = 0;
        }
        else
        {
            soundCounter++;
        }
    }
}
