using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestVector2I
{

    // A Test behaves as an ordinary method
    [Test]
    public void TestRotation() {
        Vector2I original = new Vector2I(0, 1);

        NUnit.Framework.Assert.AreEqual(original.Rotated(0), original);
        Assert.AreEqual(original.Rotated(1), new Vector2I(1, 0));
        Assert.AreEqual(original.Rotated(2), new Vector2I(0, -1));
        Assert.AreEqual(original.Rotated(3), new Vector2I(-1, 0));
        Assert.AreEqual(original.Rotated(4), original);
        Assert.AreEqual(original.Rotated(-1), new Vector2I(-1, 0));
        Assert.AreEqual(original.Rotated(-2), new Vector2I(0, -1));
        Assert.AreEqual(original.Rotated(-3), new Vector2I(1, 0));
        Assert.AreEqual(original.Rotated(-4), original);

        original.Rotate(0);
        Assert.AreEqual(original, new Vector2I(0, 1));
        original.Rotate(1);
        Assert.AreEqual(original, new Vector2I(1, 0));
        original.Rotate(-1);
        Assert.AreEqual(original, new Vector2I(0, 1));
    }
}
