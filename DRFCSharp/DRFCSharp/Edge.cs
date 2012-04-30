using System;

namespace DRFCSharp
{
	/// <summary>
	/// Edge.
	/// 
	/// The edge class can represent one or two directed edges of a flow network.
	/// In the case where only one of (vi, vj) is an edge, the instance variable
	/// only capacity_i_to_j will be greater than zero.  But, in the case that 
	/// the both (v1, v2) and (v2, v1) are edges of the flow network, both 
	/// directions will have positive capacities.
	/// </summary>
	public class Edge
	{
		Vertex v1;
		Vertex v2;
		double capacity_1_to_2;
		double capacity_2_to_1;
		double flow_1_to_2; // flow_2_to_1 = -flow_1_to_2
		
		
		public Edge (Vertex v1, Vertex v2, double capacity_1_to_2, double capacity_2_to_1)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.capacity_1_to_2 = capacity_1_to_2;
			this.capacity_2_to_1 = capacity_2_to_1;
			this.flow_1_to_2 = 0.0;
		}
		
		public static Edge AddEdge(Vertex v1, Vertex v2, double capacity_1_to_2, double capacity_2_to_1)
		{
			Edge toReturn = new Edge(v1,v2,capacity_1_to_2,capacity_2_to_1);
			v1.AddEdge(toReturn);
			v2.AddEdge(toReturn);
			return toReturn;
		}
		
		
		public Vertex Traverse (Vertex origin_vertex)
		{
			if (origin_vertex == this.v1)
			{
				return this.v2;
			}
			else
			{
				return this.v1;
			}
		}
		
		
		public double ResidualCapacity (Vertex origin_vertex)
		{
			if (origin_vertex == this.v1)
			{
				return this.capacity_1_to_2 - this.flow_1_to_2;
			}
			else
			{
				return this.capacity_2_to_1 + this.flow_1_to_2;
			}
		}
		
		/// <summary>
		/// Modifies the amount of flow currently carried by this edge.
		/// </summary>
		/// <param name='origin_vertex'>
		/// </param>
		/// <param name='flow'>
		/// The flow parameter should always be positive here. This is the amount
		/// of flow we are trying to send across the edge starting from origin_vertex.
		/// </param>
		public void AddFlow (Vertex origin_vertex, double flow)
		{
			if (flow > this.ResidualCapacity(origin_vertex))
			{
				throw new System.ArgumentException("Parameter exceeds allowable bounds.", "flow");
			}
			if (origin_vertex == this.v1)
			{
				this.flow_1_to_2 += flow;
			}
			else
			{
				this.flow_1_to_2 -= flow;
			}
		}		
	}
}

