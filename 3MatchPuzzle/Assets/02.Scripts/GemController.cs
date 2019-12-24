using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemController : MonoBehaviour
{
    Direction gDirection;

    public enum Direction : int
    {
        NONE,
        UP,
        LEFTUP,
        LEFTDOWN,
        RIGHTUP,
        RIGHTDOWN,
        DOWN,
    }

    public Direction _Direction
    {
        get { return gDirection; }
        set { gDirection = value; }
    }

    public Transform _Parents
    {
        get { return transform.parent; }
        set { transform.parent = value; }
    }

    public Vector2 _LocalPosition
    {
        get { return transform.localPosition; }
        set { transform.localPosition = value; }
    }

    public Vector2 _Position
    {
        get { return transform.position; }
    }

    public Color _Color
    {
        get { return GetComponent<SpriteRenderer>().color; }
    }
}
