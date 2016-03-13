//This script tracks the health of the tower and game status

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TowerHealth : MonoBehaviour
{
	public int numberOfLives = 5;			//How many hits tower can take
	public Image damageImage;				//Full screen red image

    public int currentLives;				//Current number of lives
	AudioSource damageAudio;				//Audio feedback of hit
	bool alive = true;						//Is the tower alive?
	[SerializeField] Text scoreText;
	static ExceptionsLogger exceptionsLogger;

    void Awake()
	{
		//Set current lives and get audio component reference
        currentLives = numberOfLives;
		damageAudio = GetComponent<AudioSource>();
        if (damageImage)
        {
            Color col = damageImage.color;
            col.a = 0f;
            damageImage.color = col;
		}
		exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();
    }

	void OnTriggerEnter(Collider other)
	{
		exceptionsLogger.AddEntry("collider hit!");
		try
		{
			scoreText.text = currentLives.ToString("0");
		}
		catch
		{
			exceptionsLogger.AddEntry("couldn't register collider hit!");
		}
		//Make sure we can only be hit by enemies and only if tower is alive
		if (other.tag != "Enemy" || !alive)
			return;

		Destroy(other.gameObject);
        currentLives -= 1;
		damageAudio.Play();

		scoreText.text = currentLives.ToString("0");

		//If we are out of lives...
		if(currentLives <= 0)
		{
			//...set alive to false and show the red damage image
			//This image will hide the gameplay for 3 seconds
			alive = false;
            if (damageImage)
            {
                Color col = damageImage.color;
                col.a = 1f;
                damageImage.color = col;
            }

			//Restart the gameplay after 3 seconds
			Invoke("Restart", 3f);
		}
	}

	void Restart()
	{
		//While the red image is still up, and before gameplay resumes, find
		//all enemies in the scene and destroy them. It doesn't matter that Find() is
		//very slow since any stutter is hidden by the red image
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
            Destroy(enemies[i]);

		//Reset lives and alive boolean
        currentLives = numberOfLives;
        alive = true;

		//Hide red image
        if (damageImage)
        {
            Color col = damageImage.color;
            col.a = 0f;
            damageImage.color = col;
		}
    }
}
