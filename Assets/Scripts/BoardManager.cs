using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    struct Space {
        public Vector3 pos;
        public Block block;
    }

    Space[] spaces;
    Queue<Block> blockPool;
    public GameObject blockPrefab;
    int[] freeSpaces;

    void Awake() {
        spaces = new Space[27];
        freeSpaces = new int[27]; 
        blockPool = new Queue<Block>();

        for(int i = 0; i < 27; i++) {
            spaces[i].pos = new Vector3(i % 3 - 1, (i % 9) / 3 - 1, i / 9 - 1);
            GameObject blockObj = Instantiate(blockPrefab);
            blockObj.transform.position = new Vector3(1000, 1000, 1000);
            blockPool.Enqueue(blockObj.GetComponent<Block>());
        }
    }

    public void AddBlock() {
        int count = 0;
        for (int i = 0; i < 27; i++) {
            if (spaces[i].block == null) {
                freeSpaces[count] = i;
                count++;
            }
        }
        int num = freeSpaces[Random.Range(0, count)];
        Block b = blockPool.Dequeue();
        spaces[num].block = b;
        b.Spawn(spaces[num].pos);
    }
}
