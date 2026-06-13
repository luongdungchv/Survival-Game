using Dacodelaac.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dacodelaac.Collections
{
    public class KdTreeCollection<T> : BaseSO, IEnumerable<T>, IEnumerable, ISerializationCallbackReceiver where T : IPosition
    {
        [SerializeField] bool just2D;
        [SerializeField] bool clearOnPlay;

        KdNode root;
        KdNode last;
        int count;
        float lastUpdate = -1f;
        KdNode[] open;

        public int Count => count;

        public bool IsReadOnly => false;
        public float AverageSearchLength { protected set; get; }
        public float AverageSearchDeep { protected set; get; }

        public T this[int key]
        {
            get
            {
                if (key >= count)
                    throw new ArgumentOutOfRangeException();
                var current = root;
                for (var i = 0; i < key; i++)
                    current = current.next;
                return current.component;
            }
        }

        public void Add(T item)
        {
            add(new KdNode() {component = item});
        }

        public void AddAll(List<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public T Find(Predicate<T> match)
        {
            var current = root;
            while (current != null)
            {
                if (match(current.component))
                    return current.component;
                current = current.next;
            }

            return default;
        }

        public void RemoveAt(int i)
        {
            var list = new List<KdNode>(getNodes());
            list.RemoveAt(i);
            Clear();
            foreach (var node in list)
            {
                node.oldRef = null;
                node.next = null;
            }

            foreach (var node in list)
                add(node);
        }

        public void RemoveAll(Predicate<T> match)
        {
            var list = new List<KdNode>(getNodes());
            list.RemoveAll(n => match(n.component));
            Clear();
            foreach (var node in list)
            {
                node.oldRef = null;
                node.next = null;
            }

            foreach (var node in list)
                add(node);
        }

        public int CountAll(Predicate<T> match)
        {
            var count = 0;
            foreach (var node in this)
            {
                if (match(node))
                {
                    count++;
                }
            }
            return count;
        }

        public void Clear()
        {
            root = null;
            last = null;
            count = 0;
        }

        public void UpdatePositions(float rate)
        {
            if (Time.timeSinceLevelLoad - lastUpdate < 1f / rate)
                return;

            lastUpdate = Time.timeSinceLevelLoad;

            UpdatePositions();
        }

        public void UpdatePositions()
        {
            //save old traverse
            var current = root;
            while (current != null)
            {
                current.oldRef = current.next;
                current = current.next;
            }

            //save root
            current = root;

            //reset values
            Clear();

            //readd
            while (current != null)
            {
                add(current);
                current = current.oldRef;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = root;
            while (current != null)
            {
                yield return current.component;
                current = current.next;
            }
        }

        public List<T> ToList()
        {
            var list = new List<T>();
            foreach (var node in this)
                list.Add(node);
            return list;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected float distance(Vector3 a, Vector3 b)
        {
            if (just2D)
                return (a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z);
            else
                return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
        }

        protected float getSplitValue(int level, Vector3 position)
        {
            if (just2D)
            {
                return (level % 2 == 0) ? position.x : position.z;
            }
            else
            {
                return (level % 3 == 0) ? position.x : (level % 3 == 1) ? position.y : position.z;
            }
        }

        void add(KdNode newNode)
        {
            count++;
            newNode.left = null;
            newNode.right = null;
            newNode.level = 0;
            var parent = findParent(newNode.component.Position);

            //set last
            if (last != null)
                last.next = newNode;
            last = newNode;

            //set root
            if (parent == null)
            {
                root = newNode;
                return;
            }

            var splitParent = getSplitValue(parent);
            var splitNew = getSplitValue(parent.level, newNode.component.Position);

            newNode.level = parent.level + 1;

            if (splitNew < splitParent)
                parent.left = newNode; //go left
            else
                parent.right = newNode; //go right
        }

        KdNode findParent(Vector3 position)
        {
            //travers from root to bottom and check every node
            var current = root;
            var parent = root;
            while (current != null)
            {
                var splitCurrent = getSplitValue(current);
                var splitSearch = getSplitValue(current.level, position);

                parent = current;
                if (splitSearch < splitCurrent)
                    current = current.left; //go left
                else
                    current = current.right; //go right
            }

            return parent;
        }

        public T FindClosest(Vector3 position)
        {
            return findClosest(position);
        }

        public IEnumerable<T> FindClose(Vector3 position)
        {
            var output = new List<T>();
            findClosest(position, output);
            return output;
        }

        protected T findClosest(Vector3 position, List<T> traversed = null)
        {
            if (root == null)
                return default;

            var nearestDist = float.MaxValue;
            KdNode nearest = null;

            if (open == null || open.Length < Count)
                open = new KdNode[Count];
            for (int i = 0; i < open.Length; i++)
                open[i] = null;

            var openAdd = 0;
            var openCur = 0;

            if (root != null)
                open[openAdd++] = root;

            while (openCur < open.Length && open[openCur] != null)
            {
                var current = open[openCur++];
                if (traversed != null)
                    traversed.Add(current.component);

                var nodeDist = distance(position, current.component.Position);
                if (nodeDist < nearestDist)
                {
                    nearestDist = nodeDist;
                    nearest = current;
                }

                var splitCurrent = getSplitValue(current);
                var splitSearch = getSplitValue(current.level, position);

                if (splitSearch < splitCurrent)
                {
                    if (current.left != null)
                        open[openAdd++] = current.left; //go left
                    if (Mathf.Abs(splitCurrent - splitSearch) * Mathf.Abs(splitCurrent - splitSearch) < nearestDist &&
                        current.right != null)
                        open[openAdd++] = current.right; //go right
                }
                else
                {
                    if (current.right != null)
                        open[openAdd++] = current.right; //go right
                    if (Mathf.Abs(splitCurrent - splitSearch) * Mathf.Abs(splitCurrent - splitSearch) < nearestDist &&
                        current.left != null)
                        open[openAdd++] = current.left; //go left
                }
            }

            AverageSearchLength = (99f * AverageSearchLength + openCur) / 100f;
            AverageSearchDeep = (99f * AverageSearchDeep + nearest.level) / 100f;

            return nearest.component;
        }

        float getSplitValue(KdNode node)
        {
            return getSplitValue(node.level, node.component.Position);
        }

        IEnumerable<KdNode> getNodes()
        {
            var current = root;
            while (current != null)
            {
                yield return current;
                current = current.next;
            }
        }
        
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (clearOnPlay)
            {
                Clear();
            }
        }

        protected class KdNode
        {
            internal T component;
            internal int level;
            internal KdNode left;
            internal KdNode right;
            internal KdNode next;
            internal KdNode oldRef;
        }
    }
}