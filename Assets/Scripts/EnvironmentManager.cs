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
    public int _randomPercent = 50;

    bool _showVoids = true;

    Texture2D _inputImage;
    List<GVoxel> _targets = new List<GVoxel>();
    List<GVoxel> _pathVoxel = new List<GVoxel>();
    List<GVoxel> _path;
    public Slider slider;
    

    public int radius;
    public float Maxdistance;
    public float Mindistance;


    List<GVoxel> _originalPath;

    #endregion

    #region Unity Standard Methods

    void Start()
    {
        // Initialise the voxel grid
        //Vector3Int gridSize = new Vector3Int(25, 10, 25);
        //_voxelGrid = new VoxelGrid(gridSize, Vector3.zero, 1, parent: this.transform);

        //Initialise the voxel grid from image
        _inputImage = Resources.Load<Texture2D>("Data/map10");


        _voxelGrid = new VoxelGrid(_inputImage, Vector3.zero, 1, 1, parent: this.transform);
        _path = new List<GVoxel>();


        // Set the random engine's seed
        Random.InitState(_randomPercent);
       
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


        if (Input.GetMouseButton(0))
        {
            SetClickedAsTarget();
        }

        //Analyse sunlight
        if (Input.GetKeyDown(KeyCode.L))
        {
            OcclusionControl(18.0f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            AccessibilityControl(18.8f,10);
        }

        //Expand path


        //clear the gird
        if (Input.GetKeyDown(KeyCode.V))
        {
            VoxeliseImage();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreatePaths();

        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _voxelGrid.ClearGrid();

        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            //SetRandomAliveVoxels(1);
            SetRandomVoxels(3);
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
                else if (voxel.FColor == FunctionColor.Orange) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.black);
                else if (voxel.FColor == FunctionColor.Yellow) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.yellow);
                else if (voxel.FColor == FunctionColor.Green) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.green);
                else if (voxel.FColor == FunctionColor.Cyan) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.cyan);
                else if (voxel.FColor == FunctionColor.Magenta) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.magenta);
                else if (voxel.FColor == FunctionColor.Blue) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.blue);
                else if (voxel.FColor == FunctionColor.White) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.white);
                
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

    //UI Bottom
    public void VoxeliseImage()
    {
        _voxelGrid.SetStatesFromImage(_inputImage);
    }

    #region start voxel

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

    public void SetRandomVoxels(int amount)
    {
        int x = Random.Range(0, _voxelGrid.GridSize.x);
        int z = Random.Range(0, _voxelGrid.GridSize.z);

        List<GVoxel> RanVoxel = new List<GVoxel>();

        for (int i = 0; i < amount; i++)
        {
            Vector3Int idx = new Vector3Int(x, 0, z);

            if (Util.ValidateIndex(_voxelGrid.GridSize, idx))
            {
                var voxel = _voxelGrid.Voxels[x, 0, z];

                if (voxel.IsActive && voxel.FColor == FunctionColor.Blue)
                {
                    RanVoxel.Add((GVoxel)voxel);

                }
            }
            //var RanVoxels= GetVoxels().Where(v => v.IsActive && v.FColor == FunctionColor.Blue);
        }
        foreach (GVoxel ranV in RanVoxel)
        {
            //ranV.FColor = FunctionColor.White;
            //ranV.IsAlive = true;
            ranV.SetState(1);
            ranV.SetAsTarget();

            if (ranV.IsTarget)
            {
                _targets.Add(ranV);

            }
            else
            {
                _targets.Remove(ranV);
            }

        }

    }

    #endregion

    #region Plot growing and area adjusting method

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

    #endregion

    public void OcclusionControl(float OcDis)
    {
        var PlotVoxels2 = _voxelGrid.GetVoxels().Where(v => v.IsActive && v.VoxelCollider.tag == "PlotVoxel");


        foreach (GVoxel plotVV in PlotVoxels2)
        {
            plotVV.RaycastSunScore();

            float[] allScore = { plotVV.LightScore };

            float Min = Mathf.Min(allScore);

            if (plotVV.LightScore > /*Min + */ OcDis)
            {
                plotVV.SetState(1);

            }
            else
            {
                plotVV.SetState(0);
                plotVV.FColor = FunctionColor.Blue;
            }
            Debug.Log($"Score is {plotVV.LightScore}");
            Debug.Log($"Min is {Min}");

        }
    }

    public void AdjustOcclusion()
    {
        Maxdistance = (float)slider.value;
        
        OcclusionControl(Maxdistance);

    }

    public void AccessibilityControl(float Maxdis, int PercentageReduce)
    {
        

        var PlotVoxels = _voxelGrid.GetVoxels().Where(v => v.IsActive && v.VoxelCollider.tag == "PlotVoxel");

        int numReduce = PlotVoxels.Count() * PercentageReduce / 100;

        foreach (GVoxel plotV in PlotVoxels)
        {
            plotV.RaycastSunScore();

            if (plotV.LightScore > Maxdis)
            {
                plotV.SetState(0);
                plotV.FColor = FunctionColor.Blue;
            }

            //Debug.Log($"Score is {plotV.LightScore}");
        }

    }

    public void AdjustAccessibility()
    {
        Maxdistance = (float)slider.value;
        AccessibilityControl(Maxdistance,10);

    }

    #endregion






}





