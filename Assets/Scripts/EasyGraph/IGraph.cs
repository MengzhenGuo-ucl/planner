using System.Collections;
using System.Collections.Generic;

namespace EasyGraph
{
    public interface IGraph<TVertex, TEdge> 
        where TEdge : IEdge<TVertex>
        where TVertex : class
    {
        /// <summary>
        /// Gets a value indicating if the graph allows parallel edges
        /// </summary>
        bool AllowParallelEdges { get; }

        Dictionary<TVertex, List<TEdge>> VertexEdgeDict { get; }

        List<TVertex> GetAllVertices();
              List<TEdge> GetEdges();
        List<TEdge> GetConnectedEdges(TVertex vertex);
    }
}
