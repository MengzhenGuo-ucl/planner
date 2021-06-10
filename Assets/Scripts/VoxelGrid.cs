using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EasyGraph;

public class VoxelGrid
{
    #region Public fields

    public Vector3Int GridSize;
    public Voxel[,,] Voxels;
    public Corner[,,] Corners;
    public Face[][,,] Faces = new Face[3][,,];
    public Edge[][,,] Edges = new Edge[3][,,];
    public Vector3 Origin;
    public Vector3 Corner;
    public float VoxelSize { get; private set; }

    public UndirecteGraph<GVoxel, Edge<GVoxel>> Graph;
    private List<Edge<GVoxel>> _edges;
    public Dijkstra<GVoxel, Edge<GVoxel>> DijkstraGraph;

    public int state;
    public Vector3Int Index;
    public int currentRule = 1;
    

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor for a basic <see cref="VoxelGrid"/>
    /// Adds a game object containing a collider to each of first layer voxels
    /// </summary>
    /// <param name="size">Size of the grid</param>
    /// <param name="origin">Origin of the grid</param>
    /// <param name="voxelSize">The size of each <see cref="Voxel"/></param>
    public VoxelGrid(Vector3Int size, Vector3 origin, float voxelSize, Transform parent = null)
    {
        GridSize = size;
        Origin = origin;
        VoxelSize = voxelSize;

        Voxels = new GVoxel[GridSize.x, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    if (y == 0)
                    {
                        Voxels[x, y, z] = new Voxel(
                            new Vector3Int(x, y, z),
                            this,
                            createCollider: true,
                            parent: parent);
                    }
                    else
                    {
                        Voxels[x, y, z] = new Voxel(
                            new Vector3Int(x, y, z),
                            this);
                    }

                }
            }
        }

        MakeFaces();
        MakeCorners();
        MakeEdges();
    }

    /// <summary>
    /// Create voxel grid from the size of input image
    /// </summary>
    /// <param name="input"></input image>
    /// <param name="origin"></origin>
    /// <param name="voxelSize"></size>
    /// <param name="height"></param>
    /// <param name="parent"></param>
    /// 
    public VoxelGrid(Texture2D input, Vector3 origin, int height, float voxelSize, Transform parent = null)
    {

        // create new grid with image size
        GridSize = new Vector3Int(input.width, height, input.height);

        Origin = origin;
        VoxelSize = voxelSize;

        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];
        _edges = new List<Edge<GVoxel>>();

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    if (y == 0)
                    {
                        Voxels[x, y, z] = new GVoxel(
                            new Vector3Int(x, y, z),
                            this,
                            0,
                            createCollider: true,
                            parent: parent);

                        if (x >0)
                        {
                            _edges.Add(new Edge<GVoxel>(Voxels[x, y, z] as GVoxel, Voxels[x - 1, y, z] as GVoxel));
                        }
                        if (z > 0)
                        {
                            _edges.Add(new Edge<GVoxel>(Voxels[x, y, z] as GVoxel, Voxels[x, y, z-1] as GVoxel));
                        }
                    }
                    else
                    {
                        Voxels[x, y, z] = new GVoxel(
                            new Vector3Int(x, y, z),
                            this,
                            0);
                    }
                }
            }
        }

        Graph = new UndirecteGraph<GVoxel, Edge<GVoxel>>(_edges);
        DijkstraGraph = new Dijkstra<GVoxel, Edge<GVoxel>>(Graph);



        MakeFaces();
        MakeCorners();
        MakeEdges();
    }

    #endregion

    #region Grid elements constructors

    /// <summary>
    /// Creates the Faces of each <see cref="Voxel"/>
    /// </summary>
    private void MakeFaces()
    {
        // make faces
        Faces[0] = new Face[GridSize.x + 1, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
                }

        Faces[1] = new Face[GridSize.x, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
                }

        Faces[2] = new Face[GridSize.x, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
                }
    }

    /// <summary>
    /// Creates the Corners of each Voxel
    /// </summary>
    private void MakeCorners()
    {
        Corner = new Vector3(Origin.x - VoxelSize / 2, Origin.y - VoxelSize / 2, Origin.z - VoxelSize / 2);

        Corners = new Corner[GridSize.x + 1, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
                }
    }

    /// <summary>
    /// Creates the Edges of each Voxel
    /// </summary>
    private void MakeEdges()
    {
        Edges[2] = new Edge[GridSize.x + 1, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
                }

        Edges[0] = new Edge[GridSize.x, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
                }

        Edges[1] = new Edge[GridSize.x + 1, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
                }
    }

    #endregion

    #region Grid operations


    /// <summary>
    /// Get the Faces of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the faces</returns>
    public IEnumerable<Face> GetFaces()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Faces[n].GetLength(0);
            int ySize = Faces[n].GetLength(1);
            int zSize = Faces[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Faces[n][x, y, z];
                    }
        }
    }

    /// <summary>
    /// Get the Voxels of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Voxels</returns>
    public IEnumerable<Voxel> GetVoxels()
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    yield return Voxels[x, y, z];
                }
    }

    /// <summary>
    /// Get the Corners of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Corners</returns>
    public IEnumerable<Corner> GetCorners()
    {
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    yield return Corners[x, y, z];
                }
    }

    /// <summary>
    /// Get the Edges of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the edges</returns>
    public IEnumerable<Edge> GetEdges()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Edges[n].GetLength(0);
            int ySize = Edges[n].GetLength(1);
            int zSize = Edges[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Edges[n][x, y, z];
                    }
        }
    }

    #endregion

    #region Public Methods

    public void ResetGrid(bool alive)
    {
        foreach (GVoxel voxel in Voxels)
        {
            voxel.IsAlive = alive;

        }
    }

    #region Game of life not used

    //public void UpdateGrid()
    //{
    //    for (int x = 0; x < GridSize.x; x++)
    //    {
    //        for (int z = 0; z < GridSize.z; z++)
    //        {

    //            GOLRules((GVoxel)Voxels[x, 0, z]);


    //            //foreach (GVoxel voxel in Voxels)
    //            //{
    //            //    voxel.ChangeStatus();
    //            //}
    //        }
    //    }
    //}

    //private void GOLRules(GVoxel tempVoxel)
    //{
    //    int numOfAliveNeighbours;

    //    //GVoxel AliveVoxel = GetVoxels().Where(v => v.IsAlive) as GVoxel;
    //    //tempVoxel = AliveVoxel.GetEightNeighbours();

    //    numOfAliveNeighbours = GetNumberOfAliveNeighbours(tempVoxel.GetEightNeighbours().ToList());

    //    if (numOfAliveNeighbours < 1) tempVoxel.ChangeStatus();
    //    else if (numOfAliveNeighbours <= 4) tempVoxel.ChangeStatus();
    //    else if (numOfAliveNeighbours > 4) tempVoxel.ChangeStatus();

    //    Debug.Log(numOfAliveNeighbours);


    //}

    //private int GetNumberOfAliveNeighbours(List<GVoxel> neighbours)
    //{
    //    int numOfAliveNeighbours = 0;

    //    for (int x = 0; x < GridSize.x; x++)
    //    {
    //        for (int z = 0; z < GridSize.z; z++)

    //        {
                
    //            GVoxel AliveVoxel = GetVoxels().Where(v => v.IsAlive) as GVoxel;


    //            foreach (var vox in neighbours)
    //            {
                    
    //                if (vox == AliveVoxel) numOfAliveNeighbours++;
    //            }

    //        }
    //    }
    //    return numOfAliveNeighbours;

    //}

    //public IEnumerable<GVoxel> GetEightNeighbours()
    //{

    //    int x = Index.x;
    //    int y = Index.y;
    //    int z = Index.z;
    //    var s = GridSize;

    //    if (x != s.x - 1) yield return(GVoxel)Voxels[x + 1, y, z];

    //    if (x != 0) yield return (GVoxel)Voxels[x - 1, y, z];

    //    if (z != s.z - 1) yield return(GVoxel)Voxels[x, y, z + 1];

    //    if (z != 0) yield return (GVoxel)Voxels[x, y, z - 1];

    //    if (z != 0 && x != 0 && x != s.x - 1 && z != s.z - 1)
    //    {
    //        yield return (GVoxel)Voxels[x - 1, y, z - 1];
    //        yield return (GVoxel)Voxels[x - 1, y, z + 1];
    //        yield return (GVoxel)Voxels[x + 1, y, z + 1];
    //        yield return (GVoxel)Voxels[x + 1, y, z - 1];
    //    }

    //}

    //void UpdateStates()
    //{
    //    //loop through all voxel in the blue area
    //    for (int x = 0; x < GridSize.x; x++)
    //    {
    //        for (int z = 0; z < GridSize.z; z++)
    //        {
    //            //Get current state of the voxel

    //            GVoxel voxel = (GVoxel)Voxels[x,0,z];

    //            voxel.SetState(state);

    //            int rl = currentRule;

    //            //Initial result of the update is the previous state of the cell
    //            int result = state;

    //            //Get the living neighbour count of current cell
    //            int count = GetLivingNeighbours(x, z);

    //            //Check the rules and apply the correct state based on the result
    //            if (state == 1 && count < 2)
    //            {
    //                result = 0;
    //                rl = 0;
    //            }
    //            if (state == 1 && (count == 2 || count == 3))
    //            {
    //                result = 1;
    //                rl = 1;
    //            }
    //            if (state == 1 && count > 3)
    //            {
    //                result = 0;
    //                rl = 2;
    //            }
    //            if (state == 0 && count == 3)
    //            {
    //                result = 1;
    //                rl = 3;
    //            }


    //            //states[x, y] = result;
    //            //rule[x, y] = rl;



    //        }
    //    }

    //}

    #endregion

    /// <summary>
    /// Expand shortest path with determined radius
    /// </summary>
    /// <param name="path"></param>shortest path voxels
    /// <param name="radius"></param>expanded radius
    /// <returns></returns>
    public List<GVoxel> GrowPlot(List<GVoxel> path, int radius)
    {
        List<GVoxel> expandedVoxels = new List<GVoxel>();

        foreach (var voxel in path)
        {
            List<GVoxel> availableVoxels = new List<GVoxel>();
            availableVoxels.Add(voxel);
            //Iterate through the neighboring layer within the radius
            for (int i = 0; i < radius; i++)
            {
                List<GVoxel> tempVoxels = new List<GVoxel>();

                foreach (var aVoxel in availableVoxels)
                {
                    //Get neighbors
                    Voxel[] neighbours;
                    neighbours = aVoxel.GetFaceNeighbours().ToArray();

                    //Iterate each neighbors + and check if is available
                    foreach (GVoxel neighbour in neighbours)
                    {
                        //+ if color is blue(backyard area that allows to grow)
                        if (neighbour.FColor == FunctionColor.Blue 
                            && neighbour.IsActive /*&& Util.ValidateIndex(GridSize, neighbour.Index)*/ 
                            && !availableVoxels.Contains(neighbour)
                            && !tempVoxels.Contains(neighbour))
                        {
                            tempVoxels.Add(neighbour);
                        }
                    }
                    //Debug.Log(tempVoxels.Count);
                }
                availableVoxels.AddRange(tempVoxels);
            }
            expandedVoxels.AddRange(availableVoxels);
        }
        // set the plot color and quality
        foreach (GVoxel gvoxel in expandedVoxels)
        {
            path.Add(gvoxel);
            gvoxel.FColor = FunctionColor.Yellow;
            gvoxel.Qname = ColorQuality.Plot;
            gvoxel.SetState(1);

        }
        return expandedVoxels;
    }

    /// <summary>
    /// Reads an image pixel data and set the color pixels and corresponding label/quality to the grid
    /// </summary>
    /// <param name="image">The reference image</param>
    /// <param name="layer">The target layer</param>
    public void SetStatesFromImage(Texture2D inputImage, int layer = 0)
    {
        // Iterate through the XZ plane
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int z = 0; z < GridSize.z; z++)
            {
                // Get the pixel color from the image
                Color pixel = inputImage.GetPixel(x, z);

                //read voxel as its child:graphvoxel

                GVoxel voxel = (GVoxel)Voxels[x, 0, z];
                //read RGB channel
                float[] colorScores = new float[8]
                {
               Mathf.Abs(pixel.r - 0) + Mathf.Abs(pixel.g - 1) + Mathf.Abs(pixel.b - 1),//Cyan
                Mathf.Abs(pixel.r - 0) + Mathf.Abs(pixel.g - 0) + Mathf.Abs(pixel.b - 1),//Blue
                Mathf.Abs(pixel.r - 1) + Mathf.Abs(pixel.g - 0) + Mathf.Abs(pixel.b - 1),//Magenta 
                Mathf.Abs(pixel.r - 1) + Mathf.Abs(pixel.g - 0) + Mathf.Abs(pixel.b - 0),//Red
                Mathf.Abs(pixel.r - 0) + Mathf.Abs(pixel.g - 1) + Mathf.Abs(pixel.b - 0),//Green
                Mathf.Abs(pixel.r - 1) + Mathf.Abs(pixel.g - 1) + Mathf.Abs(pixel.b - 0),//Yellow
                Mathf.Abs(pixel.r - 1) + Mathf.Abs(pixel.g - 1) + Mathf.Abs(pixel.b - 1),//White
                Mathf.Abs(pixel.r - 1f) + Mathf.Abs(pixel.g - 0.6f) + Mathf.Abs(pixel.b - 0.0f)//Orange
              
                };
                //Get the colors right


                float biggestScore = 0;
                int colorIndex = 0;
                int colorQuality = 0;

                for (int i = 0; i < colorScores.Length; i++)
                {
                    if (colorScores[i] > biggestScore)
                    {
                        biggestScore = colorScores[i];
                        colorIndex = i;
                        colorQuality = i;
                    }
                }

                Voxels[x, layer, z].FColor = (FunctionColor)colorIndex;
                Voxels[x, layer, z].Qname = (ColorQuality)colorQuality;


                // assign the layermask to be used in analysis
                foreach (var voxels in Voxels)
                {
                    if (voxel.IsActive && voxel.VoxelCollider.gameObject.layer == 0)
                    {

                        if (voxel.FColor == FunctionColor.Blue) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("Backyard");
                        else if (voxel.FColor == FunctionColor.Red) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("House");
                        else if (voxel.FColor == FunctionColor.Yellow) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("Extension");
                        else if (voxel.FColor == FunctionColor.Magenta) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("Boundary");
                        else if (voxel.FColor == FunctionColor.Green) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("Tree");
                        else if (voxel.FColor == FunctionColor.White) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("Plot");
                        else if (voxel.FColor == FunctionColor.Cyan) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("Road");
                        else if (voxel.FColor == FunctionColor.Orange) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("SmallBuilding");
                        else if (voxel.FColor == FunctionColor.Empty) voxel.VoxelCollider.gameObject.layer = LayerMask.NameToLayer("EmptyLand");

                    }

                }

            }
        }

    }


    public void ClearGrid()
    {
        foreach (var voxel in Voxels)
        {
            voxel.FColor = FunctionColor.Empty;
        }
    }


    // Generate image from Grid, voxel to pixel, read the plot on the top layer(ovverlapping) FROM VOXEL DATA TO IMAGE
    public Texture2D ImageFromGrid(int layer = 0, bool overlapping = false)
    {
        TextureFormat textureFormat;
        //RGB png 8 depth
        textureFormat = TextureFormat.RGB24;

        //new image
        Texture2D gridImage = new Texture2D(GridSize.x, GridSize.z, textureFormat, true);

        //iterare through all voxles, get their color and write to grid image
        for (int i = 0; i < GridSize.x; i++)
        {
            for (int j = 0; j < GridSize.z; j++)
            {
                //get all voxel from x,z
                var voxel = Voxels[i, 0, j];

                // assign color based on function color
                Color co;
                if (voxel.FColor == FunctionColor.White) co = Color.white;
                else if (voxel.FColor == FunctionColor.Red) co = Color.red;
               
                else if (voxel.FColor == FunctionColor.Blue) co = Color.blue;
                else if (voxel.FColor == FunctionColor.Yellow) co = Color.yellow;
                else if (voxel.FColor == FunctionColor.Green) co = Color.green;
                else if (voxel.FColor == FunctionColor.Cyan) co = Color.cyan;
                else if (voxel.FColor == FunctionColor.Magenta) co = Color.magenta;
                else if (voxel.FColor == FunctionColor.Orange) co = Color.gray;
                else co = new Color(1.0f, 0.64f, 0f); //orange  rgb value/max255

                gridImage.SetPixel(i, j, co);

            }
        }

        //from memory value to actual pixel
        gridImage.Apply();
        return gridImage;
    }

    public List<Edge<GVoxel>> GetEdgesOfTypes(FunctionColor color) => _edges.Where(e => e.Source.FColor == color && e.Target.FColor == color).ToList();
    public List<Edge<GVoxel>> GetEdgesByTypes(FunctionColor color1, FunctionColor color2) => _edges.Where(
        e => (e.Source.FColor == color1 || e.Source.FColor == color2) &&
        (e.Target.FColor == color1 || e.Target.FColor == color2)).ToList();



    #endregion
}


/// <summary>
/// Color coded values
/// </summary>
public enum FunctionColor
{
    Empty = -1,
    Red = 0,
    Yellow = 1,
    Green = 2,
    Cyan = 3,
    Magenta = 4,
    Blue = 5,
    White = 6,
    Orange = 7

}

public enum ColorQuality
{
    Plot = 0,
    House = 1,
    Street = 2,
    Backyard = 3,
    Frontyard = 4,
    SmallBuilding = 5,
    Tree = 6,
    EmptyLand = 7,
    LandTexture = 8

}