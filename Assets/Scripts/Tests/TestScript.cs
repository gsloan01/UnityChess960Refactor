using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestRooks()
    {
        BoardManager.GenerateRandomRooks(out int rook1, out int rook2);
        Assert.AreNotEqual(rook2, rook1);
        Assert.That(Mathf.Abs(rook1 - rook2) > 1, "bruh");
        Assert.That(rook1 <= 7 && rook1 >= 0);
        Assert.That(rook2 <= 7 && rook2 >= 0);
    }

    [Test]
    [TestCase(0, 7)]
    [TestCase(1, 7)]
    [TestCase(2, 7)]
    [TestCase(3, 7)]
    [TestCase(3, 6)]
    [TestCase(3, 5)]
    public void TestKing(int rook1, int rook2)
    {
        BoardManager.GenerateRandomKing(rook1, rook2, out int kingPos);
        Assert.AreNotEqual(rook2, kingPos);
        Assert.AreNotEqual(rook1, kingPos);

        if (rook1 > rook2)
        {
            Assert.That(rook1 > kingPos && rook2 < kingPos);
        }
        else
        {
            Assert.That(rook1 < kingPos && rook2 > kingPos);
        }

    }
    static List<int> bishopTestPieces = new List<int> { 0, 7, 3 };

    [Test]
    public void TestBishops()
    {
        BoardManager.GenerateRandomBishops(bishopTestPieces, out int bishop1, out int bishop2);
        Assert.AreNotEqual(bishop1, bishop2);
        Assert.AreNotEqual(bishop1 % 2, bishop2 % 2);
        Assert.That(bishop1 <= 7 && bishop1 >= 0);
        Assert.That(bishop2 <= 7 && bishop2 >= 0);
    }

    static List<int> knightTestPieces = new List<int> { 0, 7, 3, 4, 5 };

    [Test]
    public void TestKnights()
    {
        
        BoardManager.GenerateRandomKnights(knightTestPieces, out int knight1, out int knight2);
        Assert.AreNotEqual(knight1, knight2);
        Assert.That(knight1 <= 7 && knight1 >= 0);
        Assert.That(knight2 <= 7 && knight2 >= 0);
        Assert.That(!knightTestPieces.Contains(knight1) && !knightTestPieces.Contains(knight2));
    }

    static List<int> queenTestPieces = new List<int> { 0, 7, 3, 4, 5, 1, 6 };

    [Test]
    public void TestQueen()
    {
        BoardManager.GenerateRandomQueen(queenTestPieces, out int queenPos);
        Assert.That(queenPos <= 7 && queenPos >= 0);
        Assert.That(!queenTestPieces.Contains(queenPos));
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
