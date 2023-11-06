using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;

public class MainController : MonoBehaviour
{
    private MapBuilder mapBuilder;
    private IMixedRealitySpatialAwarenessMeshObserver observer; 
    public Material floorMarkerMaterial, obstacleMarkerMaterial, pathMarkerMaterial;
    private bool mapCreated, mapVisualised, pathVisualised, isScanning, robotRunning;
    public Vector3 startLocation, endLocation;
    private List<GameObject> mapMarkers, pathMarkers;
    int [,] lastProcessedMap;
    int[,] lastFoundPath;
    int cellsPerMarker = 1;
    int mapSize = 50;
    // String debugText = "";
    public TextMesh paramsText, debugText, robotText, step1debugText;
    public GameObject endLocationMarker, robotBody; 
    private Vector3 lastMarkerPosition;
    private int pathCounter = 0;


    private void Start(){
        mapBuilder = new MapBuilder(debugTextMesh: debugText, expandObstacles: false);
        startLocation = new Vector3(0f,0.1f,0f);
        endLocation = new Vector3(0f,0.1f,1f); 
        pathVisualised = false;
        mapVisualised = false;
        robotRunning = false;
        mapCreated = false;
        lastProcessedMap = null;
        lastFoundPath = null;
        mapMarkers = new List<GameObject>();
        pathMarkers = new List<GameObject>();
        isScanning = false; 
        observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>(); 
        observer.Suspend(); 
        PrintRunParameters();
        debugText.text = "Started main controller\n"; 
    }

    public void StopStartScanning(){
        if (isScanning){
            observer.Suspend();
            isScanning = false; 
            debugText.text += "Stopped scanning\n";
        }
        else{
            observer.Resume();
            isScanning = true; 
            debugText.text += "Started scanning\n";
        }
    }

    public void BuildMap(){
        if (isScanning) return;
        debugText.text += "In BuildMap\n";
        lastProcessedMap = mapBuilder.BuildRoomMap(observer.Meshes);
        mapCreated = true;
        debugText.text += String.Format("Map built, the extents are {0},{1}\n", lastProcessedMap.GetUpperBound(0)+1, lastProcessedMap.GetUpperBound(1)+1);
        debugText.text += String.Format("Map cell sizes are: {0} and {1}\n", mapBuilder.GetCellSize("x"), mapBuilder.GetCellSize("z"));
        // debugText.text += mapBuilder.PrintMap() + "\n";
 
        
    }

    public void StopStartScanBuildMap(){

        if (isScanning){
            observer.Suspend();
            isScanning = false; 
            debugText.text += "Stopped scanning, entering build map\n";
            BuildMap();
            visualiseMap();
        }
        else{
            observer.Resume();
            isScanning = true; 
            ResetMap();
            ResetPath();
            debugText.text += "Started scanning\n";
        }
    }

    public void FindPath(){
        if (pathCounter ==0) {
            debugText.text += "In FindPath\n";
        }
        if (!mapCreated) {
            debugText.text += "Map not created, can't find path\n";
            return;
        }
        AStar astar = new AStar(lastProcessedMap);
        startLocation = robotBody.transform.position;
        endLocation = endLocationMarker.transform.position; 
        int[] startCell = mapBuilder.FindCell(startLocation);
        int[] endCell = mapBuilder.FindCell(endLocation);
        if (pathCounter ==0) debugText.text += String.Format("Start cell is {0},{1} and end cell is {2},{3}\n", startCell[0],startCell[1],endCell[0],endCell[1]);
        lastFoundPath = astar.FindPath(startCell[0],startCell[1],endCell[0],endCell[1], debugText); 
        if (pathCounter ==0) {
            debugText.text += String.Format("Path found, path length is {0}\n", lastFoundPath.GetUpperBound(1)+1); 
        }
        
    }

    public void FindShowPath(){
        FindPath();
        VisualisePath();
    }

    public void visualiseMap(){ 

        debugText.text += "In visualiseMap\n";
        if (mapVisualised) {
            debugText.text += "Map already visualised, exiting function.\n";
            return;
        }
        if (lastProcessedMap is null || !mapCreated) {
            debugText.text += "There is no map to visualise\n"; 
            return;
        } 
        GameObject newMarker; 

        for (int xCellIndex=0; xCellIndex <= lastProcessedMap.GetUpperBound(0); xCellIndex+= cellsPerMarker){
                for (int zCellIndex = 0; zCellIndex <= lastProcessedMap.GetUpperBound(1); zCellIndex += cellsPerMarker){
                      if (lastProcessedMap[xCellIndex,zCellIndex]== 0)
                        { 
                            newMarker = placeMarker(position: mapBuilder.FindWorldPosition(xCellIndex,zCellIndex), localScale: new Vector3(0.02f, 0.02f, 0.02f), material:floorMarkerMaterial);
                        }
                        else{
                            newMarker = placeMarker(position: mapBuilder.FindWorldPosition(xCellIndex,zCellIndex), localScale: new Vector3(0.02f, 0.02f, 0.02f), material:obstacleMarkerMaterial);
                        }
                    mapMarkers.Add(newMarker); 
                }
            }

        mapVisualised = true;
    }

