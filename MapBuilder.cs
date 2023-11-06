using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Linq;

public class MapBuilder
{
    // // Start is called before the first frame update
    // public TextMesh savedText, mapPrint ; 
    public int[] mapSize; 
    private float[] roomExtents= {0f,0f,0f,0f}; 
    private int counter = 0; 
    private float xCellSize, zCellSize, floorLevel, ceilingLevel, neighbourMajority;  
    public float floorLevelThreshold;
    private String debugText, mapText;
    public bool expandObstacles;
    int [,] lastProcessedMap;

    int blockSize, coreSize;
    TextMesh debugTextMesh;

    public MapBuilder(TextMesh debugTextMesh,
                        int xMapSize = 50,
                        int zMapSize = 50,
                        float floorLevelThreshold = 0.2f, 
                        bool expandObstacles = false, 
                        int blockSize = 4,
                        int coreSize = 2, 
                        float neighbourMajority = 0.8f
                      )
    {
        this.mapSize = new int[]{xMapSize,zMapSize};
        this.floorLevelThreshold = floorLevelThreshold;
        this.lastProcessedMap = null;
        this.expandObstacles = expandObstacles;
        this.debugTextMesh = debugTextMesh; 
        this.blockSize = blockSize;
        this.coreSize = coreSize;
        this.neighbourMajority = neighbourMajority;

    }

    public int[,] BuildRoomMap(IReadOnlyDictionary<int,SpatialAwarenessMeshObject> meshes){
        FindRoomBounds(meshes); 
        debugText+= "Room extents are: " + roomExtents[0].ToString() + "," + roomExtents[1].ToString() + "," 
                                                + roomExtents[2].ToString() + "," + roomExtents[3].ToString() + "\n"; 
        debugText += String.Format("Ceiling and floor levels:{0} and {1}\n", floorLevel, ceilingLevel);

        int[,] processedMap = new int[mapSize[0], mapSize[1]]; 
            // initialise map with zeros
        for (int x=0; x < mapSize[0]; x++){
            for (int z = 0; z < mapSize[1]; z++){
                processedMap[x,z] = 0; 
            }
        }
            // debugText+= "Initialised map, first elements: " + processedMap[0,0].ToString() + ", " + processedMap[0,1].ToString() + "\n"; 
            // calculate cell size based on the size of the room and the map size
            xCellSize = ((roomExtents[1]-roomExtents[0])/mapSize[0]);
            zCellSize = ((roomExtents[3]-roomExtents[2])/mapSize[1]); 
            // debugText+= "Calculated cell sizes: " + xCellSize.ToString() + " and " + zCellSize.ToString() + "\n"; 
            // variable to count points above the floor in each mesh
            int pointCount; 

            // debugText+= "Entering meshes loop, floor level is: " + floorLevel.ToString()+ " and ceiling level: " + ceilingLevel.ToString() + "\n"; 

            foreach (SpatialAwarenessMeshObject m in meshes.Values){
                pointCount = 0; 
                foreach( Vector3 meshVertex in m.Filter.mesh.vertices) {
                    // calculate the world position of the vertex (the basic is given with reference to the mesh)
                    Vector3 vertexWorldPosition = m.GameObject.transform.TransformPoint(meshVertex); 
                    if (vertexWorldPosition.y > floorLevel + floorLevelThreshold && vertexWorldPosition.y < ceilingLevel - floorLevelThreshold){
                        pointCount++; 
                        int[] cellIndex = FindCell(vertexWorldPosition); 
                        processedMap[cellIndex[0], cellIndex[1]] = 1;
                    } 
                }
                // debugText += "Processed mesh number " + counter + " with points above ground: " + pointCount + "\n"; 
            }
        lastProcessedMap = processedMap;
        MapCleanupBlocks();
        ExpandObstacles(0.1f);
        debugTextMesh.text += debugText;
        return processedMap; 
        

    }
    public int[] FindCell(Vector3 coordinates){
        
        int[] cellIndex = new int[2]; 
        Vector3 roomRefCoords = WorldToRoomPosition(coordinates); 
        cellIndex[0] =  (int) Math.Ceiling(roomRefCoords.x/xCellSize)-1 ; 
        cellIndex[1] =  (int) Math.Ceiling(roomRefCoords.z/zCellSize)-1  ; 
        if (cellIndex[0]==-1){
            cellIndex[0] = 0; 
        }
        if (cellIndex[1]==-1){
            cellIndex[1] = 0; 
        }
        cellIndex[1] = (int)mapSize[1] - 1 - cellIndex[1]; 
        return cellIndex; 
    
    }

