using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;

public class GVoxel : Voxel
{
    #region Private field

    #endregion

    #region Public field

    public int _state = 0;
    public bool IsTarget;
    public bool IsPlot;
    public float LightScore {get; private set;}


    #endregion

    #region Construct

    public GVoxel(Vector3Int index, VoxelGrid voxelGrid, int state, bool createCollider = false, Transform parent = null)
    {
       
        Index = index;
        _voxelGrid = voxelGrid;
        _size = _voxelGrid.VoxelSize;
        _state = state;
        IsActive = true;
        FColor = FunctionColor.Empty;

        if (createCollider)
        {
            var colliderPrefab = Resources.Load<GameObject>("Prefabs/VoxelCollider");
            VoxelCollider = GameObject.Instantiate(colliderPrefab, parent, true);
            VoxelCollider.transform.localPosition = new Vector3(Index.x, Index.y, Index.z) * _size;
            VoxelCollider.name = $"{Index.x}_{Index.y}_{Index.z}";
            VoxelCollider.tag = "Voxel";
            IsAlive = false;
       
        }
    }

    #endregion


    #region Private method

    #endregion

    #region Public method


    public void SetAsTarget()
    {
        IsTarget = !IsTarget;

        if (IsTarget)
        {
            FColor = FunctionColor.White;
            VoxelCollider.gameObject.layer = LayerMask.NameToLayer("Plot");

        }
        else
        {
            FColor = FunctionColor.Blue;

        }
    }

    public void SetState(int newStatus)
    {
        _state = newStatus;
        

        if (_state == 1)
        {
            //alive
            FColor = FunctionColor.Yellow;
            IsPlot = true;

            VoxelCollider.tag = "PlotVoxel";
        }
        else if(_state == 0)
        {
            //dead
            FColor = FunctionColor.Blue;
            IsPlot = false;

            VoxelCollider.tag = "Voxel";
        }

    }

    public void ChangeStatus()
    {
        
        if (IsAlive == true)
        {
            SetState(0);
        }

        else if (IsAlive == false)
        {
            SetState(1);
        }
    }

  
    public GVoxel[] GetEightNeighbours()
    {
        GVoxel[] result = new GVoxel[8];

        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != s.x - 1) result[0] = (GVoxel)_voxelGrid.Voxels[x + 1, y, z];
        else result[0] = null;

        if (x != 0) result[1] = (GVoxel)_voxelGrid.Voxels[x - 1, y, z];
        else result[1] = null;

        if (z != s.z - 1) result[2] = (GVoxel)_voxelGrid.Voxels[x, y, z + 1];
        else result[2] = null;

        if (z != 0) result[3] = (GVoxel)_voxelGrid.Voxels[x, y, z - 1];
        else result[3] = null;

        if (z != 0 && x != 0 && x != s.x - 1 && z != s.z - 1)
        {
            result[4] = (GVoxel)_voxelGrid.Voxels[x - 1, y, z - 1];
            result[5] = (GVoxel)_voxelGrid.Voxels[x - 1, y, z + 1];
            result[6] = (GVoxel)_voxelGrid.Voxels[x + 1, y, z + 1];
            result[7] = (GVoxel)_voxelGrid.Voxels[x + 1, y, z - 1];
        }
        else result[4 & 5 & 6 & 7] = null;

        return result;

    }


    public float RaycastSunScore()
    {
        LightScore = 0;
        float maxRayLength = 20f;
        //Make a circle with all direcitons
        List<Vector3> directions = new List<Vector3>()
        {
           
            // 360 degree  - 8 directions
            new Vector3(1,   0   ,0),
            new Vector3(1f,  1f  ,0),
            new Vector3(0,   1f  ,0),
            new Vector3(-1f, 1f  ,0),
            new Vector3(-1,   0   ,0),
            new Vector3(-1f,-1f ,0),
            new Vector3(0,  -1f ,0),
            new Vector3(1f, -1f,0)

    };

        foreach (var direction in directions)
        {
            RaycastHit hit;
            //int layerMask = 9;

            //Make sure only the voxels that represent physical opbjects can hit (either disable colliders or put the other voxels into the ignoreraycast layer)
            if (Physics.Raycast(VoxelCollider.transform.position, direction, out hit, maxRayLength, LayerMask.GetMask("Boundary")))
            {
                //get distance from centre of the voxel to the hit  
                var distance = hit.distance;
                Debug.Log(distance);
                //add distance to lightscore
                LightScore += distance;
            }
            else
            {
                LightScore += maxRayLength;
            }

        }

        LightScore /= directions.Count;
        return LightScore;
    }



    #endregion
}
