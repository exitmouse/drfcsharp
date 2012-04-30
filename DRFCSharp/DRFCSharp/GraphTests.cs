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
			
	}
}

