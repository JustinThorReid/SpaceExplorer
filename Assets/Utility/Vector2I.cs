using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Vector2I {
    public int x, y;

    public Vector2I(Vector2I other) {
        this.x = other.x;
        this.y = other.y;
    }

    public Vector2I(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public Vector2I() {
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

    public Vector2 floatVal {
        get {
            return new Vector2(x, y);
        }
    }

    public override int GetHashCode() {
        return x ^ y;
    }

    public String ToString() {
        return "[" + x + "," + y + "]";
    }
}

