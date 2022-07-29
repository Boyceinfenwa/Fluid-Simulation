using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/*
namespace SpatialHash
{
    public interface ISpatialHash3D
    {
        Vector3 GetPos();
        float GetRadius();
    }



    public class NewSpatialHash<T> where T : ISpatialHash3D
    {
        public float sceneWidth;
        public float sceneheight;
        public float sceneDepth;

        public float cellSizeX;
        public float cellSizeY;
        public float cellSizeZ;

        public float minX;
        public float minY;
        public float minZ;

        public int cols;
        public int rows;
        public int lay;

        public Dictionary<int, List<T>> buckets;
        public int bucketSize;

        public SpatialHash(int cols, int rows, int lay, float sceenW, float sceenH, float sceneD, float minX, float minY, float minZ)
        {
            sceneheight = sceenH;
            sceneWidth = sceenW;
            sceneDepth = sceneD;

            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;

            this.cellSizeX = sceenW / cols;
            this.cellSizeY = sceenH / rows;
            this.cellSizeZ = sceneD / lay;

            this.buckets = new Dictionary<int, List<T>>(this.cols * this.rows * this.lay);
            for (int i = 0; i < cols * rows * lay; i++)
            {
                this.buckets.Add(i, new List<T>());
            }
            this.bucketSize = buckets.Count;
        }

        public void Insert(T obj)
        {
            List<T> objects = new List<T>();

            int[] cellIDs = GetBucketIDs(obj);
            for (int i = 0; i < cellIDs.Length; i++)
            {
                int item = cellIDs[i];
                if (item >= bucketSize || item < 0)
                {
                    continue;
                }
                buckets[item].Add(obj);
            }
        }

        public List<T> GetNearby(T obj)
        {
            List<T> objects = new List<T>();
            int[] bucketIds = GetBucketIDs(obj);

            for (int i = 0; i < bucketIds.Length; i++)
            {
                int item = bucketIds[i];

                if (item >= bucketSize || item < 0)
                {
                    continue;
                }
                objects.AddRange(buckets[item]);
            }
            return objects;
        }

        public void Clear()
        {
            for (int i = 0; i < cols * rows * lay; i++)
            {
                this.buckets[i].Clear();
            }
        }

        private void AddBucket(Vector3 vec, float width, int[] bucketIDs, int idx)
        {
            int cellPos = (int)(
                (FastFloor((vec.x - minX) / cellSizeX)) +
                (FastFloor((vec.y - minY) / cellSizeY)) +
                (FastFloor((vec.z - minZ) / cellSizeZ)) * width);

            bucketIDs[idx] = cellPos;
        }
        private int[] GetBucketIDs(T obj)
        {
            int[] bucketIds = new int[4];

            Vector3 objPos = obj.GetPos();
            float objRad = obj.GetRadius();

            Vector3 min = new Vector3(objPos.x - objRad, objPos.y - objRad, objPos.z - objRad);
            Vector3 max = new Vector3(objPos.x + objRad, objPos.y + objRad, objPos.z + objRad);

            AddBucket(min, cols, bucketIds, 0);
            //TopRight
            AddBucket(new Vector3(max.x, min.y,min.z), cols, bucketIds, 1);
            //BottomRight
            AddBucket(max, cols, bucketIds, 2);
            //BottomLeft
            AddBucket(new Vector3(min.x, max.y,max.z), cols, bucketIds, 3);

            return bucketIds;
        }

        private const int _BIG_ENOUGH_INT = 16 * 1024;
        private const double _BIG_ENOUGH_FLOOR = _BIG_ENOUGH_INT + 0.0000;

        private static int FastFloor(float f)
        {
            return (int)(f + _BIG_ENOUGH_FLOOR) - _BIG_ENOUGH_INT;
        }
    }
} */ 