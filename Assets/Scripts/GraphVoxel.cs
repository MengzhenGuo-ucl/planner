using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVoxel:Voxel
{
    #region Private field

    private float _state;

    #endregion

    #region Public field
    public bool IsTarget;
    #endregion

    #region Construct

    public GraphVoxel(Vector3Int index, VoxelGrid voxelGrid, float state, bool createCollider = false, Transform parent = null)
    {
        //Index = index;
        //_voxelGrid = voxelGrid;
        //_size = voxelGrid.VoxelSize;

        //_state = state;

        //_PlotVoxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //_PlotVoxel.transform.position = (_voxelGrid.Origin + Index) * _size;
        //_PlotVoxel.transform.localScale *= _voxelGrid.VoxelSize * 1;
        //_PlotVoxel.name = $"Voxel_{Index.x}_{Index.y}_{Index.z}";
        //_PlotVoxel.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Plot");

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
        }


        //}

        //public GraphVoxel(Vector3Int index, VoxelGrid voxelGrid, float state, float sizeFactor)
        //{
        //    Index = index;
        //    _voxelGrid = voxelGrid;
        //    _size = _voxelGrid.VoxelSize;
        //    FColor = FunctionColor.White;

        //    _state = state;

        //    _voxelGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    _voxelGO.transform.position = (_voxelGrid.Origin + Index) * _size;
        //    _voxelGO.transform.localScale *= _voxelGrid.VoxelSize * sizeFactor;
        //    _voxelGO.name = $"Voxel_{Index.x}_{Index.y}_{Index.z}";
        //    _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Plot");
        //}
      




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

    #endregion
}