    public Vector3 FindWorldPosition(int xCellIndex, int zCellIndex){

        float y;
        if (lastProcessedMap[xCellIndex,zCellIndex]== 0){ 
            y = floorLevel + 0.1f;
        }
        else{
            y = floorLevel + 0.8f;
        }
         
        return new Vector3(roomExtents[0] + xCellIndex * xCellSize + xCellSize/2,
                             y,
                             roomExtents[3] - zCellIndex*zCellSize- zCellSize/2);
    }

    public Vector3 WorldToRoomPosition(Vector3 coordinates){
        return new Vector3(coordinates.x - roomExtents[0], coordinates.y, coordinates.z - roomExtents[2]); 
    }

    private void FindRoomBounds(IReadOnlyDictionary<int,SpatialAwarenessMeshObject> meshes){
        debugText+= "In findRoomBounds\n"; 
        float minLevel = 0f, maxLevel = 0f;  

        foreach (SpatialAwarenessMeshObject m in meshes.Values){
                if (m.Filter.mesh.bounds.center.y < minLevel){
                    minLevel = m.Filter.mesh.bounds.center.y; 
                }
                if (m.Filter.mesh.bounds.center.y > maxLevel){
                    maxLevel = m.Filter.mesh.bounds.center.y; 
                }
                var mBounds = m.Filter.mesh.bounds; 
                if (mBounds.min.x < roomExtents[0]) roomExtents[0] = mBounds.min.x; 
                if (mBounds.min.z < roomExtents[2]) roomExtents[2] = mBounds.min.z; 
                if (mBounds.max.x > roomExtents[1]) roomExtents[1] = mBounds.max.x; 
                if (mBounds.max.z > roomExtents[3]) roomExtents[3] = mBounds.max.z; 
                }
            
        floorLevel = minLevel; ceilingLevel = maxLevel; 
        }

    public String PrintMap(){ 
        if (lastProcessedMap is null){
            return "";
        }
        String s= ""; 
        for (int xCellIndex=0; xCellIndex < mapSize[0]; xCellIndex++){
                for (int zCellIndex = 0; zCellIndex < mapSize[1]; zCellIndex++){
                    s += lastProcessedMap[xCellIndex,zCellIndex].ToString(); 
                }
                s+= "\n"; 
            }
        // debugText+= "Printing map\n"; 
        return s;  
        
    }

    public void IncreaseThreshold(){
        
        floorLevelThreshold += 0.05f; 
        debugText= "Changed threshold to:" + floorLevelThreshold; 
    }

    public void DecreaseThreshold(){
        floorLevelThreshold -= 0.05f; 
        debugText= "Changed threshold to:" + floorLevelThreshold; 
    }

    public void Reset(){

        lastProcessedMap = null; 
        floorLevelThreshold = 0.2f; 
        roomExtents.SetValue(0f,0); roomExtents.SetValue(0f,1); roomExtents.SetValue(0f,2); roomExtents.SetValue(0f,3);
        mapText = "" ; 
        counter = 0; 
        mapSize = new int[]{50,50}; 
        debugText = "Cleared MapBuilder data";

    }
        
    public int[,] GetMap(){
        return lastProcessedMap;
    }
    public void SetMapSize(int xSize, int zSize){
        this.mapSize[0] = xSize;
        this.mapSize[1] = zSize; 
    }

    public void SetFloorLevelThreshold(float threshold){
        this.floorLevelThreshold = threshold;
    }

    private void MapCleanup(){
        if (lastProcessedMap is null) return;
        float neighbourMajority = 0.7f;
        int xDistance = (int) (0.05* mapSize[0]), zDistance = (int) (0.05*mapSize[1]);
        List<int> neighbourValues; 
        List<int[]> cellsToChange = new List<int[]>();
        // if there's a point and the ones around it have a different value 
        for (int xCellIndex=0; xCellIndex < mapSize[0]; xCellIndex++){
            for (int zCellIndex = 0; zCellIndex < mapSize[1]; zCellIndex ++){
                neighbourValues = FindNeighboursValues(new int[] {xCellIndex,zCellIndex}, xDistance,zDistance); 
                if (neighbourValues.Sum() > neighbourMajority * neighbourValues.Count && lastProcessedMap[xCellIndex,zCellIndex] == 0 || 
                    neighbourValues.Sum() < (1-neighbourMajority)*neighbourValues.Count && lastProcessedMap[xCellIndex,zCellIndex] == 1) {
                        cellsToChange.Add(new int[] {xCellIndex,zCellIndex});
                }
            }
        }
        foreach (int[] cell in cellsToChange){
            lastProcessedMap[cell[0],cell[1]] = 1 - lastProcessedMap[cell[0],cell[1]];
        }

    }

