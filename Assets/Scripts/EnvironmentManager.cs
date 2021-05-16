using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EasyGraph;

public class EnvironmentManager : MonoBehaviour
{
    #region Fields and properties

    VoxelGrid _voxelGrid;
    int _randomSeed = 666;

    bool _showVoids = true;

    Texture2D _inputImage;
    List<GraphVoxel> _targets = new List<GraphVoxel>();
    List<GraphVoxel> _pathVoxel = new List<GraphVoxel>();
    List<GraphVoxel> _path;

    #endregion

    #region Unity Standard Methods

    void Start()
    {
        // Initialise the voxel grid
        //Vector3Int gridSize = new Vector3Int(25, 10, 25);
        //_voxelGrid = new VoxelGrid(gridSize, Vector3.zero, 1, parent: this.transform);

        //Initialise the voxel grid from image
        _inputImage = Resources.Load<Texture2D>("Data/map2");


        _voxelGrid = new VoxelGrid(_inputImage, Vector3.zero, 1, 1, parent: this.transform);
        _path = new List<GraphVoxel>();

      


        // Set the random engine's seed
        Random.InitState(_randomSeed);
    }

    void Update()
    {
        // Draw the voxels according to their Function Colors
        DrawVoxels();

        // Use the V key to switch between showing voids
        if (Input.GetKeyDown(KeyCode.V))
        {
            _showVoids = !_showVoids;
        }

        ////drawing white pixel
        //if (Input.GetMouseButton(0))
        //{
        //    var initialVoxel = StartVoxel();

        //    if (initialVoxel != null)
        //    {
        //        //print(initialVoxel.Index);
        //        print(initialVoxel.Qname);
        //        _voxelGrid.GrowPlot(initialVoxel.Index,1);

        //    }
        //}

        if (Input.GetMouseButton(0))
        {
            SetClickedAsTarget();
        }

        //Expand path
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _voxelGrid.GrowPlot(_path, 5);

            Debug.Log(_path.Count);

        }

