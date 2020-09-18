
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MovementInput : MonoBehaviour {

    [Space]
	[SerializeField] Vector3 desiredMoveDirection = default;
	[SerializeField] float desiredRotationSpeed = 0.1f;
	[SerializeField] float allowPlayerRotation = 0.1f;

	[Header("Animation Smoothing")]
    [Range(0,1f)]
	[SerializeField] float StartAnimTime = 0.3f;
    [Range(0, 1f)]
	[SerializeField] float StopAnimTime = 0.15f;

	[Header("Misc")]
	[SerializeField] SceneLoader sceneLoader = default;
	[SerializeField] int LevelGoal = 10;
	[SerializeField] int CurrentGoalSteps = 0;
	[SerializeField] float Highscore = 100;
	[SerializeField] float CurrentTime = 0;

	[Header("Movement")]
	//Dash stuff
	[SerializeField] float power = 3f;
	[SerializeField] float maxPower = 3f;
	[SerializeField] bool killMode = false;
	[SerializeField] Color defaultLampColor1 = default;
	[SerializeField] Color defaultLampColor2 = default;
	[SerializeField] GameObject jumpEffect = default;

	[Header("UI")]
	[SerializeField] Text powerText = default;
	[SerializeField] Text LevelGoalText = default;
	[SerializeField] Text WarningText = default;
	[SerializeField] Text RunningTimeText = default;
	[SerializeField] Text HighScoreText = default;

	
	[Header("Audio")]
	[SerializeField] AudioClip soundCollect = default;
	[SerializeField] AudioClip soundKill = default;


	AudioSource audioSource;
	Rigidbody rigidb;
	Animator anim;
	Scene scene;
	Light lamp1;
	Light lamp2;
	Camera cam;

	//Jumping aka boosting
	bool wantJump = false;

	//Movement
	float Speed;
	float InputX = default;

	// Use this for initialization
	void Start () {
		anim = this.GetComponent<Animator> ();
		cam = Camera.main;
		rigidb = GetComponent<Rigidbody>();
		scene = SceneManager.GetActiveScene();
		Highscore = PlayerPrefs.GetFloat("HS_" + scene.name, Highscore);
		HighScoreText.text = Highscore.ToString("F2");

		lamp1 = gameObject.transform.Find("Lamp1").GetComponent<Light>();
		lamp2 = gameObject.transform.Find("Lamp2").GetComponent<Light>();
		audioSource = gameObject.GetComponentInChildren<AudioSource>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		InputMagnitude();
		

        if (power >= 3f && wantJump) {
			Instantiate(jumpEffect, transform.position, Quaternion.identity);
			power -= 3f;
			var newVelocity = rigidb.velocity;
			newVelocity.y = 6;
			rigidb.velocity = newVelocity;
		}
		wantJump = false;

	}


	void Update()
	{
		powerText.text = power.ToString("F2") + " Power";
		LevelGoalText.text = CurrentGoalSteps + " / " + LevelGoal + " Batteries";

		CurrentTime += Time.deltaTime;
		RunningTimeText.text = CurrentTime.ToString("F2");

		if (rigidb.velocity.x < -1f || rigidb.velocity.x > 1f)
		{
			power -= Time.deltaTime;

		}
		if (killMode)
		{
			power -= Time.deltaTime*10;

		}

		if (Input.GetButton("Fire3"))
        {
			killMode = true;
        }
        else
        {
			killMode = false;
        }

		if (Input.GetButtonDown("Jump"))
        {
			wantJump = true;
        }
		if (killMode)
        {
			lamp1.color = Color.red;
			lamp2.color = Color.red;
		}
        else
        {
			lamp1.color = defaultLampColor1;
			lamp2.color = defaultLampColor2;

		}
		lamp1.intensity = 5 * power/maxPower;
		lamp2.intensity = 2 * power/maxPower;


		if (transform.position.y < -100)
        {
			sceneLoader.reloadScene();
        }

		if (power <= 0)
		{
			sceneLoader.reloadScene();
		}
		if (power < 10f)
        {
			powerText.color = Color.red;
        }
        else
        {
			powerText.color = Color.white;
		}
	}

	void LateUpdate()
    {
		cam.transform.position = transform.position + new Vector3(0, 3, 10);

    }

    void PlayerMoveAndRotation() {
		InputX = Input.GetAxis ("Horizontal");

		var right = cam.transform.right;
		right.y = 0f;

		right.Normalize();
		desiredMoveDirection = right * InputX;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);

		var newVelocity = rigidb.velocity;
		
		newVelocity.x = -1 * 500 * Time.deltaTime * InputX;
		if (killMode)
        {
			newVelocity.x *= 1.3f;
        }
		rigidb.velocity = newVelocity;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Zombie")
        {
			if (killMode)
            {
				audioSource.PlayOneShot(soundKill);
				collision.gameObject.GetComponent<Enemy>().Die();
				AddPower(4);
			}
            else
            {
				sceneLoader.reloadScene();
			}
		}

	}

    void OnTriggerEnter(Collider other)
    {

		if (other.gameObject.tag == "LevelEnd")
		{
			if (LevelGoal <= CurrentGoalSteps)
            {
				if (Highscore > CurrentTime)
                {

					PlayerPrefs.SetFloat("HS_" + scene.name, CurrentTime);
				}
				sceneLoader.nextScene();
			}
            else
            {
				WarningText.text = "Please collect at least " + LevelGoal +" Batteries!";
            }
		}
		if (other.gameObject.tag == "Battery")
		{
			audioSource.PlayOneShot(soundCollect);
			Destroy(other.gameObject);
			CurrentGoalSteps++;
			AddPower(maxPower);
		}
	}

    void OnTriggerExit(Collider other)
    {
		WarningText.text = "";
    }

	void InputMagnitude() {
		InputX = Input.GetAxis ("Horizontal");
		

		Speed = new Vector2(InputX, 0).sqrMagnitude;


		if (Speed > allowPlayerRotation) {
			anim.SetFloat("Blend", Speed, StartAnimTime, Time.deltaTime);
			PlayerMoveAndRotation();
		}
		else if (Speed < allowPlayerRotation) {
			anim.SetFloat ("Blend", Speed, StopAnimTime, Time.deltaTime);
			var newVelocity = rigidb.velocity;
			newVelocity.x = 0;
			rigidb.velocity = newVelocity;
		}
	}

	void AddPower(float p)
    {
		power = Mathf.Min(maxPower, power + p);
    }
}
