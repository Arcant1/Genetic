﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Singleton = null; // The current EvolutionManager Instance

    int CarCount = 100; // The number of cars per generation
    [SerializeField] GameObject CarPrefab; // The Prefab of the car to be created for each instance
    [SerializeField] Text GenerationNumberText; // Some text to write the generation number
    public static bool filmar { get; set; }
    int GenerationCount = 0; // The current generation number
    public static bool finalizo { get; set; }
    List<Car> Cars = new List<Car>(); // This list of cars currently alive
    public static Vector3 posInicial { get; set; }
    NeuralNetwork BestNeuralNetwork = null;// The best NeuralNetwork currently available
    NeuralNetwork SecondBest = null; //Test mio
    double BestFitness = double.MinValue; // The Fitness of the first NeuralNetwork ever created
    double SecondFitness = double.MinValue;
    public GameObject test;
    public static Vector3 ini { get; set; }

    void Update()
    {
        if(finalizo)
        {
            finalizo = false;
            StartGeneration();
        }
    }
    // On Start
    private void Start()
    {
        if (Singleton == null) // If no other instances were created
            Singleton = this; // Make the only instance this one
        else
            gameObject.SetActive(false); // There is another instance already in place. Make this one inactive.

        BestNeuralNetwork = new NeuralNetwork(Car.NextNetwork); // Set the BestNeuralNetwork to a random new network

        StartGeneration();
    }

    // Sarts a whole new generation
    void StartGeneration()
    {
        filmar = false;
        if (TrackGenerator.PistaCreada)
        {
            filmar = true;
            GenerationCount++; // Increment the generation count
            GenerationNumberText.text = "Generation: " + GenerationCount; // Update generation text

            for (int i = 0; i < CarCount; i++)
            {
                if (i == 0)
                    Car.NextNetwork = BestNeuralNetwork; // Make sure one car uses the best network
                if (i == 1 && GenerationCount > 3)
                {
                    Car.NextNetwork = SecondBest;
                    Car.NextNetwork.Mutate();
                }
                else
                {
                    Car.NextNetwork = new NeuralNetwork(BestNeuralNetwork); // Clone the best neural network and set it to be for the next car
                    Car.NextNetwork.Mutate(); // Mutate it
                }

                Cars.Add(Instantiate(CarPrefab, TrackGenerator.posIni[i], Quaternion.identity, transform).GetComponent<Car>()); // Instantiate a new car and add it to the list of cars
            }
        }
    }

    // Gets called by cars when they die
    public void CarDead(Car DeadCar, double Fitness)
    {
        Cars.Remove(DeadCar); // Remove the car from the list
        Destroy(DeadCar.gameObject); // Destroy the dead car
        if (Fitness > BestFitness) // If it is better that the current best car
        {
            SecondBest = BestNeuralNetwork;
            SecondFitness = BestFitness;
            BestNeuralNetwork = DeadCar.TheNetwork; // Make sure it becomes the best car
            BestFitness = Fitness; // And also set the best fitness
        }
        else if (Fitness > SecondFitness)
        {
            SecondFitness = Fitness;
            SecondBest = DeadCar.TheNetwork;
        }

        if (Cars.Count <= 0) // If there are no cars left
        {
            finalizo = true;
            TrackGenerator.PistaCreada = false;
            StartGeneration(); // Create a new generation
        }
    }
}

