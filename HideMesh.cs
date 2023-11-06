using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;
using System; 

public class HideMesh : MonoBehaviour
{


    IMixedRealitySpatialAwarenessMeshObserver observer ;
    // Start is called before the first frame update
    void Start()
    {
        observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>(); 
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
    }

    public void hideMesh()
    {
        // var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>(); 
        if (observer.DisplayOption == SpatialAwarenessMeshDisplayOptions.None){
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible; 
        } 
        else {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None; 
            Console.Write(observer.Meshes);
        }

    }

     
}
