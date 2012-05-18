using NUnit.Framework;
using System;

namespace DRFCSharp
{
	[TestFixture]
	public class GraphTests
	{
		Vertex a;
		Vertex b;
		Edge e;
		[SetUp]
		public void TwoVertexGraph()
		{
			a = new Vertex();
			b = new Vertex();
			e = Edge.AddEdge(a,b,1d,1d);
		}
		[Test]
		public void EdgeAddBoth(){
			Assert.AreSame(a.edges[0],b.edges[0]);
		}
		[Test]
		public void AddFlowCanWork(){
			Assert.IsTrue(a.AddFlowTo(b));
		}
		[Test]
		public void AddFlowMaximizes(){
			a.AddFlowTo(b);
			Assert.Less(e.ResidualCapacity(a),0.0001d);
		}
		[Test]
		public void AddFlowIsSane(){
			a.AddFlowTo(b);
			Assert.GreaterOrEqual(e.ResidualCapacity(a),0d);
		}
		[Test]
		public void AddFlowRespectsMinimumCapacityInPath(){
			Vertex c = new Vertex();
			Edge f = Edge.AddEdge(b,c,1000d,1000d);
			a.AddFlowTo(c);
			Assert.AreEqual(f.ResidualCapacity(b),999d);
		}
		[Test]
		public void AddFlowGoesBackwards(){
			b.AddFlowTo(a);
			a.AddFlowTo(b);
			Assert.AreEqual(e.ResidualCapacity(a),0d);
		}
		[Test]
		public void ResidualCapacityConnectedNodesSanity(){
			a.ResidualCapacityConnectedNodes();
			Assert.IsTrue(b.tagged_as_one);
		}
		[Test]
		public void ResidualCapacityConnectedNodesDoesNotTravelSaturatedEdges(){
			a.AddFlowTo(b);
			a.ResidualCapacityConnectedNodes();
			Assert.IsFalse(b.tagged_as_one);
		}
		[Test]
		public void CanMakeNontrivialCut()
		{
			Vertex vert1 = new Vertex();
			Vertex vert2 = new Vertex();
			Vertex vert3 = new Vertex();
			Vertex vert4 = new Vertex();
			Vertex vert5 = new Vertex();
			Vertex vert6 = new Vertex();
/*   o:o-o6
 *   |\:
 * o-o-o
 * 1 
 * Where : is a weak edge
 */
			Edge.AddEdge(vert1,vert2,1d,1d);
			Edge.AddEdge(vert2,vert3,1d,1d);
			Edge.AddEdge(vert2,vert4,1d,1d);
			Edge.AddEdge(vert3,vert4,1d,1d);
			Edge.AddEdge(vert3,vert5,0.1d,0.1d);
			Edge.AddEdge(vert4,vert5,0.1d,0.1d);
			Edge.AddEdge(vert5,vert6,1d,1d);
			while(vert1.AddFlowTo(vert6));
			vert1.ResidualCapacityConnectedNodes();
			bool all_on_that_should_be = vert1.tagged_as_one && vert2.tagged_as_one && vert3.tagged_as_one && vert4.tagged_as_one;
			bool all_off_that_should_be = !(vert5.tagged_as_one) && !(vert6.tagged_as_one);
			Assert.IsTrue(all_off_that_should_be && all_on_that_should_be);
		}
	}
}

