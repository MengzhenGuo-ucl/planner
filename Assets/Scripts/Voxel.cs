﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Voxel : IEquatable<Voxel>
{
    #region Public fields

    public Vector3Int Index;
    public List<Face> Faces = new List<Face>(6);
    public Vector3 Center => (Index + _voxelGrid.Origin) * _size;
    public bool IsActive;


    public FunctionColor FColor;

    public ColorQuality Qname;

    public GameObject VoxelCollider = null;
    public GameObject _voxelGO;

    #endregion

    #region Protected fields

    protected VoxelGrid _voxelGrid;
    protected float _size;


    #endregion

    #region Contructors

    /// <summary>
    /// Creates a regular voxel on a voxel grid
    /// </summary>
    /// <param name="index">The index of the Voxel</param>
    /// <param name="voxelgrid">The <see cref="VoxelGrid"/> this <see cref="Voxel"/> is attached to</param>
    /// <param name="voxelGameObject">The <see cref="GameObject"/> used on the Voxel</param>
    public Voxel(Vector3Int index, VoxelGrid voxelGrid, bool createCollider = false, Transform parent = null)
    {
        Index = index;
        _voxelGrid = voxelGrid;
        _size = _voxelGrid.VoxelSize;
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

        //_PlotVoxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //_PlotVoxel.transform.position = (_voxelGrid.Origin + Index) * _size;
        //_PlotVoxel.transform.localScale *= _voxelGrid.VoxelSize * 1;
        //_PlotVoxel.name = $"Voxel_{Index.x}_{Index.y}_{Index.z}";
        //_PlotVoxel.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Plot");
    }

    //public Voxel(Vector3Int index, VoxelGrid voxelGrid, float sizeFactor = 1f)
    //{
    //    Index = index;
    //    _voxelGrid = voxelGrid;
    //    _size = _voxelGrid.VoxelSize;
    //    FColor = FunctionColor.Empty;

    //    var colliderPrefab = Resources.Load<GameObject>("Prefabs/VoxelCollider");
    //    _voxelGO = GameObject.Instantiate(colliderPrefab);
    //    _voxelGO.transform.position = (_voxelGrid.Origin + Index) * _size;
    //    _voxelGO.transform.localScale *= _voxelGrid.VoxelSize * sizeFactor;
    //    _voxelGO.name = $"Voxel_{Index.x}_{Index.y}_{Index.z}";
    //    //_voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Plot");
    //}


    /// <summary>
    /// Generic constructor, alllows the use of inheritance
    /// </summary>

    public Voxel() { }

    #endregion

    #region Public methods

    /// <summary>
    /// Get the neighbouring voxels at each face, if it exists
    /// </summary>
    /// <returns>All neighbour voxels</returns>
    public IEnumerable<Voxel> GetFaceNeighbours()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != s.x - 1) yield return _voxelGrid.Voxels[x + 1, y, z];
        //if (x != 0) yield return _voxelGrid.Voxels[x - 1, y, z];

        if (y != s.y - 1) yield return _voxelGrid.Voxels[x, y + 1, z];
        if (y != 0) yield return _voxelGrid.Voxels[x, y - 1, z];

        if (z != s.z - 1) yield return _voxelGrid.Voxels[x, y, z + 1];
        //if (z != 0) yield return _voxelGrid.Voxels[x, y, z - 1];

        if (z != 0 && x != 0) yield return _voxelGrid.Voxels[x + 1, y, z + 1];
    }

    /// <summary>
    /// Get the neighbouring voxels at each face, if it exists
    /// </summary>
    /// <returns>All neighbour voxels</returns>
    public IEnumerable<Voxel> GetFaceNeighboursXZ()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != s.x - 1) yield return _voxelGrid.Voxels[x + 1, y, z];
        //if (x != 0) yield return _voxelGrid.Voxels[x - 1, y, z];

        //if (y != s.y - 1) yield return _voxelGrid.Voxels[x, y + 1, z];
        //if (y != 0) yield return _voxelGrid.Voxels[x, y - 1, z];

        if (z != s.z - 1) yield return _voxelGrid.Voxels[x, y, z + 1];
        //if (z != 0) yield return _voxelGrid.Voxels[x, y, z - 1];

        if (z != 0 && x != 0) yield return _voxelGrid.Voxels[x + 1, y, z + 1];
    }

    public Voxel[] GetFaceNeighboursArray()
    {
        Voxel[] result = new Voxel[6];

        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != s.x - 1) result[0] = _voxelGrid.Voxels[x + 1, y, z];
        else result[0] = null;

        //if (x != 0) result[1] = _voxelGrid.Voxels[x - 1, y, z];
        //else result[1] = null;

        if (y != s.y - 1) result[2] = _voxelGrid.Voxels[x, y + 1, z];
        else result[2] = null;

        if (y != 0) result[3] = _voxelGrid.Voxels[x, y - 1, z];
        else result[3] = null;

        if (z != s.z - 1) result[4] = _voxelGrid.Voxels[x, y, z + 1];
        else result[4] = null;

        if (z != 0 && x != 0) result[5] = _voxelGrid.Voxels[x + 1, y, z + 1];
        //if (z != 0) result[5] = _voxelGrid.Voxels[x, y, z - 1];
        //else result[5] = null;




        return result;
    }



    /// <summary>
    /// Activates the visibility of this voxel
    /// </summary>
    public void ActivateVoxel(bool state)
    {
        IsActive = state;
    }


    public float RaycastSunScore()
    {
        float lightscore = 0;
        float maxRayLength = 10f;
        //Make a circle with all direcitons
        List<Vector3> directions = new List<Vector3>()
        {
            new Vector3(1,0,0),
            new Vector3(.5f,.5f,0)

    };

        foreach (var direction in directions)
        {
            RaycastHit hit;
            //Make sure only the voxels that represent physical opbjects can hit (either disable colliders or put the other voxels into the ignoreraycast layer)
            if (Physics.Raycast(VoxelCollider.transform.position, direction, out hit, maxRayLength)) 
            {
                //get distance from centre of the voxel to the hit
                //add distance to lightscore
            }
            else
            {
                lightscore += maxRayLength;
            }
        }

        lightscore /= directions.Count;
        return lightscore;
    }
    #endregion

    #region Equality checks

    /// <summary>
    /// Checks if two Voxels are equal based on their Index
    /// </summary>
    /// <param name="other">The <see cref="Voxel"/> to compare with</param>
    /// <returns>True if the Voxels are equal</returns>
    public bool Equals(Voxel other)
    {
        return (other != null) && (Index == other.Index);
    }

    /// <summary>
    /// Get the HashCode of this <see cref="Voxel"/> based on its Index
    /// </summary>
    /// <returns>The HashCode as an Int</returns>
    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    #endregion
}