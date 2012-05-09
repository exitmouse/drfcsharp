using System;
using System.Collections.Generic;

namespace DRFCSharp
{
	public class Vertex
	{
		public List<Edge> edges;
		public bool tagged_as_one;
		
		public Vertex ()
		{
			edges = new List<Edge>();
			tagged_as_one = false;
		}
		
		public void AddEdge(Edge e)
		{
			edges.Add(e);
		}
		public double AddFlowTo(List<Vertex> traversed, Vertex target, double max_capacity)
		{
			if(max_capacity <= 0d) return 0d;
			if(this == target)
				return max_capacity;
			traversed.Add(this);
			foreach(Edge e in edges)
			{
				Vertex eTo = e.Traverse(this);
				if(traversed.Contains(eTo)) continue;
				double flow_added = eTo.AddFlowTo(traversed, target, Math.Min(e.ResidualCapacity(this),max_capacity));
				if(flow_added > 0d) //If we've added any flow, the path is viable. Travel up the stack actually adding the flow.
				{
					e.AddFlow(this,flow_added);
					return flow_added;
				}
			}
			//If none of the edges returned any flow or were viable, then there is
			//no path with positive capacity from this vertex to the target through
			//as yet untraversed nodes.
			return 0d;
		}
		public bool AddFlowTo(Vertex target)
		{
			return AddFlowTo(new List<Vertex>(),target,30000000d)>0d;//Hack
		}
		public void ResidualCapacityConnectedNodes()
		{
			tagged_as_one = true;
			foreach(Edge e in edges)
			{
				if(e.ResidualCapacity(this) <= 0.0001d) continue; //TODO possibly a hack? Don't know. If <= 0d is safe, we shoulf use it.
				Vertex v = e.Traverse(this);
				if(v.tagged_as_one) continue;
				v.ResidualCapacityConnectedNodes();
			}
		}
	}
}