    private List<int> FindNeighboursValues(int [] cell, int xDistance, int zDistance){
        List<int> neighbourList = new List<int>();
        Debug.Log(String.Format("Map upper bounds are {0} and {1}", lastProcessedMap.GetUpperBound(0),lastProcessedMap.GetUpperBound(1)));
        for (int x = Math.Max(0,cell[0]- xDistance); x <= Math.Min(lastProcessedMap.GetUpperBound(0), cell[0] + xDistance); x++){
            for (int z = Math.Max(0,cell[1]- zDistance); z <= Math.Min(lastProcessedMap.GetUpperBound(1), cell[1] + zDistance); z++){
                Debug.Log("Getting neighbour with index: " + x + ", " + z );
               
                if (x == cell[0] && z == cell[1]) continue;
                neighbourList.Add(lastProcessedMap[x,z]); 
            }
        }
        return neighbourList;
    }
    private List<int[]> FindNeighbours(int [] cell, int xDistance, int zDistance){
        List<int[]> neighbourList = new List<int[]>();
        
        // Debug.Log(String.Format("Map upper bounds are {0} and {1}", lastProcessedMap.GetUpperBound(0),lastProcessedMap.GetUpperBound(1)));
        for (int x = Math.Max(0,cell[0]-xDistance); x <= Math.Min(lastProcessedMap.GetUpperBound(0), cell[0] + xDistance); x++){
            for (int z = Math.Max(0,cell[1]-zDistance); z <= Math.Min(lastProcessedMap.GetUpperBound(1), cell[1] + zDistance); z++){
                
                if (x == cell[0] && z == cell[1]) continue;
                // Debug.Log("Getting neighbour with index: " + x + ", " + z );
                neighbourList.Add(new int[] {x,z}); 
            }
        }
        return neighbourList;
    }
    public void ExpandObstacles(float safetyDistance){
        if (lastProcessedMap is null) return;
        debugTextMesh.text += String.Format("In expand obstacles\n");
        int xDistance = (int) Math.Ceiling(safetyDistance/xCellSize), zDistance = (int) Math.Ceiling(safetyDistance/zCellSize);
        debugTextMesh.text += String.Format("Cells distance on x: {0} and on z: {1}\n", xDistance, zDistance);
        List<int[]> cellsToChange = new List<int[]>();
        List<int[]> xNeighbours, zNeighbours; 
        // if there's a point and the ones around it have a different value 
        // debugTextMesh.text += String.Format("Entering loop");
        for (int xCellIndex=0; xCellIndex < mapSize[0]; xCellIndex++){
            for (int zCellIndex = 0; zCellIndex < mapSize[1]; zCellIndex ++){
                if (lastProcessedMap[xCellIndex,zCellIndex] == 1){
                    Debug.Log(String.Format("Checking for cell {0},{1}",xCellIndex,zCellIndex));
                    xNeighbours = FindNeighbours(new int[] {xCellIndex,zCellIndex}, xDistance,0); 
                    zNeighbours = FindNeighbours(new int[] {xCellIndex,zCellIndex}, 0, zDistance);
                    foreach (int[] xNeighbour in xNeighbours){
                        if (lastProcessedMap[xNeighbour[0], xNeighbour[1]]==0){
                            cellsToChange.Add(xNeighbour);
                            Debug.Log(String.Format("Adding to change cell {0},{1}", xNeighbour[0], xNeighbour[1]));
                        } 
                    }
                    foreach (int[] zNeighbour in zNeighbours){
                        if (lastProcessedMap[zNeighbour[0], zNeighbour[1]]==0){
                            cellsToChange.Add(zNeighbour);
                            Debug.Log(String.Format("Adding to change cell {0},{1}", zNeighbour[0], zNeighbour[1]));
                        } 
                    }


                }  
            }
        }
        debugTextMesh.text += String.Format("Number of cells to change {0}\n",cellsToChange.Count);
        foreach (int[] cell in cellsToChange){
            lastProcessedMap[cell[0],cell[1]] = 1 ;
        }
    }

