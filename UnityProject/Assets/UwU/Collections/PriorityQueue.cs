namespace UwU.Collections
{
    using System;
    using System.Collections.Generic;

    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private struct Node
        {
            public TElement Element;
            public TPriority Priority;

            public Node(TElement element, TPriority priority)
            {
                this.Element = element;
                this.Priority = priority;
            }
        }

        private List<Node> _heap = new();

        public int Count => this._heap.Count;

        public void Enqueue(TElement element, TPriority priority)
        {
            Node node = new(element, priority);
            this._heap.Add(node);
            HeapifyUp(this._heap.Count - 1);
        }

        public TElement Dequeue()
        {
            if (this._heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var frontElement = this._heap[0].Element;

            int lastIndex = this._heap.Count - 1;
            this._heap[0] = this._heap[lastIndex];
            this._heap.RemoveAt(lastIndex);

            if (this._heap.Count > 0)
            {
                HeapifyDown(0);
            }

            return frontElement;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (this._heap[index].Priority.CompareTo(this._heap[parentIndex].Priority) < 0)
                {
                    var temp = this._heap[index];
                    this._heap[index] = this._heap[parentIndex];
                    this._heap[parentIndex] = temp;
                    index = parentIndex;
                }
                else
                {
                    break;
                }
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = this._heap.Count - 1;
            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                if (leftChild <= lastIndex && this._heap[leftChild].Priority.CompareTo(this._heap[smallest].Priority) < 0)
                {
                    smallest = leftChild;
                }

                if (rightChild <= lastIndex && this._heap[rightChild].Priority.CompareTo(this._heap[smallest].Priority) < 0)
                {
                    smallest = rightChild;
                }

                if (smallest != index)
                {
                    var temp = this._heap[index];
                    this._heap[index] = this._heap[smallest];
                    this._heap[smallest] = temp;
                    index = smallest;
                }
                else
                {
                    break;
                }
            }
        }
    }
}