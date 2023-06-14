using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // References
    public GameObject controller;
    public GameObject movePlate;
    private bool hasMoved = false;
    AudioSource audioData;

    // Piece Position: These represent the board "coordinates," not the actual Unity coordinates
    private int orbitBoard = -1; // Orbit
    private int rayBoard = -1; // Ray

    // Variable to keep track of the "blue" player or "red" player
    private string player;

    // References for all the sprites that the dejarikpiece can be
    public Sprite blue_brute, blue_guardian, blue_predator, blue_scout;
    public Sprite red_brute, red_guardian, red_predator, red_scout;

  

    void OnMouseUp()
    { 
        if (!controller.GetComponent<Game>().IsGameOver() && controller.GetComponent<Game>().GetCurrentPlayer() == player 
        && controller.GetComponent<Game>().ArePiecesEnabled()) 
        {
            DestroyMovePlates();
            InitiateMovePlates();
            InitiateAttackPlates();
        }
    }
    
    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        
        // Take the instantiated location and adjust the transform
        SetCoords();

        // Accesses the Sprite Renderer associated with this Game Object and applies the appropriate sprite
        switch (this.name)
        {
            case "blue_brute" : this.GetComponent<SpriteRenderer>().sprite = blue_brute; player = "blue"; break;
            case "blue_guardian" : this.GetComponent<SpriteRenderer>().sprite = blue_guardian; player = "blue"; break;
            case "blue_predator" : this.GetComponent<SpriteRenderer>().sprite = blue_predator; player = "blue"; break;
            case "blue_scout" : this.GetComponent<SpriteRenderer>().sprite = blue_scout; player = "blue"; break;

            case "red_brute" : this.GetComponent<SpriteRenderer>().sprite = red_brute; player = "red"; break;
            case "red_guardian" : this.GetComponent<SpriteRenderer>().sprite = red_guardian; player = "red"; break;
            case "red_predator" : this.GetComponent<SpriteRenderer>().sprite = red_predator; player = "red"; break;
            case "red_scout" : this.GetComponent<SpriteRenderer>().sprite = red_scout; player = "red"; break;
        }
    }

    public int NormalizeCoordinates(int rayCoord)
    {
        if (rayCoord < 0)
        {
            rayCoord += 12;
            return rayCoord;
        } 
        else if (rayCoord > 11)
        {
            rayCoord -= 12;
            return rayCoord;
        }
        else
        {
            return rayCoord;
        }
    }
    
    // Converts the current board coordinates for the piece this script is attached to into world coordinates and adjusts the piece's transform accordingly
    public void SetCoords() 
    {  
        this.transform.position = controller.GetComponent<Game>().boardTiles.Find(t => t.rNum == orbitBoard && t.aNum == rayBoard).GetTileCenter();
        audioData = GetComponent<AudioSource>();
        audioData.Play(0);
    }

    public int GetOrbitBoard()
    {
        return orbitBoard;
    }

    public int GetRayBoard()
    {
        return rayBoard;
    }

    public void SetOrbitBoard(int orbit)
    {
        orbitBoard = orbit;
    }

    public void SetRayBoard(int ray)
    {
        rayBoard = ray;
    }

    public bool GetHasMoved()
    {
        return hasMoved;
    }

    public void UpdateHasMoved()
    {
        hasMoved = true;
    }

    public void InitiateMovePlates()
    {
        switch (this.name)
        {
            case "blue_brute" :
            case "red_brute" :
                BruteMovePlate();
                break;
            case "blue_guardian" :
            case "red_guardian" :
                GuardianMovePlate();
                break;
            case "blue_predator" :
            case "red_predator" :
                PredatorMovePlate();
                break;
            case "blue_scout" :
            case "red_scout" :
                ScoutMovePlate();
                break;
        }
    }

    public void InitiateAttackPlates()
    {
        switch (this.name)
        {
            case "blue_brute" :
            case "red_brute" :
                BruteAttackPlate();
                break;
            case "blue_guardian" :
            case "red_guardian" :
                GuardianAttackPlate();
                break;
            case "blue_predator" :
            case "red_predator" :
                PredatorAttackPlate();
                break;
            case "blue_scout" :
            case "red_scout" :
                ScoutAttackPlate();
                break;
        }
    }

    // Initiates attack plates if the piece can chain an attack
    public void ChainAttack()
    {
        controller.GetComponent<Game>().DisablePieces();
        DestroyMovePlates();
        InitiateAttackPlates();

        // Creates a move plate at the piece's current position if any attack plates are created
        if (GameObject.FindGameObjectsWithTag("MovePlate").Length > 0)
        {
            PointMovePlate(orbitBoard, rayBoard);
        }
        // Moves to the next turn if no attack plates are created
        if (GameObject.FindGameObjectsWithTag("MovePlate").Length == 0)
        {  
            controller.GetComponent<Game>().EnablePieces();
            controller.GetComponent<Game>().NextTurn();
        }
    }

    // Manages scout turns
    public void ScoutMove(bool attack, bool piecesEnabled)
    {
        // Move first
        if (!attack && piecesEnabled)
        {
            controller.GetComponent<Game>().DisablePieces();
            DestroyMovePlates();
            InitiateAttackPlates();
            // Creates a move plate at the piece's current position if any attack plates are created
            if (GameObject.FindGameObjectsWithTag("MovePlate").Length > 0)
            {
                PointMovePlate(orbitBoard, rayBoard);
            }
            // Moves to the next turn if no attack plates are created
            if (GameObject.FindGameObjectsWithTag("MovePlate").Length == 0)
            {  
                controller.GetComponent<Game>().NextTurn();
                controller.GetComponent<Game>().EnablePieces();
            }
        }
        // Attack after move
        if (attack && !piecesEnabled)
        {
            DestroyMovePlates();
            controller.GetComponent<Game>().NextTurn();
            controller.GetComponent<Game>().EnablePieces();
        }
        // Attack first
        if (attack && piecesEnabled)
        {
            controller.GetComponent<Game>().DisablePieces();
            DestroyMovePlates();
            InitiateMovePlates();
        }
        // Move after attack
        if (!attack && !piecesEnabled)
        {
            DestroyMovePlates();
            controller.GetComponent<Game>().NextTurn();
            controller.GetComponent<Game>().EnablePieces();
        }
    }
    
    // Destroys all MovePlates present on the board
    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            movePlates[i].tag = "NullPlate";
            Destroy(movePlates[i]);
        }
    }

    public void BruteMovePlate()
    {
        PointMovePlate(orbitBoard, rayBoard + 1);
        PointMovePlate(orbitBoard, rayBoard - 1);
        PointMovePlate(orbitBoard + 1, rayBoard);
        PointMovePlate(orbitBoard + 1, rayBoard + 1);
        PointMovePlate(orbitBoard + 1, rayBoard - 1);
        PointMovePlate(orbitBoard - 1, rayBoard);
        PointMovePlate(orbitBoard - 1, rayBoard + 1);
        PointMovePlate(orbitBoard - 1, rayBoard - 1);     
        
        // Allows brute to move backwards off the board in the outer orbit
        if (orbitBoard == 1)
        {
            PointMovePlate(orbitBoard, rayBoard + 6);
        }
    }

    public void PredatorMovePlate()
    {
        PointMovePlate(orbitBoard, rayBoard + 2);
        PointMovePlate(orbitBoard, rayBoard - 2);
        PointMovePlate(orbitBoard + 1, rayBoard);
        PointMovePlate(orbitBoard - 1, rayBoard);
    }

    public void GuardianMovePlate()
    {
        LMovePlate(orbitBoard, rayBoard, 1, 1); 
        LMovePlate(orbitBoard, rayBoard, 1, -1);
        LMovePlate(orbitBoard, rayBoard, -1, 1);
        LMovePlate(orbitBoard, rayBoard, -1, -1);
    }

    public void ScoutMovePlate()
    {
        PointMovePlate(orbitBoard, rayBoard);
        DiagonalMovePlate(orbitBoard, rayBoard, 1, 1);
        DiagonalMovePlate(orbitBoard, rayBoard, 1, -1);
        DiagonalMovePlate(orbitBoard, rayBoard, -1, 1);
        DiagonalMovePlate(orbitBoard, rayBoard, -1, -1);
        RayMovePlate(orbitBoard, rayBoard, 1);
        RayMovePlate(orbitBoard, rayBoard, -1);
    }

    // Generates a MovePlate at the specified (orbit, ray) coordinate
    public void PointMovePlate(int orbit, int ray)
    {
        ray = NormalizeCoordinates(ray);
        Game sc = controller.GetComponent<Game>();
        
        if (sc.PositionOnBoard(orbit, ray))
        {
            GameObject dp = sc.GetPosition(orbit, ray); 

            if (dp == null || dp == this.gameObject)
            {
                MovePlateSpawn(orbit, ray);
            }
        }
    }

    // Generates MovePlates 1 forward or backward and 2 to the left or right, prevents piece hopping
    public void LMovePlate(int orbit, int ray, int orbitIncrement, int rayIncrement)
    // orbitIncrement and rayIncrement can be 1 or -1 depending on intended direction 
    {
        Game sc = controller.GetComponent<Game>();

        if (sc.PositionOnBoard(orbit + orbitIncrement, ray) && sc.GetPosition(orbit + orbitIncrement, ray) == null)
        {
            RayMovePlate(orbit + orbitIncrement, ray, rayIncrement);
        }
        if (sc.PositionOnBoard(orbit + orbitIncrement, ray))
        {
            for (int i = 0; i < 2; i++)
            { 
                ray += rayIncrement;
                ray = NormalizeCoordinates(ray);
                GameObject dp = sc.GetPosition(orbit, ray); 
                if (dp != null)
                {
                    return;
                } 
            }
            if (sc.PositionOnBoard(orbit + orbitIncrement, ray) && sc.GetPosition(orbit + orbitIncrement, ray) == null)
            {
                MovePlateSpawn(orbit + orbitIncrement, ray);
            }
        }
    }

    // Generates MovePlates 2 to the left or 2 to the right, prevents piece hopping
    public void RayMovePlate(int orbit, int ray, int rayIncrement) 
    // rayIncrement can be 1 or -1 depending on intended direction
    {
        Game sc = controller.GetComponent<Game>();

        for (int i = 0; i < 2; i++)
        { 
            ray += rayIncrement;
            ray = NormalizeCoordinates(ray);
            GameObject dp = sc.GetPosition(orbit, ray); 
            if (dp != null)
            {
                return;
            } 
        }
        MovePlateSpawn(orbit, ray);
    }

    // Generates MovePlates at diagonally adjacent spaces, prevents piece hopping
    public void DiagonalMovePlate(int orbit, int ray, int orbitIncrement, int rayIncrement) // orbitIncrement and rayIncrement can be 1 or -1 depending on intended direction
    {
        Game sc = controller.GetComponent<Game>();

        if (sc.PositionOnBoard(orbit + orbitIncrement, ray))
        {
            GameObject dp1 = sc.GetPosition(orbit + orbitIncrement, ray);
            ray += rayIncrement;
            ray = NormalizeCoordinates(ray);
            GameObject dp2 = sc.GetPosition(orbit, ray);
            orbit += orbitIncrement;
            if (dp1 == null || dp2 == null)
            {
                GameObject dp3 = sc.GetPosition(orbit, ray);
                if (dp3 == null)
                {
                    MovePlateSpawn(orbit, ray);
                }
            }
        }
    }

    public void MovePlateSpawn(int orbit, int ray)
    {
        Vector3 tileCenter = controller.GetComponent<Game>().boardTiles.Find(t => t.rNum == orbit && t.aNum == ray).GetTileCenter();
        tileCenter[2] -= 0.5f;
        GameObject mp = Instantiate(movePlate, tileCenter, Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(orbit, ray); // Sets the board coordinates of the MovePlate script
    }

    public void BruteAttackPlate()
    {
        PointAttackPlate(orbitBoard, rayBoard + 1);
        PointAttackPlate(orbitBoard, rayBoard - 1);
        PointAttackPlate(orbitBoard + 1, rayBoard);
        PointAttackPlate(orbitBoard + 1, rayBoard + 1);
        PointAttackPlate(orbitBoard + 1, rayBoard - 1);
        PointAttackPlate(orbitBoard - 1, rayBoard);
        PointAttackPlate(orbitBoard - 1, rayBoard + 1);
        PointAttackPlate(orbitBoard - 1, rayBoard - 1);     
    }

    public void GuardianAttackPlate()
    {
        RayAttackPlate(orbitBoard, rayBoard, 1);
        RayAttackPlate(orbitBoard, rayBoard, -1);
    }

    public void PredatorAttackPlate()
    {
        PointAttackPlate(orbitBoard + 1, rayBoard + 2);
        PointAttackPlate(orbitBoard + 1, rayBoard - 2);
        PointAttackPlate(orbitBoard - 1, rayBoard + 2);
        PointAttackPlate(orbitBoard - 1, rayBoard - 2);
    }

    public void ScoutAttackPlate()
    {
        PointAttackPlate(orbitBoard + 1, rayBoard);
        PointAttackPlate(orbitBoard - 1, rayBoard);
    }

    public void PointAttackPlate(int orbit, int ray)
    {
        ray = NormalizeCoordinates(ray);
        Game sc = controller.GetComponent<Game>();

        if (sc.PositionOnBoard(orbit, ray))
        {
            GameObject dp = sc.GetPosition(orbit, ray);
            
            switch (this.name)
            {
                case "blue_brute" :
                case "red_brute" :
                    if (dp != null && dp.GetComponent<Piece>().player != player && dp.GetComponent<Piece>().GetHasMoved())
                    {
                        AttackPlateSpawn(orbit, ray);
                    }
                    break;
                case "blue_predator" :
                case "red_predator" :
                case "blue_guardian" :
                case "red_guardian" :
                case "blue_scout" :
                case "red_scout" :
                    if (dp != null && dp.GetComponent<Piece>().player != player)
                    {
                        AttackPlateSpawn(orbit, ray);
                    }
                    break;
            }
        }
    }

    // Generates attack plates 2 to the left or 2 to the right, prevents piece hopping
    public void RayAttackPlate(int orbit, int ray, int rayIncrement) 
    // rayIncrement can be 1 or -1 depending on intended direction
    {
        int attackSpace = NormalizeCoordinates(ray + (rayIncrement * 2));
        Game sc = controller.GetComponent<Game>();
        
        for (int i = 0; i < 2; i++)
        {
            ray += rayIncrement;
            ray = NormalizeCoordinates(ray);
            GameObject dp = sc.GetPosition(orbit, ray); 
            if (ray == attackSpace && dp != null && dp.GetComponent<Piece>().player != player)
            {
                AttackPlateSpawn(orbit, ray);
            }
            else if (dp != null)
            {
                return;
            } 
        } 
    }
    
    public void AttackPlateSpawn(int orbit, int ray)
    {
        Vector3 tileCenter = controller.GetComponent<Game>().boardTiles.Find(t => t.rNum == orbit && t.aNum == ray).GetTileCenter();
        tileCenter[2] -= 0.5f;
        GameObject mp = Instantiate(movePlate, tileCenter, Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(orbit, ray); // Sets the board coordinates of the MovePlate script
    }
}