    public float GetCellSize(String dimension){
        if (dimension == "x") return xCellSize; 
        if (dimension == "z") return zCellSize; 
        else return 0.0f;
    }

    private void MapCleanupBlocks(){
        if (lastProcessedMap is null) return;
        List<int[]> cellsToChange = new List<int[]>();
        int blockSum = 0;

        for (int ix = 0; ix < mapSize[0]; ix += (int) blockSize/2){
            for(int iz = 0; iz < mapSize[1]; iz += (int) blockSize/2){
                    Debug.Log(String.Format("In block starting from: {0},{1}", ix,iz)); 
                    // check the majority in the block
                    blockSum = BlockEdgeSum(ix,iz,blockSize, coreSize); 
                    Debug.Log(String.Format("Block sum is", blockSum)); 
                    if (blockSum > neighbourMajority * blockSize * blockSize){
                        Debug.Log(String.Format("Changing core to 1"));
                        ChangeCoreValues(ix, iz, blockSize, coreSize, 1);
                    }
                    if (blockSum < (1 - neighbourMajority) * blockSize * blockSize){
                        Debug.Log(String.Format("Changing core to 0"));
                        ChangeCoreValues(ix,iz,blockSize,coreSize,0);
                    }

            }
        } 
    }

    private int BlockSum(int ixStart, int izStart, int blockSize, int coreSize = 0){

        int sum = 0;
        for (int x = ixStart; x <= Math.Min(lastProcessedMap.GetUpperBound(0), ixStart + blockSize); x++){
            for (int z = izStart; z <= Math.Min(lastProcessedMap.GetUpperBound(1), izStart + blockSize); z++){
                sum += lastProcessedMap[x,z]; 
            }
        }
        return sum;
    }

    private int BlockEdgeSum(int ixStart, int izStart, int blockSize, int coreSize){

        int sum = 0;
        for (int x = ixStart; x <= Math.Min(lastProcessedMap.GetUpperBound(0), ixStart + blockSize); x++){
            for (int z = izStart; z <= Math.Min(lastProcessedMap.GetUpperBound(1), izStart + blockSize); z++){
                if (x >= ixStart + (blockSize - coreSize)/2 &&
                    x<=ixStart + blockSize - (blockSize - coreSize)/2 &&
                    z >= izStart + (blockSize - coreSize)/2 && 
                    z<=izStart + blockSize - (blockSize - coreSize)/2) {
                        continue;
                    }
                sum += lastProcessedMap[x,z]; 
            }
        }
        return sum;
    }

    private void ChangeCoreValues(int ixStart, int izStart, int blockSize, int coreSize, int newValue){
        Debug.Log(String.Format("In change core values, start cell is {0},{1}", ixStart + (blockSize - coreSize)/2, izStart + (blockSize - coreSize)/2));
        for (int x = ixStart + (blockSize - coreSize)/2; x <= Math.Min(lastProcessedMap.GetUpperBound(0), ixStart + blockSize - (blockSize - coreSize)/2); x++){
            for (int z = izStart + (blockSize - coreSize)/2; z <= Math.Min(lastProcessedMap.GetUpperBound(1), izStart + blockSize - (blockSize - coreSize)/2); z++){
                lastProcessedMap[x,z] = newValue; 
            }
        }
        Debug.Log(String.Format("In change core values, end cell is {0},{1}", ixStart + blockSize - (blockSize - coreSize)/2
                                                                            , izStart + blockSize - (blockSize - coreSize)/2));
    }

      public float[,] PositionPath(int[,] path){

        float [,] positionPath = new  float [2, path.GetUpperBound(1) + 1]; 
        for (int cellIndex = 0; cellIndex <= path.GetUpperBound(1); cellIndex++){
            positionPath[0,cellIndex] = FindWorldPosition(path[0,cellIndex],path[1,cellIndex]).x;
            positionPath[1,cellIndex] = FindWorldPosition(path[0,cellIndex],path[1,cellIndex]).z;
        }

        return positionPath;
    }

    public bool IsOnFloor(Vector3 position){

        return position.y <= floorLevel + floorLevelThreshold*2;

    }
  



}
