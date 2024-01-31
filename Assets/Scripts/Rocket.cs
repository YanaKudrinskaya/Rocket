using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Rocket : MonoBehaviour
{
    
    [SerializeField] Text energyText;
    [SerializeField] int energyTotal = 1000;
    [SerializeField] int energyApply = 10;
    [SerializeField] int batteryEnergy;
    [SerializeField] float rotSpeed = 100f;
    [SerializeField] float flySpeed = 100f;
    [SerializeField] AudioClip _countdown; //обратный отсчет
    [SerializeField] AudioClip flySound;//звук двигателя
    [SerializeField] AudioClip boomSound;//звук проигрыша
    [SerializeField] AudioClip finishSound;//звук выигрыша
    [SerializeField] ParticleSystem flyParticles;
    [SerializeField] ParticleSystem boomParticles;
    [SerializeField] ParticleSystem finishParticles;
    
    
    bool collisionOff = false;
    //int timer = 10;
    int klick = 0;

    AudioSource audioSource;
    Rigidbody rigidBody;
    enum State { Starting, Playing, Dead, NextLevel };
    State state = State.Playing;
    // Start is called before the first frame update
    void Start()
    {
        energyText.text = energyTotal.ToString();
        state = State.Starting;
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (state == State.Starting)
            PlayingCountdown();
        else if (state == State.Playing)
        {
            Launch();
            Rotation();
        }
        if(Debug.isDebugBuild)
        DebugKeys();
    }
    void DebugKeys ()
    {
        if (Input.GetKeyDown(KeyCode.L))
            LoadNextLevel();
        else if(Input.GetKeyDown(KeyCode.C))
            collisionOff = !collisionOff;
    }
    void PlayingCountdown()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            klick++;
            switch (klick)
            {
                case 1:
                    audioSource.PlayOneShot(_countdown);
                    print("Музыка");
                    Invoke("StatePlaing", 4f);
                    break;
            }
        }
    }
    void StatePlaing ()
    {
        state = State.Playing;
    }
    void OnCollisionEnter(Collision collision)
    {
        if(state == State.Dead || state == State.NextLevel || state == State.Starting || collisionOff)
            return;
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                print("Ok");
                break;
            case "Finish":
                Finish();
                break;
            case "Battery":
                PlusEnergy(batteryEnergy, collision.gameObject);
                break;
            default:
                Lose();
                break;
        }
    }
    void PlusEnergy(int energyToAdd, GameObject batteryObj)
    {
        batteryObj.GetComponent<BoxCollider>().enabled = false;
        energyTotal += energyToAdd;
        
        energyText.text = energyTotal.ToString();
        Destroy(batteryObj);
    }
    void Finish()
    {
        state = State.NextLevel;
        audioSource.Stop();
        audioSource.PlayOneShot(finishSound);

        finishParticles.Play();
        Invoke("LoadNextLevel", 2f);
    }

    void Lose()
    {
        state = State.Dead;
        audioSource.Stop();
        audioSource.PlayOneShot(boomSound);
        boomParticles.Play();
        Invoke("LoadFirstLevel", 5f);
    }
    void LoadNextLevel() //Finish
    {
        int indexCurrentLevel = SceneManager.GetActiveScene().buildIndex;
        int indexNextLevel = indexCurrentLevel + 1;
        if(indexNextLevel == SceneManager.sceneCountInBuildSettings)
        {
            indexNextLevel = 0;
        } 
        SceneManager.LoadScene(indexNextLevel);
    }
   void LoadFirstLevel() //Lose
    {
        SceneManager.LoadScene(1);
    }
    void Launch() 
    {
        if (Input.GetKey(KeyCode.Space) && energyTotal > 0)
        {
            energyTotal -= Mathf.RoundToInt(energyApply * Time.deltaTime);
            energyText.text = energyTotal.ToString();
            rigidBody.AddRelativeForce(Vector3.up * flySpeed * Time.deltaTime);
            if (audioSource.isPlaying == false)
            {
                audioSource.PlayOneShot(flySound);
                flyParticles.Play();
            }
            else
            {
                audioSource.Pause();
                flyParticles.Stop();
            }
            //обратный отсчет. Домашнее задание к 3 и 14 уроку.
            /*klick++;
            switch (klick)
            {
                case 1:
                    Invoke("MyTimer", 1f);
                    break;
            }*/
        }
    }
    void Rotation() 
    {
        float rotationSpeed = rotSpeed * Time.deltaTime;

        rigidBody.freezeRotation = true;
        if (Input.GetKey(KeyCode.A)) 
        {
            transform.Rotate(Vector3.forward * rotationSpeed);
            //transform.Rotate(new Vector3 ( 0, 0, 1 ));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationSpeed);
        }
        rigidBody.freezeRotation = false;
    }
    // Домашка урок 3
    /*void MyTimer() 
    {
        if (timer == 0)
        {
            print("Пуск!");
            return;
        }
        else
        {
            print(timer--);
            Invoke("MyTimer", 1);
        }
    }*/
}