        //clear the gird
        if (Input.GetKeyDown(KeyCode.R))
        {
            _voxelGrid.ClearGrid();
        }
    }

    #endregion

    #region Private Methods


    /// <summary>
    /// Draws the voxels according to it's state and Function Corlor
    /// </summary>
    void DrawVoxels()
    {
        foreach (var voxel in _voxelGrid.Voxels)
        {
            if (voxel.IsActive)
            {
                Vector3 pos = (Vector3)voxel.Index * _voxelGrid.VoxelSize + transform.position;

                if (voxel.FColor == FunctionColor.Red) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.red);
                else if (voxel.FColor == FunctionColor.Yellow) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.yellow);
                else if (voxel.FColor == FunctionColor.Green) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.green);
                else if (voxel.FColor == FunctionColor.Cyan) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.cyan);
                else if (voxel.FColor == FunctionColor.Magenta) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.magenta);
                else if (voxel.FColor == FunctionColor.Blue) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.blue);
                else if (voxel.FColor == FunctionColor.White) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.white);
                else if (voxel.FColor == FunctionColor.Gray) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.gray);
                else if (_showVoids && voxel.Index.y == 0)
                    Drawing.DrawTransparentCube(pos, _voxelGrid.VoxelSize);
            }
        }
    }

    void GetQuality()
    {
        foreach (var voxel in _voxelGrid.Voxels)
        {
            if (voxel.IsActive)
            {
                //get the quality name
                print(voxel.Qname);

                Vector3 position = (Vector3)voxel.Index * _voxelGrid.VoxelSize + transform.position;

                if (voxel.FColor == FunctionColor.Red) voxel.Qname = ColorQuality.House;
                else if (voxel.FColor == FunctionColor.Yellow) voxel.Qname = ColorQuality.Street;
                else if (voxel.FColor == FunctionColor.Blue) voxel.Qname = ColorQuality.Backyard;
                else if (voxel.FColor == FunctionColor.Magenta) voxel.Qname = ColorQuality.Frontyard;
                else if (voxel.FColor == FunctionColor.Green) voxel.Qname = ColorQuality.Tree;
                else if (voxel.FColor == FunctionColor.Cyan) voxel.Qname = ColorQuality.LandTexture;
                else if (voxel.FColor == FunctionColor.White) voxel.Qname = ColorQuality.Plot;
                else if (_showVoids && voxel.Index.y == 0) voxel.Qname = ColorQuality.EmptyLand;


            }
        }
    }

    #endregion

    #region Public Method

    private void SetClickedAsTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //check if mouse hit
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //transform ths hitting object to gameoject
            Transform objectHit = hit.transform;

            //if the hit voxel is backyard voxel
            //II or condition to change state between is target or not || objectHit.gameObject.layer == LayerMask.NameToLayer("Plot")
            if (objectHit.gameObject.layer == LayerMask.NameToLayer("Backyard"))

            {
                //get its name(index)

                GraphVoxel selected = null;
                string voxelName = objectHit.name;
                var index = voxelName.Split('_').Select(v => int.Parse(v)).ToArray();

                //reture its index
                selected = (GraphVoxel)_voxelGrid.Voxels[index[0], index[1], index[2]];

                //string[] voxelName = objectHit.name.Split('_');

                //int x = int.Parse(voxelName[1]);
                //int y = int.Parse(voxelName[2]);
                //int z = int.Parse(voxelName[3]);

                //Vector3Int ind = new Vector3Int(x, y, z);

                //GraphVoxel voxel = (GraphVoxel)_voxelGrid.Voxels[ind.x, ind.y, ind.z];

                selected.SetAsTarget();

                //add selected target voxel to the list
                if (selected.IsTarget)
                {
                    _targets.Add(selected);

                }
                else
                {
                    _targets.Remove(selected);
                }

            }
        }
    }

    //UI Bottom
    public void VoxeliseImage()
    {
        _voxelGrid.SetStatesFromImage(_inputImage);
    }



    public void CreatePaths()
    {
        Queue<GraphVoxel> targetPool = new Queue<GraphVoxel>(_targets);
        var edges = _voxelGrid.GetEdgesByTypes(FunctionColor.Blue, FunctionColor.White);
        Debug.Log(edges.Count);

        UndirecteGraph<GraphVoxel, Edge<GraphVoxel>> graph = new UndirecteGraph<GraphVoxel, Edge<GraphVoxel>>(edges);
        Dijkstra<GraphVoxel, Edge<GraphVoxel>> dijkstra = new Dijkstra<GraphVoxel, Edge<GraphVoxel>>(graph);
        _path.AddRange(dijkstra.GetShortestPath(targetPool.Dequeue(), targetPool.Dequeue()));


        while (targetPool.Count > 0)
        {
            GraphVoxel nextVoxel = targetPool.Dequeue();

            SetNextShortestPath(nextVoxel, dijkstra);         

        }
        Debug.Log(_path.Count);
        foreach (var voxel in _path)
        {
            voxel.FColor = FunctionColor.White;           
        }

    }


    void SetNextShortestPath(GraphVoxel targetVoxel, Dijkstra<GraphVoxel, Edge<GraphVoxel>> dijkstra)
    {
        dijkstra.DijkstraCalculateWeights(targetVoxel);
        GraphVoxel closestVoxel = _path.MinBy(v => dijkstra.VertexWeight(v));
        List<GraphVoxel> newpath = new List<GraphVoxel>();

        newpath.AddRange(dijkstra.GetShortestPath(targetVoxel, closestVoxel));
        newpath.Remove(closestVoxel);
        _path.AddRange(newpath);

    }


    //public void GrowPath(List<GraphVoxel> path, int radius)
    //{

    //    path = new List<GraphVoxel>();
    //    FunctionColor fcolor = FunctionColor.White;

    //    for (int i = 0; i < radius; i++)
    //    {
    //        List<Voxel> newVoxels = new List<Voxel>();

    //        foreach (var voxel in path)
    //        {
    //            Voxel[] neighbours;
    //            neighbours = voxel.GetFaceNeighbours().ToArray();

    //            foreach (var neighbour in neighbours)
    //            {
    //                if (neighbour.IsActive && neighbour.FColor == FunctionColor.Blue && !path.Contains(neighbour) && !newVoxel.Contains(neighbour))
    //                {
    //                    newVoxels.Add(neighbour);
    //                }
    //            }


    //        }
    //        if (newVoxels.Count == 0) break;


    //    }


    //}

}












#endregion

