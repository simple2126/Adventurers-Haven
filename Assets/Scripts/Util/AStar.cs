using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStar
{
    private class Node
    {
        public Vector3Int Pos;
        public int G, H;
        public int F => G + H;
        public Node Parent;
        public Node(Vector3Int pos, int g, int h, Node parent) { Pos = pos; G = g; H = h; Parent = parent; }
    }

    private class MinHeap
    {
        private readonly List<Node> data = new();
        public int Count => data.Count;
        public void Push(Node n) { data.Add(n); Up(data.Count - 1); }
        public Node Pop()
        {
            var root = data[0];
            data[0] = data[^1];
            data.RemoveAt(data.Count - 1);
            Down(0);
            return root;
        }
        public void DecreaseKey(Node n) { Up(data.IndexOf(n)); }
        private void Up(int i) { while (i > 0 && Compare(i, (i - 1) / 2)) { Swap(i, (i - 1) / 2); i = (i - 1) / 2; } }
        private void Down(int i)
        {
            while (true)
            {
                int l = i * 2 + 1, r = l + 1, s = i;
                if (l < data.Count && Compare(l, s)) s = l;
                if (r < data.Count && Compare(r, s)) s = r;
                if (s == i) break;
                Swap(i, s); i = s;
            }
        }
        private bool Compare(int a, int b) => data[a].F < data[b].F || (data[a].F == data[b].F && data[a].H < data[b].H);
        private void Swap(int a, int b) { var t = data[a]; data[a] = data[b]; data[b] = t; }
    }

    private readonly Func<Vector3Int, Vector3Int, int> heuristic;
    private readonly Func<Vector3Int, IEnumerable<Vector3Int>> getNeighbors;
    private readonly Func<Vector3Int, Vector3Int, int> getCost;
    private readonly Func<Vector3Int, bool> isValid;

    public AStar(
        Func<Vector3Int, Vector3Int, int> heuristic,
        Func<Vector3Int, IEnumerable<Vector3Int>> getNeighbors,
        Func<Vector3Int, Vector3Int, int> getCost,
        Func<Vector3Int, bool> isValid)
    {
        this.heuristic = heuristic;
        this.getNeighbors = getNeighbors;
        this.getCost = getCost;
        this.isValid = isValid;
    }

    public bool FindPath(Vector3Int start, Vector3Int goal, out List<Vector3Int> result)
    {
        result = null;
        if (!isValid(start) || !isValid(goal)) return false;

        var open = new MinHeap();
        var openDict = new Dictionary<Vector3Int, Node>();
        var closed = new HashSet<Vector3Int>();
        var startNode = new Node(start, 0, heuristic(start, goal), null);

        open.Push(startNode);
        openDict[start] = startNode;

        while (open.Count > 0)
        {
            var curr = open.Pop();
            openDict.Remove(curr.Pos);

            if (curr.Pos == goal)
            {
                result = Reconstruct(curr);
                return true;
            }

            closed.Add(curr.Pos);
            foreach (var n in getNeighbors(curr.Pos))
            {
                if (closed.Contains(n)) continue;

                int gScore = curr.G + getCost(curr.Pos, n);
                if (openDict.TryGetValue(n, out var ex))
                {
                    if (gScore < ex.G) { ex.G = gScore; ex.Parent = curr; open.DecreaseKey(ex); }
                }
                else
                {
                    var node = new Node(n, gScore, heuristic(n, goal), curr);
                    open.Push(node);
                    openDict[n] = node;
                }
            }
        }
        return false;
    }

    private List<Vector3Int> Reconstruct(Node n)
    {
        var rev = new List<Vector3Int>();
        while (n != null) { rev.Add(n.Pos); n = n.Parent; }
        rev.Reverse(); return rev;
    }
}
