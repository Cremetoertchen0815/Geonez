using System;

namespace Nez;

/// <summary>
///     Provides functions for storing and manipulating lines.
/// </summary>
public class Line : ICloneable
{
    private const double QualityThreshold = 0.000000001d;

    // Standard Form - AX + BY = C

    // End points of the line
    private Vector2d m_P1;
    private Vector2d m_P2;

    // Creates an invalid line
    public Line()
    {
        m_P1 = new Vector2d();
        m_P2 = new Vector2d();
        SetUpABC();
    }

    // Creates a line going through the two points
    public Line(Vector2d P1, Vector2d P2)
    {
        m_P1 = P1;
        m_P2 = P2;
        SetUpABC();
    }

    // End point accessors
    public Vector2d P1
    {
        get => m_P1;
        set
        {
            m_P1 = value;
            SetUpABC();
        }
    }

    public Vector2d P2
    {
        get => m_P2;
        set
        {
            m_P2 = value;
            SetUpABC();
        }
    }

    // Standard form variable accessors
    public double A { get; private set; }

    public double B { get; private set; }

    public double C { get; private set; }

    // Clones the line
    public object Clone()
    {
        return new Line(m_P1, m_P2);
    }

    // Sets up the standard form variables from the two points P1 and P2
    private void SetUpABC()
    {
        A = m_P2.Y - m_P1.Y;
        B = m_P1.X - m_P2.X;
        C = A * m_P1.X + B * m_P1.Y;
    }

    // Returns the point that the lines cross and stores into LinesCross if the lines do in fact cross
    public Vector2d Intersect(Line Ln, ref bool LinesCross)
    {
        try
        {
            // Calculate Denominator
            var Det = Ln.A * B - Ln.B * A;
            var Res = new Vector2d((Ln.C * B - Ln.B * C) / Det, (Ln.A * C - A * Ln.C) / Det);
            LinesCross = true;
            return Res;
        }
        catch
        {
            // Lines are parallel (or do not intersect within the range of a double)
            LinesCross = false;
            return new Vector2d();
        }
    }

    public bool DoesSectionIntersect(Line Ln, out Vector2d pt)
    {
        var ret = false;
        pt = Intersect(Ln, ref ret);

        if (!ret) return false;
        return OnSegment(pt) && Ln.OnSegment(pt);
    }

    // Returns if the point is on the line
    public bool OnLine(Vector2d Pt)
    {
        return Math.Abs(A * Pt.X + B * Pt.Y - C) < QualityThreshold;
        // A * PtX + B * PtY = C
    }


    // Returns if the point is on the line segment
    public bool OnSegment(Vector2d Pt)
    {
        if (!OnLine(Pt))
            return false;
        var rX = Math.Round(Pt.X, 10);
        var rY = Math.Round(Pt.Y, 10);
        // See if Pt is within the rectangle created by m_P1 and m_P2 inclusive
        return (Math.Min(m_P1.X, m_P2.X) <= rX) & (Math.Max(m_P1.X, m_P2.X) >= rX) & (Math.Min(m_P1.Y, m_P2.Y) <= rY) &
               (Math.Max(m_P1.Y, m_P2.Y) >= rY);
    }

    // Returns if the point is on the line segment excluding the endpoints
    public bool OnSegmentExclusive(Vector2d Pt)
    {
        if (!OnSegment(Pt))
            return false;
        // See if Pt is equal to an endpoint
        return !((Pt.X == m_P1.X) & (Pt.Y == m_P1.Y) || (Pt.X == m_P2.X) & (Pt.Y == m_P2.Y));
    }

    // Rotates the polygon around Axis by Degrees
    public Line Rotate(float Degrees, Vector2d Axis)
    {
        return new Line(m_P1.Rotate(Degrees, Axis), m_P2.Rotate(Degrees, Axis));
    }
}