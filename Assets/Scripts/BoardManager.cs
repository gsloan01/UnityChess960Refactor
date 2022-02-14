using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public bool chess960;

    public static BoardManager Instance { get; set; }
    private bool[,] allowedMoves { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public Chessman[,] Chessmans { get; set; }
    private Chessman selectedChessman;

    public bool isWhiteTurn = true;

    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnAllChessmans(chess960);
        EnPassantMove = new int[2] { -1, -1 };
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    // Select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }

        if (Input.GetKey("escape"))
            Application.Quit();
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null) return;

        if (Chessmans[x, y].isWhite != isWhiteTurn) return;

        bool hasAtLeastOneMove = false;

        allowedMoves = Chessmans[x, y].PossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
            return;

        selectedChessman = Chessmans[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece

                if (c.GetType() == typeof(King))
                {
                    // End the game
                    EndGame();
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                else
                    c = Chessmans[x, y + 1];

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if(y == 7) // White Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, true);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, false);
                    selectedChessman = Chessmans[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedChessman.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedChessman.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y, bool isWhite)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(chessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(chessmanPrefabs[index], position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    IEnumerator ShowPossibilities()
    {
        while(true)
        {
            SpawnAllChessmans(chess960);
            yield return new WaitForSeconds(10);
            EndGame();
        }
    }

    private void SpawnAllChessmans(bool chess960)
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];

        if(!chess960)
        {
            StartChess();

        }
        else
        {

            StartChess960();
            //Generate knights and place a queen in the last open place

            //Generate the opposite side

            //write some tests

            
        }

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true);
        }
        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false);
        }
    }

    void StartChess()
    {
        /////// White ///////

        // King
        SpawnChessman(0, 3, 0, true);
        // Queen
        SpawnChessman(1, 4, 0, true);
        // Rooks
        SpawnChessman(2, 0, 0, true);
        SpawnChessman(2, 7, 0, true);
        // Bishops
        SpawnChessman(3, 2, 0, true);
        SpawnChessman(3, 5, 0, true);
        // Knights
        SpawnChessman(4, 1, 0, true);
        SpawnChessman(4, 6, 0, true);



        /////// Black ///////

        // King
        SpawnChessman(6, 4, 7, false);

        // Queen
        SpawnChessman(7, 3, 7, false);

        // Rooks
        SpawnChessman(8, 0, 7, false);
        SpawnChessman(8, 7, 7, false);

        // Bishops
        SpawnChessman(9, 2, 7, false);
        SpawnChessman(9, 5, 7, false);

        // Knights
        SpawnChessman(10, 1, 7, false);
        SpawnChessman(10, 6, 7, false);
    }
    void StartChess960()
    {
        List<int> pieces = new List<int>();

        GenerateRandomRooks(out int rook1Pos, out int rook2Pos);
        SpawnChessman(2, rook1Pos, 0, true);
        SpawnChessman(2, rook2Pos, 0, true);
        pieces.Add(rook1Pos);
        pieces.Add(rook2Pos);
        Debug.Log("W_Rook - " + rook1Pos);
        Debug.Log("W_Rook - " + rook2Pos);

        GenerateRandomKing(rook1Pos, rook2Pos, out int kingPos);
        SpawnChessman(0, kingPos, 0, true);
        pieces.Add(kingPos);
        Debug.Log("W_King - " + kingPos);

        GenerateRandomBishops(pieces, out int bishop1, out int bishop2);
        SpawnChessman(3, bishop1, 0, true);
        SpawnChessman(3, bishop2, 0, true);
        pieces.Add(bishop1);
        pieces.Add(bishop2);
        Debug.Log("W_Bishop - " + bishop1);
        Debug.Log("W_Bishop - " + bishop2);

        GenerateRandomKnights(pieces, out int knight1, out int knight2);
        SpawnChessman(4, knight1, 0, true);
        SpawnChessman(4, knight2, 0, true);
        pieces.Add(knight1);
        pieces.Add(knight2);

        Debug.Log("W_Knight - " + knight1);
        Debug.Log("W_Knight - " + knight2);

        GenerateRandomQueen(pieces, out int queenPos);
        SpawnChessman(1, queenPos, 0, true);
        pieces.Add(queenPos);

        Debug.Log("W_Queen - " + queenPos);


        PlaceBlack960(pieces);
    }

    private void EndGame()
    {
        if (isWhiteTurn)
            Debug.Log("White wins");
        else
            Debug.Log("Black wins");

        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }

        isWhiteTurn = true;
        BoardHighlights.Instance.HideHighlights();
        SpawnAllChessmans(chess960);
    }

    //This generates rooks that have at least one space open between them
    public static void GenerateRandomRooks(out int rook1Pos, out int rook2Pos)
    {
        rook1Pos = 1;
        rook2Pos = 1;
        while (Mathf.Abs(rook1Pos - rook2Pos) <= 1 || rook1Pos < 0 || rook2Pos < 0 || rook1Pos > 7 || rook2Pos > 7 )
        {
            rook1Pos = UnityEngine.Random.Range(0, 8);
            rook2Pos = UnityEngine.Random.Range(0, 8);
        }

    }
    public static void GenerateRandomKing(int rook1Pos, int rook2Pos, out int kingPos)
    {
        kingPos = -1;
        if (rook1Pos > rook2Pos)
        {
            kingPos = UnityEngine.Random.Range(rook2Pos+1, rook1Pos);
        }
        else
        {
            kingPos = UnityEngine.Random.Range(rook1Pos+1, rook2Pos);
        }

        if(kingPos == rook1Pos || kingPos == rook2Pos)
        {
            Debug.LogError("King messed up");
        }
        else
        {
            
        }
    }
    public static void GenerateRandomBishops(List<int> pieces, out int bishop1, out int bishop2)
    {
        bishop1 = 1;
        bishop2 = 1;
        //           same color                
        while(bishop1 % 2 == bishop2 % 2 || pieces.Contains(bishop1) || pieces.Contains(bishop2) || bishop1 < 0 || bishop2 < 0)
        {
            bishop1 = UnityEngine.Random.Range(0, 8);
            bishop2 = UnityEngine.Random.Range(0, 8);
        }


    }
    public static void GenerateRandomKnights(List<int> pieces, out int knight1, out int knight2)
    {
        knight1 = -1;
        knight2 = -1;

        while(knight1 < 0 || knight2 < 0 || pieces.Contains(knight1) || pieces.Contains(knight2) || knight1 == knight2)
        {
            knight1 = UnityEngine.Random.Range(0, 8);
            knight2 = UnityEngine.Random.Range(0, 8);
        }


    }
    public static void GenerateRandomQueen(List<int> pieces, out int queenPos)
    {
        queenPos = -1;
        while (queenPos < 0 ||  pieces.Contains(queenPos))
        {
            queenPos = UnityEngine.Random.Range(0, 8);
        }


    }

    void PlaceBlack960(List<int> pieces)
    {
        // Rooks
        SpawnChessman(8, pieces[0], 7, false);
        SpawnChessman(8, pieces[1], 7, false);
        // King
        SpawnChessman(6, pieces[2], 7, false);
        // Bishops
        SpawnChessman(9, pieces[3], 7, false);
        SpawnChessman(9, pieces[4], 7, false);

        // Knights
        SpawnChessman(10, pieces[5], 7, false);
        SpawnChessman(10, pieces[6], 7, false);

        // Queen
        SpawnChessman(7, pieces[7], 7, false);
        Debug.Log("B_Rook - " + pieces[0]);
        Debug.Log("B_Rook - " + pieces[1]);
        Debug.Log("B_King - " + pieces[2]);
        Debug.Log("B_Bishop - " + pieces[3]);
        Debug.Log("B_Bishop - " + pieces[4]);
        Debug.Log("B_Knight - " + pieces[5]);
        Debug.Log("B_Knight - " + pieces[6]);
        Debug.Log("B_Queen - " + pieces[7]);

    }

}


