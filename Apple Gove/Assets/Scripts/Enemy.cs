using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp;
    public int damage;
    public PlayerControl player;

    private BoardManager board;
    private Vector2Int cellPos;
    private Vector2Int prevCellPos;



    public void Spawn(Vector2Int cell) {
        board = GameManager.instance.GetComponentInChildren<BoardManager>();
        GameManager.instance.enemies.Add(this);       //add to GameManager's list
        
        cellPos = cell;
        transform.position = board.CellToWorld(cellPos);
    }

    public void Tick() {
        Vector2Int newCellTarget = cellPos;
        int dir = Random.Range(0, 4);
        
        if (dir == 0) {
            newCellTarget.x += 1;
        } else if (dir == 1) {
            newCellTarget.x -= 1;
        } else if (dir == 2) {
            newCellTarget.y += 1;
        } else {
            newCellTarget.y -= 1;
        }
        
        BoardManager.CellData newCell = board.boardData[newCellTarget.x, newCellTarget.y];
        //move if target cell is valid
        if (newCell.Passable && newCell.ContainedObject == null) {
            MoveTo(newCellTarget);
        }
    }


    public void HPDown(int amount) {
        hp -= amount;
        if (hp < 1) {
            GetComponentInChildren<EDeathEffects>().PlayThenDestroy();
            transform.DetachChildren();

            GameManager.instance.enemies.Remove(this);
            Destroy(gameObject);
        }
        else {
            
            GetComponentInChildren<EDeathEffects>().Play();
        }
    }

    public void MoveTo(Vector2Int newCellTarget) {
        prevCellPos = cellPos;                                                      //move current to previous
        board.boardData[prevCellPos.x, prevCellPos.y].ContainedObject = null;       //remove object presence from previous
        
        cellPos = newCellTarget;                                                    //move next to current
        board.boardData[cellPos.x, cellPos.y].ContainedObject = this.gameObject;    //add object presence to current
        transform.position = board.CellToWorld(cellPos);                            //physically move

        if (player.cellPos == cellPos) {
            player.TouchEnemy(this);
        }
    }

    public void MoveToPrev() {
        MoveTo(prevCellPos);
    }
}
