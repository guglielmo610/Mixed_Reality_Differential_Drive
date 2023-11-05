using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AStar{

    int width, height;

    //only for debug/visualization purposes
    private const float CELL_SIZE = 5f;

    //cost of straight movement to a neighbouring cell
    private const int MOVE_STRAIGHT_COST = 10;
    //cost of the diagonal movement
    private const int MOVE_DIAGONAL_COST = 14;  //length of the diagonal

    private List<GridCell> openList;    //nodes to search
    private List<GridCell> closedList;  //nodes already searched

    private AStarGrid grid;             //grid of occupied/free space

    //integer matrix passed in input, it is converted in an AStarGrid object
    private int[,] inputMatrix;

    /*** 
     ** Constructor method
     ** Input params: int [,] inputMatrix: matrix of 0 - free and 1 - occupied cells
    **/
    public AStar(int [,] inputMatrix){
        this.width = inputMatrix.GetLength(0);
        this.height = inputMatrix.GetLength(1);
        
        //map space
        this.grid = new AStarGrid(width,height,CELL_SIZE);

        //the input is converted in to the AStarGrid
        for (int x = 0; x<inputMatrix.GetLength(0);x++){
            for(int y = 0; y< inputMatrix.GetLength(1);y++){
                if (inputMatrix[x,y] == 1){
                    grid.GetCell(x,y).isOccupied = true;
                }
            }
        }
    }

    /***
     ** Find Path method
     ** Implementation of the A* algorithm
     ** Input params: 
     **                 (startX, startY) - starting position 
     **                 (endX, endY) - goal position
     **                 TextMesh debugText - for debug purposes
     ** Output:         
     **                 int[,] path - list of (x,y) positions
    ***/
    public int[,] FindPath(int startX,int startY,int endX, int endY, TextMesh debugText){
        GridCell startNode = new GridCell(startX,startY); 
        GridCell endNode = new GridCell(endX,endY); 

        //Start searching from the start node by adding it to the open list
        openList = new List<GridCell> { startNode };
        //The closed list at the start is empty
        closedList = new List<GridCell>();

        //Initialize costs on each grid cell
        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++){
                grid.SetCellGCost(x,y,int.MaxValue);    //at the beginning the cost is set to infinite
                grid.ComputeCellFCost(x,y);             //f = g+h
                grid.SetCameFromNode(x,y,null);         //camefrom = null
            }
        }

        //Set costs of the start node:
        startNode.SetGCost(0);  // G cost
        // H cost, distance from the start until the goal
        startNode.SetGCost(ComputeDistanceCost(startNode,endNode)); 
        startNode.CalculateFCost(); // F cost

        // while there are nodes on the open list it keeps searching
        while(openList.Count>0){

            GridCell currentNode = GetLowestFCostNode(openList);

            //final node, return the calculated path to reach the end node
            if(currentNode.Equals(endNode)){
                List<GridCell> path = CalculatePath(endNode);


                // converts the output
                int [,]output = new int[path.Count,path.Count];
                if(path!=null){
                    for(int i = 0;i<=path.Count-1;i++){
                        //convert in vector made by pairs of ints for the output
                        output[0,i]= path[i].GetPosition().x;
                        output[1,i]= path[i].GetPosition().y;
                    }
                }
                return output;
            }

            //the node has aready been searched
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            //loop trough the neighbours
            foreach (GridCell neigbourNode in grid.GetNeigbours(currentNode.GetPosition().x,currentNode.GetPosition().y)){
                if(closedList.Contains(neigbourNode)) continue;

                //see if the new cost is lower then the current one
                int tentativeGCost = currentNode.GetGCost()+ComputeDistanceCost(currentNode,neigbourNode);
                if(tentativeGCost<neigbourNode.GetGCost()){
                    neigbourNode.cameFromCell = currentNode;
                    neigbourNode.SetGCost(tentativeGCost);
                    neigbourNode.SetHCost(ComputeDistanceCost(neigbourNode,endNode));
                    neigbourNode.CalculateFCost();

                    int x = neigbourNode.GetPosition().x;
                    int y = neigbourNode.GetPosition().y;

                    grid.SetCellGCost(x,y,tentativeGCost);
                    grid.SetCellHCost(x,y,ComputeDistanceCost(neigbourNode,endNode));
                    grid.ComputeCellFCost(x,y);         
                    grid.SetCameFromNode(x,y,currentNode);

                    if(!openList.Contains(neigbourNode)){
                        openList.Add(neigbourNode);
                    }
                }
            }
        }
        //we have finished the nodes in the open list
        return null;
    }


    //trace back the steps frome the end to the start
    private List<GridCell> CalculatePath(GridCell endNode){
        List<GridCell> path = new List<GridCell>();

        GridCell currentNode = grid.GetCell(endNode.GetPosition().x,endNode.GetPosition().y);
        path.Add(currentNode);

        Debug.Log("current node "+currentNode.ToString()+" came from: "+currentNode.cameFromCell.ToString());
        while(currentNode.cameFromCell != null){
            path.Add(currentNode.cameFromCell);
            currentNode = currentNode.cameFromCell;
        }

        path.Reverse();

        Debug.Log("[CALCULATE PATH]The path is:");

        foreach(GridCell cell in path){
            Debug.Log(cell.ToString());
        }

        return path;
    }

    //h cost is the distance from the End (ignoring the obstacles)
    private int ComputeDistanceCost(GridCell start, GridCell end){
        Vector2Int a = start.GetPosition();
        Vector2Int b = end.GetPosition();

        int xDistance = Mathf.Abs(a.x-b.x);
        int yDistance = Mathf.Abs(a.y-b.y);
        int remaining = Mathf.Abs(xDistance-yDistance);

        //amount we can move diagonally + amount we can move going straight
        return MOVE_DIAGONAL_COST*Mathf.Min(xDistance,yDistance)+MOVE_STRAIGHT_COST*remaining;
    }

    //the current node is the one with lowest f cost
    private GridCell GetLowestFCostNode(List<GridCell> pathList) {
        GridCell lowestFCostNode = pathList[0];
        for (int i = 0;i<pathList.Count;i++){
            if(pathList[i].GetFCost() < lowestFCostNode.GetFCost()){
                lowestFCostNode= pathList[i];
            }
        }

        Debug.Log("Lowest f cost node:"+lowestFCostNode.ToString());
        return lowestFCostNode;
    }

    public AStarGrid GetGrid(){
        return grid;
    }

}