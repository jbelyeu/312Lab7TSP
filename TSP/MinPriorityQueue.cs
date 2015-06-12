using System;
using System.Collections.Generic;
using System.Text;


namespace VisualIntelligentScissors
{
    //source: http://www.codeproject.com/Articles/126751/Priority-queue-in-C-with-the-help-of-heap-data-str
    public class MinPQ<TPriority, TValue>
    {
        private List<KeyValuePair<TPriority, TValue>> baseHeap;
        private IComparer<TPriority> comparer;

        public MinPQ()
            : this(Comparer<TPriority>.Default)
        { }

        public MinPQ(IComparer<TPriority> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException();
            }

            this.baseHeap = new List<KeyValuePair<TPriority, TValue>>();
            this.comparer = comparer;
        }

        public void enqueue(TPriority priority, TValue value)
        {
            this.insert(priority, value);
        }

        public TValue dequeue(TPriority priority, TValue value)
        {
            return dequeueValue().Value;
        }

        public KeyValuePair<TPriority, TValue> seeNext()
        {
            if (!isEmpty)
            {
                return this.baseHeap[0];
            }
            else
            {
                throw new InvalidOperationException("Priority Queue is empty");
            }
        }

        public TValue seeNextVal()
        {
            return seeNext().Value;
        }

        public TPriority seeNextKey()
        {
            return seeNext().Key;
        }

        public bool isEmpty
        {
            get { return baseHeap.Count == 0; }
        }

        public bool contains(TPriority priority, TValue value)
        {
            KeyValuePair<TPriority, TValue> toCheck = new KeyValuePair<TPriority, TValue>(priority, value);
            return this.baseHeap.Contains(toCheck);
        }

        private void insert(TPriority priority, TValue value)
        {
            KeyValuePair<TPriority, TValue> newVal = new KeyValuePair<TPriority, TValue>(priority, value);
            this.baseHeap.Add(newVal);

            this.heapifyForInsert(this.baseHeap.Count - 1); //runs heapify backward through the list
        }

        private KeyValuePair<TPriority, TValue> dequeueValue()
        {
            if (!isEmpty)
            {
                KeyValuePair<TPriority, TValue> result = this.baseHeap[0];
                this.deleteRoot();
                return result;
            }
            else
            {
                throw new InvalidOperationException("Queue empty");
            }
        }

        private void deleteRoot()
        {
            if (isEmpty)
            {
                this.baseHeap.Clear();
                return;
            }

            this.baseHeap[0] = baseHeap[baseHeap.Count - 1];
            this.baseHeap.RemoveAt(baseHeap.Count - 1);

            this.heapifyForDelete(0);
        }

        private int heapifyForInsert(int pos)
        {
            if (pos >= baseHeap.Count) //end of the heap
            {
                return -1;
            }

            while (pos > 0) //work back toward the beginning
            {
                int parentPos = (pos - 1) / 2;
                //compare middle to pos. If parent greater than pos, exchange them 
                if (comparer.Compare(baseHeap[parentPos].Key, baseHeap[pos].Key) > 0)
                {
                    exchangeElements(parentPos, pos);
                    pos = parentPos;
                }
                else break;
            }
            return pos;
        }

        private void heapifyForDelete(int pos)
        {
            if (pos >= baseHeap.Count) //end of the heap
            {
                return;
            }

            while (true) //work from beginning until done
            {
                //exhange each element with smallest child
                int smallest = pos;
                int left = 2 * pos + 1;
                int right = 2 * pos + 2;

                //if middle greater than left child
                if (left < this.baseHeap.Count &&
                    this.comparer.Compare(this.baseHeap[smallest].Key, this.baseHeap[left].Key) > 0)
                {
                    smallest = left;
                }
                //if middle greater than right child
                if (right < this.baseHeap.Count &&
                    this.comparer.Compare(this.baseHeap[smallest].Key, this.baseHeap[right].Key) > 0)
                {
                    smallest = right;
                }
                if (smallest != pos)
                {
                    this.exchangeElements(smallest, pos);
                    pos = smallest;
                }
                else break;
            }
        }

        private void exchangeElements(int pos1, int pos2)
        {
            //value is a value-holder while pos2 overwrites pos1
            KeyValuePair<TPriority, TValue> value = baseHeap[pos1];
            baseHeap[pos1] = baseHeap[pos2];
            baseHeap[pos2] = value;
        }



    }
}
