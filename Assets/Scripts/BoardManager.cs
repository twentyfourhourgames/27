using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    struct Space {
        public Vector3 pos;
        public Block block;
    }

    int[,,] rows = {
        { {0,1,2 }, {3,4,5 }, {6,7,8 }, {9,10,11 }, {12,13,14 }, {15,16,17 }, {18,19,20 }, {21,22,23 }, {24,25,26 } },
        { {0,3,6 }, {1,4,7 }, {2,5,8 }, {9,12,15 }, {10,13,16 }, {11,14,17 }, {18,21,24 }, {19,22,25 }, {20,23,26 } },
        { {0,9,18 }, {1,10,19 }, {2,11,20 }, {3,12,21 }, {4,13,22 }, {5,14,23 }, {6,15,24 }, {7,16,25 }, {8,17,26 } }
    };
    Space[] spaces;
    Queue<Block> blockPool;
    public GameObject blockPrefab;
    int[] freeSpaces;
    bool hasMoved;
    bool isReady;

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
        StartGame();
    }

    void StartGame() {
        for (int i = 0; i < 27; i++) {
            Block b = spaces[i].block;
            if (b != null) {
                spaces[i].block = null;
                b.transform.position = new Vector3(1000, 1000, 1000);
                blockPool.Enqueue(b);
            }
        }
        AddBlock();
        isReady = true;
    }

    void AddBlock() {
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

    public void MoveClick(int btn) {
        if (isReady)
            Move((btn));
    }

    void Move(int dir) {
        if (!isReady) return;
        hasMoved = false;
        bool rev = dir > 2;
        dir = dir % 3;
        for (int i = 0; i < 9; i++) {
            bool alreadyMerged = false;
            int first = rows[dir, i, rev ? 0 : 2];
            int mid = rows[dir, i, 1];
            int last = rows[dir, i, rev ? 2 : 0];
            if (spaces[mid].block != null) {
                if (spaces[first].block == null) {
                    MoveBlock(mid, first, false);
                }
                else if (spaces[first].block.value == spaces[mid].block.value) {
                    MoveBlock(mid, first, true);
                    alreadyMerged = true;
                }
            }
            if (spaces[last].block != null) {
                if (spaces[mid].block != null) {
                    if (spaces[mid].block.value == spaces[last].block.value) {
                        MoveBlock(last, mid, true);
                    }
                }
                else if (spaces[first].block != null) {
                    if (spaces[first].block.value == spaces[last].block.value && !alreadyMerged) {
                        MoveBlock(last, first, true);
                    }
                    else {
                        MoveBlock(last, mid, false);
                    }
                }
                else {
                    MoveBlock(last, first, false);
                }
            }
        }
        if (hasMoved) {
            StartCoroutine(WaitAndSpawn());
        }
    }

    void MoveBlock(int from, int to, bool merge) {
        hasMoved = true;
        Block fromBlock = spaces[from].block;
        spaces[from].block = null;
        fromBlock.Move(spaces[to].pos, merge);
        if (merge) {
            spaces[to].block.Merge();
            blockPool.Enqueue(fromBlock);
        }
        else {
            spaces[to].block = fromBlock;
        }
    }

    IEnumerator WaitAndSpawn() {
        isReady = false;
        yield return new WaitForSeconds(0.2f);
        AddBlock();
        isReady = true;
    }
}
