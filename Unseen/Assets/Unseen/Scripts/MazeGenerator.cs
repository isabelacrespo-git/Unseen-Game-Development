using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Range(5, 500)]
    //dimensions of maze
    public int mazeWidth = 5, mazeHeight = 5;

    //position our algorithm will start from
    public int startX, startY;
    //an array of maze cells representing the maze grid
    MazeCell[,] maze;

    //maze cell we are currently looking at
    Vector2Int currentCell;

    public MazeCell[,] GetMaze()
    {
        maze = new MazeCell[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = new MazeCell(x, y);
            }
        }
        //start carving our maze path
        CarvePath(startX, startY);
        return maze;
    }
    List<Direction> directions = new List<Direction> {
    Direction.Up, Direction.Down, Direction.Left, Direction.Right
    };

    List<Direction> GetRandomDirections()
    {
        //make a copy of our directions list so we can alter and mess around with
        List<Direction> dir = new List<Direction>(directions);

        //make directions list to put our randomised directions into
        List<Direction> rndDir = new List<Direction>();

        //loop through and pick a random direction index from dir put it into rndDir 
        //this way rndDir has our elements in a random order
        while (dir.Count > 0)
        {
            int rnd = Random.Range(0, dir.Count);
            rndDir.Add(dir[rnd]);
            dir.RemoveAt(rnd);
        }

        //when we've got all four directions in a random order, return the queue 
        return rndDir;
    }

    //has the cell been visited 
    bool isCellValid(int x, int y)
    {
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].visited) return false;
        else return true;
    }

    //return the first neighbour that has not bee visited or is not outside the boundaries of the map
    //if it cant find one it will return the current cell, meaning we are at a dead end
    //if it returns the same cell as current cell it means there are no valid neighbours

    Vector2Int CheckNeighbours(){
        List<Direction> rndDir = GetRandomDirections();

        for(int i = 0; i < rndDir.Count; i++){
            //set neighbour coordinates to current cell for now
            Vector2Int neighbour = currentCell;

            switch (rndDir[i]){
                case Direction.Up:
                    neighbour.y++;
                    break;
                case Direction.Down:
                    neighbour.y--;
                    break;
                case Direction.Right:
                    neighbour.x++;
                    break;
                case Direction.Left:
                    neighbour.x--;
                    break;
            }
            //if the neighbour we just tried is valid we can return that neighbour if not, we go again
            if (isCellValid(neighbour.x, neighbour.y)) return neighbour;
        }
        return currentCell;

    }

    //Takes in two maze positions and sets the cells accordingly
    void BreakWalls(Vector2Int primaryCell, Vector2Int secondaryCell)
    {
       //We can only go in one direction at time so we handle this using if else statements
       //primary cell's left wall
       if(primaryCell.x > secondaryCell.x){
            maze[primaryCell.x, primaryCell.y].leftWall = false;
       }
       //secondary cell's left wall
       else if(primaryCell.x < secondaryCell.x){
            maze[secondaryCell.x, secondaryCell.y].leftWall = false;
       }
       //primary cell's top wall
       else if(primaryCell.y < secondaryCell.y){
            maze[primaryCell.x, primaryCell.y].topWall = false;
       }
       //secondary cell's top wall
       else if(primaryCell.y > secondaryCell.y){
            maze[secondaryCell.x, secondaryCell.y].topWall = false;
       }
    }

    //starting at the x, y passed in, carves a path through the maze until it encounters a "dead end"
    //if (a dead is a cell with no valid neighbours)
    void CarvePath(int x, int y)
    {
        //perform a quick check to make sure our start position is within the boundaries of the map
        //if not, set them to a default
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1){
            x = y = 0;
            Debug.LogWarning("Starting position is out of bounds, defaulting to 0,0");
        }

        //set current cell to the starting position we were passed.
        currentCell = new Vector2Int(x, y);

        //a list to keep track of our current path
        List<Vector2Int> path = new List<Vector2Int>();

        //loop until we encounter a dead end
        bool deadEnd = false;
        while (!deadEnd)
        {
            //get the next cell we're going to try
            Vector2Int nextCell = CheckNeighbours();

            //if that cell has no valid neighbours, set deadend to true so we break out of the loop
            if(nextCell == currentCell)
            {
                //if that cell has no valid neighbours set deadend to true so we break out of the loop
                for(int i = path.Count - 1; i >= 0; i--)
                {
                    currentCell = path[i];
                    path.RemoveAt(i);
                    nextCell = CheckNeighbours();

                    //if we find a valid neighbour, break out of the loop
                    if (nextCell != currentCell) break;
                }
                if(nextCell == currentCell)
                {
                    deadEnd = true;
                }
            }
            //if we find a valid neighbour we break the walls between the current cell and valid neighbour
            else
            {
                //set wall flags on these two cells
                BreakWalls(currentCell, nextCell);
                //set cell to visited before moving on
                maze[currentCell.x, currentCell.y].visited = true;
                //set the current cell to the valid neighbour
                currentCell = nextCell;
                //add this cell to our path
                path.Add(currentCell);
            }
        }
    }
}

public enum Direction
{
    Up, Down, Left, Right
}
public class MazeCell
{
    public bool visited;
    public int x, y;

    public bool topWall;
    public bool leftWall;

    //return x and y as a vector2int 
    public Vector2Int position
    {
        get
        {
            return new Vector2Int(x, y);
        }
    }
    public MazeCell (int x, int y)
    {
        // the coordinates of this cell in the maze grid
        this.x = x;
        this.y = y;

        //wether the algorithm has visited this cell or not -  false to start
        visited = false;

        //all walls are present until the algorithmn removes them
        topWall = leftWall = true;
    }
}

