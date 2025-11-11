using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    private PipePiece[] pipePieces;
    public VatController vatController;

    void Start()
    {
        var allPieces = FindObjectsOfType<PipePiece>();
        pipePieces = System.Array.FindAll(allPieces, p => p.CompareTag("PuzzlePipe"));

        Debug.Log($"[PuzzleManager] Found {pipePieces.Length} pipe pieces.");
    }

    void Update()
    {
        bool allCorrect = true;

        foreach (var piece in pipePieces)
        {
            bool isCorrect = piece.IsCorrectRotation();


            if (!isCorrect)
                allCorrect = false;
        }

        if (allCorrect)
        {
            Debug.Log("All pipes aligned! Puzzle solved.");
            vatController.StartFilling();
            enabled = false;
        }
    }
}
