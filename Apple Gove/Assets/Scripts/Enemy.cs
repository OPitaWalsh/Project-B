using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp;
    public int damage;

    private BoardManager board;
    private Vector2Int cellPos;
    private Vector2Int prevCellPos;



    public void Spawn(Vector2Int cell) {
        board = GameManager.instance.GetComponentInChildren<BoardManager>();
        MoveTo(cell);
    }

    void Update() {
        //
    }


    public void HPDown(int amount) {
        hp -= amount;
        if (hp < 1) { Destroy(gameObject); }
    }

    public void MoveTo(Vector2Int cell) {
        prevCellPos = cellPos;
        cellPos = cell;
        transform.position = board.CellToWorld(cellPos);
    }

    public void MoveToPrev() {
        MoveTo(prevCellPos);
    }
}
