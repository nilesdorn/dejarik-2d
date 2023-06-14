using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Board coordinate system: positions are defined in (orbit, ray) pairs
// orbit (0-1) represents the ring with 0 as the inner ring and 1 as the outer ring
// ray (0-11) represents the space within the ring with 0 as the space directly above the x axis on the right and progressing counter-clockwise to 11
public class Game : MonoBehaviour
{
    // References
    public GameObject dejarikpiece;
    // Positions and team for each dejarik piece
    public List<boardTile> boardTiles;
    public int pieceCountBlue = 4;
    public int pieceCountRed = 4;
    private int suddenDeathBlue = -1;
    private int suddenDeathRed = -1;
    private GameObject[,] positions = new GameObject[2, 12];
    private GameObject[] playerBlue = new GameObject[4];
    private GameObject[] playerRed = new GameObject[4];
    private string currentPlayer = "blue";
    private bool gameOver = false;
    private bool piecesEnabled = true;
    

    // Start is called before the first frame update
    void Start()
    {
        InitializeBoard();
        playerBlue = new GameObject[]{ 
            Create("blue_brute", 0, 9), Create("blue_guardian", 0, 8),
            Create("blue_predator", 1, 8), Create("blue_scout", 1, 9) };
        playerRed = new GameObject[]{
            Create("red_brute", 0, 3), Create("red_guardian", 1, 3),
            Create("red_predator", 1, 2), Create("red_scout", 0, 2) };

        // Set all piece positions on the position board
        for (int i = 0; i < playerBlue.Length; i++)
        {
            SetPosition(playerBlue[i]);
            SetPosition(playerRed[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver && Input.GetMouseButtonDown(0)) 
        {
            gameOver = false;
            SceneManager.LoadScene("Game"); // Reloads the game 
        }
    }

    public void Winner(string playerWinner)
    {
        gameOver = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }

    // Creates boardTile objects for each tile on the board and adds them to the list boardTiles
    private void InitializeBoard()
    {
        boardTiles = new List<boardTile>();
        
        float tb = 1.707f; // Top border
        float bb = 0.853f; // Bottom border

        for (int i = 0; i < 2; i++)
        {
            int lb = 30; // Left border
            int rb = 0; // Right border
            for (int j = 0; j < 12; j++)
            {
                int x = i; // Radius coordinate
                int y = j; // Angular coordinate
                var tile = new boardTile(x, y, lb, rb, tb, bb); 
                boardTiles.Add(tile); // Adds the constructed tile to the list boardTiles
                lb += 30;
                rb += 30;
            }
            tb = 2.56f;
            bb = 1.707f;
        }
    }

    // Currently unused
    public Vector3 GetWorldCoordinates(int x, int y)
    {
        return boardTiles.Find(t => t.rNum == x && t.aNum == y).GetTileCenter();
    }

    public GameObject Create(string name, int orbit, int ray)
    {
        GameObject obj = Instantiate(dejarikpiece, new Vector3(0, 0, -1), Quaternion.identity); // Instantiates dejarikpiece prefab at (0, 0, -1)
        Piece dp = obj.GetComponent<Piece>(); // Accesses the "Piece" script attached to the instantiated prefab
        dp.name = name; // Sets the name of the attached script to the name given in the function call
        dp.SetOrbitBoard(orbit); // Sets the orbitBoard int declared in the attached script to the value given in the function call
        dp.SetRayBoard(ray); // Sets the rayBoard int declared in the attached script to the value given in the function call
        dp.Activate(); // Adjusts the position of the instantiated piece to the given (x, y) board position and displays the corresponding sprite
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Piece dp = obj.GetComponent<Piece>(); // Accesses the "Piece" script attached to the instantiated prefab
        positions[dp.GetOrbitBoard(), dp.GetRayBoard()] = obj; // Sets the corresponding position in "positions" equal to the Game Object
    }

    public void SetPositionEmpty(int orbit, int ray) 
    {
        positions[orbit, ray] = null;
    }

    public GameObject GetPosition(int orbit, int ray) // Returns the Game Object at positions[x, y]
    {
        return positions[orbit, ray];
    }

    public bool PositionOnBoard(int orbit, int ray) // Checks if a given position is on the board
    {
        if (orbit < 0 || ray < 0 || orbit >= positions.GetLength(0) || ray >= positions.GetLength(1)) return false;
        return true;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void EnablePieces()
    {
        piecesEnabled = true;
    }

    public void DisablePieces()
    {
        piecesEnabled = false;
    }

    public bool ArePiecesEnabled()
    {
        return piecesEnabled;
    }
    public bool IsGameOver()
    {
        return gameOver;
    }

    public void NextTurn()
    {
        if (currentPlayer == "blue")
        {
            currentPlayer = "red";
        }
        else
        {
            currentPlayer = "blue";
        }
    }
    
    // Subtracts one piece from the count of the opposing player's team
    public void UpdatePieceCount()
    {
        if (currentPlayer == "blue")
        {
            pieceCountRed -= 1;
        }
        if (currentPlayer == "red")
        {
            pieceCountBlue -= 1;
        }
    }

    public void InitiateSuddenDeath()
    {
        if (suddenDeathBlue < 0 && pieceCountBlue == 1)
        {
            suddenDeathBlue = 3;
            GameObject.FindGameObjectWithTag("BlueSDText").GetComponent<Text>().enabled = true;
            GameObject.FindGameObjectWithTag("BlueSDText").GetComponent<Text>().text = "Blue: " + suddenDeathBlue + " turns remaining";
        }
        if (suddenDeathRed < 0 && pieceCountRed == 1)
        {
            suddenDeathRed = 3;
            GameObject.FindGameObjectWithTag("RedSDText").GetComponent<Text>().enabled = true;
            GameObject.FindGameObjectWithTag("RedSDText").GetComponent<Text>().text = "Red: " + suddenDeathRed + " turns remaining";
        }
    }

    // Updates turn counter each turn and resets turn counter on attack
    public void UpdateSuddenDeath(bool attack)
    {
        if (attack)
        {
            if (currentPlayer == "blue" && suddenDeathBlue > 0)
            {
                suddenDeathBlue = 3;
                GameObject.FindGameObjectWithTag("BlueSDText").GetComponent<Text>().text = "Blue: " + suddenDeathBlue + " turns remaining";
            }
            if (currentPlayer == "red" && suddenDeathRed > 0)
            {
                suddenDeathRed = 3;
                GameObject.FindGameObjectWithTag("RedSDText").GetComponent<Text>().text = "Red: " + suddenDeathRed + " turns remaining";
            }
        }
        if (!attack)
        {
            if (currentPlayer == "blue" && suddenDeathBlue > 0)
            {
                suddenDeathBlue -= 1;
                GameObject.FindGameObjectWithTag("BlueSDText").GetComponent<Text>().text = "Blue: " + suddenDeathBlue + " turns remaining";
            }
            if (currentPlayer == "blue" && suddenDeathBlue == 0)
            {
                Winner("Red");
            }
            if (currentPlayer == "red" && suddenDeathRed > 0)
            {
                suddenDeathRed -= 1;
                GameObject.FindGameObjectWithTag("RedSDText").GetComponent<Text>().text = "Red: " + suddenDeathRed + " turns remaining";
            }
            if (currentPlayer == "red" && suddenDeathRed == 0)
            {
                Winner("Blue");
            }
        }  
    }
}



public class boardTile
{
    // Board coordinates
    public int rNum; // Radius coordinate, specifying rim (1-2)
    public int aNum; // Angular coordinate, specifying space (1-12)

    // Tile borders, as viewed facing the tile from the board's center
    private float lBorder; // Left border, measured in degrees
    private float rBorder; // Right border, measured in degrees 
    private float tBorder; // Top border, measured in units
    private float bBorder; // Bottom border, measured in units

    private Vector3 tileCenter; // Location of a tile's mathematical center in world coordinates

    // Class constructor
    public boardTile(int x, int y, int leftBorder, int rightBorder, float topBorder, float bottomBorder)
    {    
        
        // Sets the declared class variables equal to those passed in the constructor call
        rNum = x;
        aNum = y;
        lBorder = leftBorder;
        rBorder = rightBorder;
        tBorder = topBorder;
        bBorder = bottomBorder;
    
        float centerAng = lBorder - ((lBorder - rBorder) / 2); // Calculates the angle to the center of the tile from the +x axis
        float centerRad = tBorder - ((tBorder - bBorder) / 2); // Calculates the radius of the center of the tile from the the origin

        // Calculates the position of the center of the tile in world coordinates
        if (centerAng >= 0 && centerAng < 90)
        {
            float worldX = Mathf.Cos(centerAng * Mathf.Deg2Rad) * centerRad;
            float worldY = Mathf.Sqrt(Mathf.Pow(centerRad, 2) - Mathf.Pow(worldX, 2));
            tileCenter = new Vector3(worldX, worldY, -1.0f);
        }
        else if (centerAng >= 90 && centerAng < 180)
        {
            float worldX = Mathf.Sin((centerAng - 90) * Mathf.Deg2Rad) * centerRad;
            float worldY = Mathf.Sqrt(Mathf.Pow(centerRad, 2) - Mathf.Pow(worldX, 2));
            tileCenter = new Vector3(-worldX, worldY, -1.0f);
        }
        else if (centerAng >= 180 && centerAng < 270)
        {
            float worldX = Mathf.Cos((centerAng - 180) * Mathf.Deg2Rad) * centerRad;
            float worldY = Mathf.Sqrt(Mathf.Pow(centerRad, 2) - Mathf.Pow(worldX, 2));
            tileCenter = new Vector3(-worldX, -worldY, -1.0f);
        }
        else if (centerAng >= 270 && centerAng < 360)
        {
            float worldX = Mathf.Sin((centerAng - 270) * Mathf.Deg2Rad) * centerRad;
            float worldY = Mathf.Sqrt(Mathf.Pow(centerRad, 2) - Mathf.Pow(worldX, 2));
            tileCenter = new Vector3(worldX, -worldY, -1.0f);
        }
        else
        {
            Debug.Log("boardTile constructor error");
        }
    }

    public Vector3 GetTileCenter()
    {
        return tileCenter;
    }
}