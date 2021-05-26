using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EasyGraph;
using UnityEngine.UI;

public class EnvironmentManager : MonoBehaviour
{
    #region Fields and properties

    VoxelGrid _voxelGrid;
    int _randomSeed = 666;

    bool _showVoids = true;

    Texture2D _inputImage;
    List<GVoxel> _targets = new List<GVoxel>();
    List<GVoxel> _pathVoxel = new List<GVoxel>();
    List<GVoxel> _path;
    public Slider slider;
    

    public int radius;
    List<GVoxel> _originalPath;

    #endregion

    #region Unity Standard Methods

    void Start()
    {
        // Initialise the voxel grid
        //Vector3Int gridSize = new Vector3Int(25, 10, 25);
        //_voxelGrid = new VoxelGrid(gridSize, Vector3.zero, 1, parent: this.transform);

        //Initialise the voxel grid from image
        _inputImage = Resources.Load<Texture2D>("Data/map3");


        _voxelGrid = new VoxelGrid(_inputImage, Vector3.zero, 1, 1, parent: this.transform);
        _path = new List<GVoxel>();

      


        // Set the random engine's seed
        Random.InitState(_randomSeed);
    }

    void Update()
    {
        //expand area

        
        // Draw the voxels according to their Function Colors
        DrawVoxels();

        // Use the V key to switch between showing voids
        if (Input.GetKeyDown(KeyCode.V))
        {
            _showVoids = !_showVoids;
        }


        if (Input.GetMouseButton(0))
        {
            SetClickedAsTarget();
        }

        //Analyse sunlight
        if (Input.GetKeyDown(KeyCode.L))
        {
            foreach (GVoxel voxel in _path)
            {
                voxel.RaycastSunScore();
                Debug.Log($"Score is {voxel.LightScore}");
            }


        }

        //Expand path
        

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

                GVoxel selected = null;
                string voxelName = objectHit.name;
                var index = voxelName.Split('_').Select(v => int.Parse(v)).ToArray();

                //reture its index
                selected = (GVoxel)_voxelGrid.Voxels[index[0], index[1], index[2]];

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
        Queue<GVoxel> targetPool = new Queue<GVoxel>(_targets);
        var edges = _voxelGrid.GetEdgesByTypes(FunctionColor.Blue, FunctionColor.White);
        Debug.Log(edges.Count);

        UndirecteGraph<GVoxel, Edge<GVoxel>> graph = new UndirecteGraph<GVoxel, Edge<GVoxel>>(edges);
        Dijkstra<GVoxel, Edge<GVoxel>> dijkstra = new Dijkstra<GVoxel, Edge<GVoxel>>(graph);
        _path.AddRange(dijkstra.GetShortestPath(targetPool.Dequeue(), targetPool.Dequeue()));


        while (targetPool.Count > 0)
        {
            GVoxel nextVoxel = targetPool.Dequeue();

            SetNextShortestPath(nextVoxel, dijkstra);         

        }
        Debug.Log(_path.Count);
        foreach (var voxel in _path)
        {
            voxel.FColor = FunctionColor.White;           
        }

        _originalPath = _path;

    }


    void SetNextShortestPath(GVoxel targetVoxel, Dijkstra<GVoxel, Edge<GVoxel>> dijkstra)
    {
        dijkstra.DijkstraCalculateWeights(targetVoxel);
        GVoxel closestVoxel = _path.MinBy(v => dijkstra.VertexWeight(v));
        List<GVoxel> newpath = new List<GVoxel>();

        newpath.AddRange(dijkstra.GetShortestPath(targetVoxel, closestVoxel));
        newpath.Remove(closestVoxel);
        _path.AddRange(newpath);

    }


    public void AdjustRadius()
    {
        radius = (int)slider.value;
        _path = _voxelGrid.GrowPlot(_originalPath, radius);
        

    }

    

}



#endregion

