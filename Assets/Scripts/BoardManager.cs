using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    int[,] dirByRotation = {
        {0,1,2,3,4,5 },
        {5,1,0,2,4,3 },
        {3,1,5,0,4,2 },
        {2,1,3,5,4,0 },
        {5,1,3,2,4,0 },
        {3,1,2,0,4,5 },
        {2,1,0,5,4,3 },
        {0,1,5,3,4,2 },
    };

    Space[] spaces;
    Queue<Block> blockPool;
    [SerializeField]
    GameObject blockPrefab;
    [SerializeField]
    CamRotator camRotator;
    [SerializeField]
    Text scoreText, blockText, gameOverScoreText, gameOverBlockText;
    [SerializeField]
    Canvas gameCanvas, gameOverCanvas;
    [SerializeField]
    Button[] rotButtons;

    int[] freeSpaces;
    int rotation;
    bool hasMoved, hasMerged, isReady, isLookingUp;
    int score, bestBlock, colorIndex;

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

     public void StartGame() {
        for (int i = 0; i < 27; i++) {
            Block b = spaces[i].block;
            if (b != null) {
                spaces[i].block = null;
                b.transform.position = new Vector3(1000, 1000, 1000);
                blockPool.Enqueue(b);
            }
        }
        score = 0;
        bestBlock = 1;
        colorIndex = 0;
        UpdateScoreDisplay();
        int num = Random.Range(0, 27);
        Block bl = blockPool.Dequeue();
        spaces[num].block = bl;
        bl.Spawn(spaces[num].pos, 1);
        gameCanvas.enabled = true;
        gameOverCanvas.enabled = false;
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
        int val = Random.Range(0, 10) < 9 ? 1 : 2;
        b.Spawn(spaces[num].pos, val);
        if (bestBlock == 1 && val == 2) {
            bestBlock = 2;
            colorIndex = 1;
            UpdateScoreDisplay();
        }
        if (count == 1 && CheckGameOver()) {
            StartCoroutine(DoGameOver());
        }
    }

    public void MoveClick(int btn) {
        if (isReady) {
            int index = (isLookingUp ? 4 : 0) + rotation;
            Move(dirByRotation[index, btn]);
        }
    }

    public void RotateClick(bool right) {
        if (isReady) {
            if (right) {
                rotation = (rotation + 1) % 4;
                camRotator.Rotate(true, isLookingUp);
            }
            else {               
                rotation = rotation == 0 ? 3 : rotation - 1;
                camRotator.Rotate(false, isLookingUp);
            }
            StartCoroutine(WaitForAnim(false));
        }
    }

    public void VertRotateClick(bool lookUp) {
        if (lookUp != isLookingUp) {
            isLookingUp = lookUp;
            camRotator.VertRotate(lookUp);
            float s = isLookingUp ? -1 : 1;
            rotButtons[0].transform.localScale = new Vector3(-1, s, 1);
            rotButtons[1].transform.localScale = new Vector3(1, s, 1);
            StartCoroutine(WaitForAnim(false));
        }
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
            StartCoroutine(WaitForAnim(true));
        }
    }

    void MoveBlock(int from, int to, bool merge) {
        hasMoved = true;
        Block fromBlock = spaces[from].block;
        spaces[from].block = null;
        fromBlock.Move(spaces[to].pos, merge);
        if (merge) {
            spaces[to].block.Merge();
            int toVal = spaces[to].block.value;
            score += toVal;
            if (toVal > bestBlock) {
                bestBlock = toVal;
                colorIndex++;
            }
            blockPool.Enqueue(fromBlock);
            hasMerged = true;
        }
        else {
            spaces[to].block = fromBlock;
        }
    }

    IEnumerator WaitForAnim(bool spawnAfterWait) {
        isReady = false;
        yield return new WaitForSeconds(0.2f);
        if (spawnAfterWait)
            AddBlock();
        if (hasMerged) {
            UpdateScoreDisplay();
        }
        isReady = true;
    }

    void UpdateScoreDisplay() {
        scoreText.text = score.ToString();
        blockText.text = bestBlock.ToString();
        blockText.color = Block.colors[colorIndex];
    }

    bool CheckGameOver() {
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 9; j++) {
                int valA = spaces[rows[i, j, 0]].block.value,
                    valB = spaces[rows[i, j, 1]].block.value,
                    valC = spaces[rows[i, j, 2]].block.value;
                if (valA == valB || valB == valC)
                    return false;
            }
        }
        return true;
    }

    IEnumerator DoGameOver() {
        isReady = false;
        yield return new WaitForSeconds(0.5f);
        gameOverScoreText.text = score.ToString();
        gameOverBlockText.text = bestBlock.ToString();
        gameCanvas.enabled = false;
        gameOverCanvas.enabled = true;
    }
}
