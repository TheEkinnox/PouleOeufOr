using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 5.0f;
    public float acceleration = 10.0f;
    public float maxSpeed = 20.0f;
    public float distancePerTap = 1.0f; // Distance parcourue par tap

    private float currentSpeed = 0.0f;
    private Vector2 rotationInput;
    private float distanceTraveled = 0.0f;
    private GameObject carriedObject; // R�f�rence � l'objet port�
    private bool isStunned = false;
    private float stunEndTime;

    private PlayerInput playerInput;

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();

        // Activez l'Input Action Map correspondant � votre carte d'actions
        playerInput.actions.FindActionMap("PlayerControls").Enable();

        playerInput.actions["Rotate"].performed += OnRotatePerformed;
        playerInput.actions["Rotate"].canceled += OnRotateCanceled;
        playerInput.actions["Accelerate"].performed += OnAcceleratePerformed;
    }

    private void OnDisable()
    {
        // D�sactivez les callbacks lorsque le script est d�sactiv�
        var playerControls = playerInput.actions.FindActionMap("PlayerControls");
        playerControls.Disable();

        playerControls["Rotate"].performed -= OnRotatePerformed;
        playerControls["Rotate"].canceled -= OnRotateCanceled;
        playerControls["Accelerate"].performed -= OnAcceleratePerformed;
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<Vector2>();
        // Utilisez rotationInput pour effectuer la rotation du personnage ici
    }

    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        // R�initialisez la rotation lorsque le joystick est rel�ch�
        rotationInput = Vector2.zero;
    }

    private void OnAcceleratePerformed(InputAction.CallbackContext context)
    {
        if (distanceTraveled < distancePerTap && !isStunned)
        {
            currentSpeed = acceleration;
        }
    }

    private void Update()
    {
        if (!isStunned)
        {
            // Appliquez la rotation en fonction de rotationInput
            transform.Rotate(new Vector3(0, 0, rotationInput.x * rotationSpeed * Time.deltaTime));

            if (currentSpeed > 0)
            {
                float distanceThisFrame = currentSpeed * Time.deltaTime;
                distanceTraveled += distanceThisFrame;

                if (distanceTraveled >= distancePerTap)
                {
                    currentSpeed = 0f;
                    distanceTraveled = 0f;
                }

                // Appliquez la vitesse au mouvement du personnage
                Vector3 movement = new Vector3(0, distanceThisFrame, 0);
                transform.Translate(movement);
            }
        }
        else if (Time.time >= stunEndTime)
        {
            isStunned = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isStunned && collision.gameObject.CompareTag("Collectible"))
        {
            // Le joueur a ramass� un objet collectible
            carriedObject = collision.gameObject;

            // D�sactivez l'objet collectible pour le "ramasser"
            carriedObject.SetActive(false);
        }
        else if (carriedObject != null && collision.gameObject.CompareTag("Player"))
        {
            // Un autre joueur touche le joueur portant l'objet
            // �tourdissez le joueur pendant un certain temps (par exemple, 2 secondes)
            isStunned = true;
            stunEndTime = Time.time + 2.0f;

            // R�activez l'objet collectible et placez-le � la position actuelle du joueur
            carriedObject.SetActive(true);
            carriedObject.transform.position = transform.position;

            // R�initialisez la r�f�rence � l'objet port�
            carriedObject = null;
        }
    }
}