    public void VisualisePath(){
        if (pathCounter==0) debugText.text += "In visualise path\n";
        if (pathVisualised){
            DestroyMarkers("path");
            if (pathCounter==0) debugText.text += String.Format("Path was already visualised, cleared  markers.\n"); 
            pathVisualised = false;
        }
        int lastIndex = lastFoundPath.GetUpperBound(1);
        // for (int i = 0; i <= lastIndex; i++){
        //     debugText.text += String.Format("C{0}: {1},{2}; ", i, lastFoundPath[0,i],lastFoundPath[1,i]); 
        // }
        // debugText.text += "\n";
        // mark start of path
        GameObject startMarker = placeMarker(position: mapBuilder.FindWorldPosition(lastFoundPath[0,0],lastFoundPath[1,0]),
                                             localScale:new Vector3(0.1f,0.1f, 0.1f), 
                                             material:pathMarkerMaterial); 
        if (pathCounter==0) debugText.text += "Added start marker in position: "+ startMarker.transform.position +" \n"; 
        pathMarkers.Add(startMarker); 
        // mark end of path
        GameObject endMarker = placeMarker(position: mapBuilder.FindWorldPosition(lastFoundPath[0,lastIndex], lastFoundPath[1,lastIndex]),
                                            localScale:new Vector3(0.2f,0.2f, 0.2f),
                                            material:pathMarkerMaterial); 
        if (pathCounter==0) debugText.text += "Added end marker in position: "+ endMarker.transform.position +" \n"; 
        pathMarkers.Add(endMarker); 
        
        // mark all the other points on the path
        GameObject pathMarker;
        for (int cellIndex = 1; cellIndex < lastIndex; cellIndex++){
            pathMarker = placeMarker(position: mapBuilder.FindWorldPosition(lastFoundPath[0,cellIndex], lastFoundPath[1,cellIndex]),
                                    localScale:new Vector3(0.03f,0.03f, 0.03f),
                                    material:pathMarkerMaterial); 
            // debugText += "Added path marker in position: "+ endMarker.transform.position +" \n"; 
            pathMarkers.Add(pathMarker); 
        }
        pathVisualised = true;
        if (pathCounter==0) debugText.text  += "Visualised path.\n";

    }

    private GameObject placeMarker(Vector3 position, Vector3 localScale, Material material){

            GameObject newMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newMarker.transform.position = position; 
            newMarker.transform.localScale = localScale; 
            Renderer markerRenderer = newMarker.GetComponent<Renderer>(); 
            markerRenderer.material = material; 
            newMarker.GetComponent<BoxCollider>().enabled = false;
            return newMarker;

    }

    public void ResetMap(){
        debugText.text += "In ResetMap\n";
        lastProcessedMap = null;
        DestroyMarkers("map");
        mapBuilder.Reset(); 
        debugText.text += "Reset map builder\n";
        UpdateMapParams();
        debugText.text += "Updated room params\n";
        ResetPath();
        mapCreated = false;
        
    }

    public void ResetPath(){

        debugText.text += "In ResetPath\n";
        lastFoundPath = null;
        DestroyMarkers("path");

    }

    public void DestroyMarkers(String choice){
        debugText.text += "In destroy markers\n"; 
        List<GameObject> visualMarkers ; 
        if (choice == "map") {
            visualMarkers = mapMarkers;
            mapVisualised = false;
        }
        else {
            visualMarkers = pathMarkers;
            pathVisualised = false;
        }
        debugText.text += String.Format("Visual markers list for {0} contains {1} elements.\n", choice, visualMarkers.Count);
        foreach(GameObject marker in visualMarkers){
            Destroy(marker);
        }
        visualMarkers.Clear();
        debugText.text += String.Format("Destroyed markers for {0}\n", choice);
        
    }

    public void UpdateMapParams(){
        mapBuilder.SetMapSize(mapSize, mapSize);
        PrintRunParameters();
    }

    public void IncreaseMapSize(){
        mapSize += 10; 
        UpdateMapParams();
    }

    public void DecreaseMapSize(){
        mapSize -= 10; 
        UpdateMapParams();
    }

    public void IncreaseFloorThreshold(){
        mapBuilder.IncreaseThreshold();
        
    }
    public void DecreaseFloorThreshold(){
        mapBuilder.DecreaseThreshold();
        PrintRunParameters();
    }

    public void PrintRunParameters(){

        paramsText.text = "Run parameters";
        paramsText.text += String.Format("\nRoom map size: {0},{1}\nFloorLevelThreshold: {2}\nCellsPerMarker: {3}\n",
        mapBuilder.mapSize[0],mapBuilder.mapSize[1],mapBuilder.floorLevelThreshold, cellsPerMarker);
        paramsText.text += String.Format("Start location: {0}\nEnd location {1}.", startLocation.ToString(), endLocation.ToString());
        paramsText.text += String.Format("Expand obstacles: {0}.", mapBuilder.expandObstacles);

    }

    public void toggleExpand(){
        mapBuilder.expandObstacles = !mapBuilder.expandObstacles; 
        PrintRunParameters();
    }

    public void RunRobot(){
        var robotController = FindObjectOfType<PurePursuitControl>();
        if(mapCreated && pathVisualised){
            robotController.SetRobotPath(mapBuilder.PositionPath(lastFoundPath));
            debugText.text += String.Format("Set robot path to: {0} with length {1}\n", robotController.path, robotController.path.GetLength(1));
            robotController.SetRobotRunning(true); 
            
        }    }

    public void StopRobot(){
        var robotController = FindObjectOfType<PurePursuitControl>();
        robotController.StopRobot();
    }

   


}
