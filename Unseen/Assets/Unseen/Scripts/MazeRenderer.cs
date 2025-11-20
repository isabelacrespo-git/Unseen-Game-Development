using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] GameObject MazeCellPrefab;

    //this is the physical size of our maze cells, getting this wrong will result in overlapping
    //or visible gaps between each cell
    public float CellSize = 1f;
    
    private bool hasGenerated = false;

    private void Start()
    {
        if (hasGenerated)
        {
            Debug.LogWarning("MazeRenderer: Maze already generated! Skipping duplicate generation.");
            return;
        }
        
        hasGenerated = true;
        Debug.Log($"MazeRenderer: Starting maze generation for {mazeGenerator.mazeWidth}x{mazeGenerator.mazeHeight}");
        
        MazeCell[,] maze = mazeGenerator.GetMaze();
        for(int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for(int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                //instantiate a new maze cell prefab as a child of the MazeRenderer object
                GameObject newCell = Instantiate(MazeCellPrefab, new Vector3((float)x * CellSize, 0f, (float)y * CellSize), Quaternion.identity, transform);

                //get a reference to the cell's MazeCellPrefab script
                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                //determine which walls need to be active
                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;

                //bottom and right walls are deactivated by default unless we are at the bottom or right
                bool right = false;
                bool bottom = false;
                if (x == mazeGenerator.mazeWidth - 1) right = true;
                if (y == 0) bottom = true;

                mazeCell.Init(top, bottom, right, left);
            }
        }
        mazeGenerator.OnMazeGenerationComplete();
    }
}
