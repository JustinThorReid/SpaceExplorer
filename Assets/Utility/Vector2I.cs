using System;
using UnityEngine;

[System.Serializable]
public struct Vector2I
{
    public static readonly Vector2I UP = new Vector2I(0, 1);
    public static readonly Vector2I RIGHT = new Vector2I(1, 0);
    public static readonly Vector2I DOWN = new Vector2I(0, -1);
    public static readonly Vector2I LEFT = new Vector2I(-1, 0);
    public static readonly Vector2I[] DIRECTIONS = { UP, RIGHT, DOWN, LEFT };

    public static readonly Vector2I ONE = new Vector2I(1, 1);
    public static readonly Vector2I ZERO = new Vector2I(0, 0);

    public static readonly sbyte DIR_UP = 0;
    public static readonly sbyte DIR_RIGHT = 1;
    public static readonly sbyte DIR_DOWN = 2;
    public static readonly sbyte DIR_LEFT = 3;

    public static sbyte RotateDirection(int dir, int amount) {
        dir += amount;
        return (sbyte)(((dir % 4) + 4) % 4); // Because of negative 
    }

    public int x, y;

    public Vector2I(Vector2I other) {
        this.x = other.x;
        this.y = other.y;
    }

    public Vector2I(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public Vector2I(Vector2 v) {
        x = Mathf.RoundToInt(v.x);
        y = Mathf.RoundToInt(v.y);
    }
    public Vector2I(float ix, float iy) {
        x = Mathf.RoundToInt(ix);
        y = Mathf.RoundToInt(iy);
    }
    public static Vector2I Truncate(Vector2 v) {
        return new Vector2I((int)v.x, (int)v.y);
    }

    /// <summary>
    /// Rotate this vector by a multiple of 90 degrees negavtive or positive
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public void Rotate(int amount) {
        amount = ((amount % 4) + 4) % 4;

        switch(amount) {
            case 0:
                return;
            case 1:
                int t = x;
                x = y;
                y = -t;
                return;
            case 2:
                x = -x;
                y = -y;
                return;
            case 3:
                int t2 = x;
                x = -y;
                y = t2;
                return;
        }
    }

    public Vector2I Rotated(int amount) {
        Vector2I result = new Vector2I(this);
        result.Rotate(amount);
        return result;
    }

    public static Vector2I operator +(Vector2I a, Vector2I b) {
        return new Vector2I(a.x + b.x, a.y + b.y);
    }
    public static Vector2 operator +(Vector2I a, Vector2 b) {
        return new Vector2(a.x + b.x, a.y + b.y);
    }
    public static Vector2I operator -(Vector2I a, Vector2I b) {
        return new Vector2I(a.x - b.x, a.y - b.y);
    }
    public static Vector2 operator -(Vector2I a, Vector2 b) {
        return new Vector2(a.x - b.x, a.y - b.y);
    }

    public static Vector2 operator *(Vector2I a, float b) {
        return new Vector2(a.x * b, a.y * b);
    }
    public static Vector2I operator *(Vector2I a, int b) {
        return new Vector2I(a.x * b, a.y * b);
    }
    public static Vector2I operator *(Vector2I a, uint b) {
        return new Vector2I(a.x * b, a.y * b);
    }

    public static Vector2 operator /(Vector2I a, float b) {
        return new Vector2(a.x / b, a.y / b);
    }
    public static Vector2I operator /(Vector2I a, int b) {
        return new Vector2I(a.x / b, a.y / b);
    }
    public static Vector2I operator /(Vector2I a, uint b) {
        return new Vector2I(a.x / b, a.y / b);
    }

    public static bool operator ==(Vector2I a, Vector2I b) {
        if(object.ReferenceEquals(a, null) ^ object.ReferenceEquals(b, null)) // One is null and not the other
            return false;
        if(object.ReferenceEquals(a, null)) // They are the same so if a is null so is b
            return true;
        return a.x == b.x && a.y == b.y;
    }

    public override bool Equals(System.Object b) {
        if(b is Vector2I) {
            Vector2I other = (Vector2I)b;
            return x == other.x && y == other.y;
        }

        return false;
    }
    public static bool operator !=(Vector2I a, Vector2I b) {
        return !(a == b);
    }

    public int SqrMagnitude() {
        return x * x + y * y;
    }
    public double magnitude() {
        return Math.Sqrt(SqrMagnitude());
    }

    public Vector2 floatVal
    {
        get {
            return new Vector2(x, y);
        }
    }

    public override int GetHashCode() {
        return x ^ y;
    }

    public override String ToString() {
        return "[" + x + "," + y + "]";
    }
}

