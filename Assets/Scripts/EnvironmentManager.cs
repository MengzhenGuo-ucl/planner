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
    List<GraphicVoxel> _targets = new List<GraphicVoxel>();
    List<GraphicVoxel> _path;


    #endregion

    #region Unity Standard Methods

    void Start()
    {
        // Initialise the voxel grid
        //Vector3Int gridSize = new Vector3Int(25, 10, 25);
        //_voxelGrid = new VoxelGrid(gridSize, Vector3.zero, 1, parent: this.transform);

        //Initialise the voxel grid from image
        _inputImage = Resources.Load<Texture2D>("Data/map1");

        
        _voxelGrid = new VoxelGrid(_inputImage,Vector3.zero,1, 1, parent: this.transform);
        _path = new List<GraphicVoxel>();



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
        //create random plots
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //CreateRandomPlot(10, 5, 1);
            GenerateRandomPlotsAndSave(10,5,1,2,6);
        }

        //clear the gird
        if (Input.GetKeyDown(KeyCode.R))
        {
            _voxelGrid.ClearGrid();
        }
    }

    #endregion

    #region Private Methods

    //随机生成一些plot然后再测试,基于几个点的最短路径
    void GenerateRandomPlotsAndSave(int area, int maxVoxles, int minVoxels, int minRadius,int maxRadius)
    {

        string saveFolder = "PlotsTest";

        for (int i = 0; i < area; i++)
        {
            // control influence of random seed
            int areasize = Random.Range(minVoxels, maxVoxles);
            _voxelGrid.ClearGrid();

            CreateRandomPlot(areasize,maxVoxles,minVoxels);

            //shortest path from image to grid
            //get data from voxelgrid and translate it into image
            Texture2D gridImage = _voxelGrid.ImageFromGrid();

            //resize image
            Texture2D resizedImage = ImageReadWrite.Resize256(gridImage, Color.black);//pass through grid image

            ImageReadWrite.SaveImage(resizedImage, $"{saveFolder}/Grid_{i}");
        }
    }

    // method to creat each random plot
    void CreateRandomPlot(int areasize,  int maxVoxles, int minVoxels,int height =0)
    {
        for (int i = 0; i < areasize; i++)
        {
            bool success = false;

            //if GrowPlot is not successed, do this
            while (!success)
            {
                //a random value between 0,1 - taggle
                float rand = Random.value;
                
                //only generate plot on the garden or empty land

                int x;
                int z;
                int distance;

                //类似一个概率滑块来将随机值和0.5的概率联系
                if (rand < 0.5f)
                {
                    // condition(bool) + ? + if result is true set to 0 + : else set to maxium
                    x = Random.value < 0.5f ? Random.Range(0,_voxelGrid.GridSize.x) : _voxelGrid.GridSize.x - 1;
                    z = Random.Range(0, _voxelGrid.GridSize.x);
                    distance = 1;

                }
                //>0.5, opposite
                else
                {
                    z = Random.value < 0.5f ? Random.Range(0, _voxelGrid.GridSize.z) : _voxelGrid.GridSize.z - 1;
                    x = Random.Range(0, _voxelGrid.GridSize.z);
                    distance = 2;
                }


                Vector3Int startPoint = new Vector3Int(x, 0, z);
                Vector3Int endPoint = new Vector3Int(x+distance, 0, z+distance);

                int voxelAmt = Random.Range(minVoxels, maxVoxles);

                success = _voxelGrid.GrowPlot(startPoint, voxelAmt);

                

            }

        }
    }

    //返回voxel
    Voxel StartVoxel() 
    {

        Voxel selected = null;
        

   

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if(Physics.Raycast(ray,out RaycastHit hit))
        {
            //Get this voxel propiety
            Transform objectHit = hit.transform;

            if (objectHit.CompareTag("Voxel"))
            {
                //get its name(index)
                string voxelName = objectHit.name;
                var index = voxelName.Split('_').Select(v => int.Parse(v)).ToArray();
                
                

                //reture its index
                selected = _voxelGrid.Voxels[index[0], index[1], index[2]];

                //give white color
                Drawing.DrawCube(selected.Index, _voxelGrid.VoxelSize, Color.white);
                //selected.VoxelCollider.GetComponent<Renderer>().material.color = Color.white;

            }
        }

        return selected;
    }

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
                
                 if (voxel.FColor == FunctionColor.Red)     Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.red);
                else if (voxel.FColor == FunctionColor.Yellow)  Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.yellow);
                else if (voxel.FColor == FunctionColor.Green)   Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.green);
                else if (voxel.FColor == FunctionColor.Cyan)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.cyan);
                else if (voxel.FColor == FunctionColor.Magenta) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.magenta);
                else if (voxel.FColor == FunctionColor.Blue)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.blue);
                else if (voxel.FColor == FunctionColor.White)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.white);
                else if (voxel.FColor == FunctionColor.Gray)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.gray);
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

                GraphicVoxel selected = null;
                string voxelName = objectHit.name;
                var index = voxelName.Split('_').Select(v => int.Parse(v)).ToArray();

                //reture its index
                selected = (GraphicVoxel)_voxelGrid.Voxels[index[0], index[1], index[2]];

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
        Queue<GraphicVoxel> targetPool = new Queue<GraphicVoxel>(_targets);
        var edges = _voxelGrid.GetEdgesByTypes(FunctionColor.Blue, FunctionColor.White);
        Debug.Log(edges.Count);

        UndirecteGraph<GraphicVoxel, Edge<GraphicVoxel>> graph = new UndirecteGraph<GraphicVoxel, Edge<GraphicVoxel>>(edges);
        Dijkstra<GraphicVoxel, Edge<GraphicVoxel>> dijkstra = new Dijkstra<GraphicVoxel, Edge<GraphicVoxel>>(graph);
        _path.AddRange(dijkstra.GetShortestPath(targetPool.Dequeue(), targetPool.Dequeue()));

        while (targetPool.Count > 0)
        {
            GraphicVoxel nextVoxel = targetPool.Dequeue();
            SetNextShortestPath(nextVoxel, dijkstra);
        }
        Debug.Log(_path.Count);

        foreach (var voxel in _path)
        {
            voxel.FColor = FunctionColor.White;
        }
    }

    void SetNextShortestPath(GraphicVoxel targetVoxel, Dijkstra<GraphicVoxel, Edge<GraphicVoxel>> dijkstra)
    {
        dijkstra.DijkstraCalculateWeights(targetVoxel);
        GraphicVoxel closestVoxel = _path.MinBy(v => dijkstra.VertexWeight(v));
        List<GraphicVoxel> newpath = new List<GraphicVoxel>();
        
        newpath.AddRange(dijkstra.GetShortestPath(targetVoxel, closestVoxel));
        newpath.Remove(closestVoxel);
        _path.AddRange(newpath);
    }

    #endregion
}
