using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerScript : MonoBehaviour
{
    public GameObject player1SpawnPoint;
    public GameObject player2SpawnPoint;

    public GameObject player1Prefab; // Préfab pour le joueur 1
    public GameObject player2Prefab; // Préfab pour le joueur 2

    private bool player1Connected = false;
    private bool player2Connected = false;

    private void Update()
    {
        if (!player1Connected && Gamepad.current != null && Gamepad.current.startButton.isPressed)
        {
            ConnectPlayer1();
        }

        if (!player2Connected && Gamepad.current != null && Gamepad.current.selectButton.isPressed)
        {
            ConnectPlayer2();
        }
    }

    public void ConnectPlayer1()
    {
        var player1 = PlayerInput.Instantiate(player1Prefab, controlScheme: "Player1", pairWithDevice: Gamepad.current);
        player1.transform.position = player1SpawnPoint.transform.position; // Place le joueur 1 au point de spawn de joueur 1
        player1Connected = true;
    }

    public void ConnectPlayer2()
    {
        var player2 = PlayerInput.Instantiate(player2Prefab, controlScheme: "Player2", pairWithDevice: Gamepad.current);
        player2.transform.position = player2SpawnPoint.transform.position; // Place le joueur 2 au point de spawn de joueur 2
        player2Connected = true;
    }
}
