using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;
    GameObject reference = null; // Reference to the dejarikpiece Game Object that created this MovePlate
    // Board positions, not world positions
    int orbitCoord;
    int rayCoord;
    // false: movement, true: attacking
    public bool attack = false;

    void Start()
    {
        if (attack)
        {
            // Change to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        if (attack)
        {
            GameObject dp = controller.GetComponent<Game>().GetPosition(orbitCoord, rayCoord); // GetPosition() returns a dejarikpiece Game Object
            Destroy(dp);
            controller.GetComponent<Game>().UpdatePieceCount();
            if (controller.GetComponent<Game>().pieceCountBlue == 0)
            {
                controller.GetComponent<Game>().Winner("Red");
            }
            if (controller.GetComponent<Game>().pieceCountRed == 0)
            {
                controller.GetComponent<Game>().Winner("Blue");
            }            
        }

        // Updates coordinates in positions[,] and adjusts piece transform to new position
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Piece>().GetOrbitBoard(), reference.GetComponent<Piece>().GetRayBoard()); 
        reference.GetComponent<Piece>().SetOrbitBoard(orbitCoord); 
        reference.GetComponent<Piece>().SetRayBoard(rayCoord);
        reference.GetComponent<Piece>().SetCoords();
        controller.GetComponent<Game>().SetPosition(reference);
        
        // Sets hasMoved to true
        reference.GetComponent<Piece>().UpdateHasMoved();

        controller.GetComponent<Game>().UpdateSuddenDeath(attack);
        controller.GetComponent<Game>().InitiateSuddenDeath();

        // Manages scout movement
        if (reference.GetComponent<Piece>().name == "blue_scout" || reference.GetComponent<Piece>().name == "red_scout")
        {
            reference.GetComponent<Piece>().ScoutMove(attack, controller.GetComponent<Game>().ArePiecesEnabled());
        }
        else if (attack)
        {
            reference.GetComponent<Piece>().ChainAttack();
        }
        else
        {
            controller.GetComponent<Game>().NextTurn();
            controller.GetComponent<Game>().EnablePieces();
            reference.GetComponent<Piece>().DestroyMovePlates();
        } 
    }

    public void SetCoords(int orbit, int ray)
    {
        orbitCoord = orbit;
        rayCoord = ray;
    }
    
    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}