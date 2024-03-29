using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.Tilemaps;
using UnityEngine.Sprites;

// TR - top-right, TL - top-left, BL - bottom-left, BR - bottom-right, N - none,
// EL - edge-left, ET - edge-top, ER - edge-right, EB - edge-bottom,
// H - horizontal (left = 1, right = 1), V - vertical (top = 1, bottom = 1)
public enum CurveCellType { TR, TL, BL, BR, N, EL, ET, ER, EB, H, V }

public class LevelGenerator : MonoBehaviour
{
    public Tile t1;
    public Tile t2;

    public Tilemap tm;


    int[,] generatedMap;

    public int width;
    public int height;

    public string seed;
    private System.Random pseudoRandom;

    [Range(0, 100)]
    public int fillPercentage;

    //Hilbert Curve:
    Vector2[] hilbertPts;
    public int hilbertOrder;
    public int hilbertSize;
    int hilbertIndex;

    [SerializeField]
    int shiftX = 0, shiftY = 0;

    int[,] hilbertPointsInt;

    public int pathWidth;

    //Negative path 
    int[,] hilbertNegativePointsInt;
    public int negativePathGirth;

    ArrayList curvePoints;
    struct CurvePoint
    {
        public int x;
        public int y;
        public CurveCellType type;
    }

    Dictionary<Vector2, ArrayList> segments;

    public bool isDisplayNegativePath;
    public bool isDisplayGuidelines;

    private void Awake()
    {
        curvePoints = new ArrayList();
        segments = new Dictionary<Vector2, ArrayList>();
        CellularAutomata();
    }

    void CellularAutomata()
    {
        seed = (seed.Length <= 0) ? Time.time.ToString() : seed;
        pseudoRandom = new System.Random(seed.GetHashCode());

        hilbertPts = new Vector2[(int)Mathf.Pow(4, hilbertOrder)];
        hilbertIndex = 0;

        HilbertCurve(0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                     0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                     hilbertOrder);

        GenerateGuidelines();
        GenerateMap();

        for (int i = 0; i < 5; i++)
            SmoothMap();

        RemoveUnreachableCells();
        RestoreEdge();
    }

    void GenerateMap()
    {
        generatedMap = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                generatedMap[x, y] = (pseudoRandom.Next(0, 100) < fillPercentage) ? 1 : 0;
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                //Rules CA
                // T > 4 => C = true
                // T = 4 => C = C
                // T < 4 => C = false
                int neighbours = getNeighbours(x, y, generatedMap);

                if (neighbours > 4)
                    generatedMap[x, y] = 1;
                else if (neighbours < 4)
                    generatedMap[x, y] = 0;

                generatedMap[x, y] = (hilbertNegativePointsInt[x, y] == 1) ? 0 : generatedMap[x, y];

                if (hilbertPointsInt[x, y] == 1)
                {
                    generatedMap[x, y] = 1;
                    if (neighbours > 4)
                        FloodFillNeighboursWithValue(x, y, 1);
                }
            }
        }
    }

    int getNeighbours(int x, int y, int[,] m)
    {
        int neighbours = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                neighbours += m[i + x, j + y];
            }
        }
        neighbours -= m[x, y];

        return neighbours;
    }

    void RestoreEdge()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    generatedMap[x, y] = 0;
            }
        }
    }

    void RemoveUnreachableCells()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                generatedMap[x, y] = (getNeighbours(x, y, generatedMap) <= 0) ? 0 : generatedMap[x, y];
            }
        }
    }

    void HilbertCurve(float x, float y, float xi, float xj, float yi, float yj, int n)
    {
        /*
         * def hilbert(x0, y0, xi, xj, yi, yj, n):
         *   if n <= 0:
         *     X = x0 + (xi + yi)/2
         *     Y = y0 + (xj + yj)/2
         *
         *   # Output the coordinates of the cv
         *   print("%s %s 0" % (X, Y))
         *   else:
         *     hilbert(x0,               y0,               yi/2, yj/2, xi/2, xj/2, n - 1)
         *     hilbert(x0 + xi/2,        y0 + xj/2,        xi/2, xj/2, yi/2, yj/2, n - 1)
         *     hilbert(x0 + xi/2 + yi/2, y0 + xj/2 + yj/2, xi/2, xj/2, yi/2, yj/2, n - 1)
         *     hilbert(x0 + xi/2 + yi,   y0 + xj/2 + yj,  -yi/2,-yj/2,-xi/2,-xj/2, n - 1)
         */

        if (n <= 0)
        {
            float X = x + (xi + yi) / 2;
            float Y = y + (xj + yj) / 2;


            hilbertPts[hilbertIndex] = new Vector2((int)X * hilbertSize + shiftX, (int)Y * hilbertSize + shiftY);
            hilbertIndex++;
        }
        else
        {
            HilbertCurve(x, y, yi / 2, yj / 2, xi / 2, xj / 2, n - 1);
            HilbertCurve(x + xi / 2, y + xj / 2, xi / 2, xj / 2, yi / 2, yj / 2, n - 1);
            HilbertCurve(x + xi / 2 + yi / 2, y + xj / 2 + yj / 2, xi / 2, xj / 2, yi / 2, yj / 2, n - 1);
            HilbertCurve(x + xi / 2 + yi, y + xj / 2 + yj, -yi / 2, -yj / 2, -xi / 2, -xj / 2, n - 1);
        }
    }

    void GenerateGuidelines()
    {
        // 4 points per one curve segment chunk
        hilbertPts = new Vector2[(int)Mathf.Pow(4, hilbertOrder)];
        hilbertPointsInt = new int[Mathf.Max(width, height), Mathf.Max(width, height)];
        hilbertIndex = 0;
        shiftX = pseudoRandom.Next(-Mathf.Max(width, height) * (hilbertSize - 1), 0);
        shiftY = pseudoRandom.Next(-Mathf.Max(width, height) * (hilbertSize - 1), 0);

        hilbertNegativePointsInt = new int[width, height];

        HilbertCurve(0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                     0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                     hilbertOrder);

        // clear curve's grid by setting all cells to 0
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                hilbertPointsInt[x, y] = 0;
            }
        }

        // hilbert curve connection nodes
        for (int i = 0; i < hilbertPts.Length; i++)
        {
            int x = (int)hilbertPts[i].x;
            int y = (int)hilbertPts[i].y;

            if (x < Mathf.Max(width, height) && x >= 0 &&
                y < Mathf.Max(width, height) && y >= 0)
            {
                hilbertPointsInt[x, y] = 1;
            }
        }

        // filling in all cells between hilbert curve nodes
        for (int i = 0; i < hilbertPts.Length - 1; i++)
        {
            int x_curr = (int)hilbertPts[i].x;
            int y_curr = (int)hilbertPts[i].y;
            int x_next = (int)hilbertPts[i + 1].x;
            int y_next = (int)hilbertPts[i + 1].y;
            int x_dist = (int)Mathf.Abs(x_curr - x_next);
            int y_dist = (int)Mathf.Abs(y_curr - y_next);

            Vector2 dir = (hilbertPts[i + 1] - hilbertPts[i]).normalized;

            if (x_dist == 0 && y_dist > 0)
            {
                for (int y = 0; y < y_dist; y++)
                {
                    if (x_curr >= 0 && x_curr < Mathf.Max(width, height) &&
                        y_curr + ((int)dir.y * y) >= 0 && y_curr + ((int)dir.y * y) < Mathf.Max(width, height))
                    {
                        hilbertPointsInt[x_curr, y_curr + ((int)dir.y * y)] = 1;
                    }
                }
            }
            else if (x_dist > 0 && y_dist == 0)
            {
                for (int x = 0; x < x_dist; x++)
                {
                    if (x_curr + ((int)dir.x * x) >= 0 && x_curr + ((int)dir.x * x) < Mathf.Max(width, height) &&
                        y_curr >= 0 && y_curr < Mathf.Max(width, height))
                    {
                        hilbertPointsInt[x_curr + ((int)dir.x * x), y_curr] = 1;
                    }
                }
            }
        }

        // filling in all cells between hilbert curve nodes
        for (int i = 0; i < hilbertPts.Length - 1; i++)
        {
            ImprintPathBetweenCells(hilbertPts[i], hilbertPts[i + 1], hilbertPointsInt);
        }

        CreateCurveCorners();
        CreateSegments();
        CreateCriticalPath();
        CreateNegativePath();

    }

    void FloodFillNeighboursWithValue(int x, int y, int val)
    {
        for (int i = -pathWidth; i <= pathWidth; i++)
        {
            for (int j = -pathWidth; j <= pathWidth; j++)
            {
                int i_x = i + x;
                int j_y = j + y;

                if (i_x < 0 || j_y < 0 || i_x > width - 1 || j_y > height - 1)
                    continue;

                generatedMap[i_x, j_y] = val;
            }
        }
    }

    int GetNeighboursNegativePathCellCount(int x, int y)
    {
        int neighbors = 0;
        for (int i = -negativePathGirth; i <= negativePathGirth; i++)
        {
            for (int j = -negativePathGirth; j <= negativePathGirth; j++)
            {
                int i_x = i + x;
                int j_y = j + y;

                if (i_x <= 0 || j_y <= 0 || i_x > width - 1 || j_y > height - 1)
                    neighbors += 1;
                else
                    neighbors += hilbertPointsInt[i_x, j_y];
            }
        }

        neighbors -= hilbertPointsInt[x, y];

        return neighbors;
    }

    void CreateNegativePath()
    {
        // creating 'negative' curve map
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                hilbertNegativePointsInt[x, y] = (hilbertPointsInt[x, y] == 1) ? 0 : 1;
            }
        }

        // trim 'negative' curve map
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int neighbors = GetNeighboursNegativePathCellCount(x, y);
                hilbertNegativePointsInt[x, y] = (neighbors > 0) ? 0 : hilbertNegativePointsInt[x, y];
            }
        }
    }

    CurvePoint GetCurvePoint(int x, int y)
    {
        int x_left = x - 1;
        int x_right = x + 1;

        int y_bottom = y - 1;
        int y_top = y + 1;

        CurvePoint cell = new CurvePoint();
        cell.x = x;
        cell.y = y;
        cell.type = CurveCellType.N;

        if (y_bottom < 0)
        {
            // EB - edge-bottom
            if (y_bottom == -1 &&
                hilbertPointsInt[x, y] == 1 &&
                hilbertPointsInt[x_right, y] == 0 &&
                hilbertPointsInt[x, y_top] == 1 &&
                hilbertPointsInt[x_left, y] == 0)
            {
                cell.type = CurveCellType.EB;
            }
        }
        else if (x_left < 0)
        {
            // EL - edge-left
            if (x_left == -1 &&
                hilbertPointsInt[x, y] == 1 &&
                hilbertPointsInt[x_right, y] == 1 &&
                hilbertPointsInt[x, y_top] == 0 &&
                hilbertPointsInt[x, y_bottom] == 0)
            {
                cell.type = CurveCellType.EL;
            }
        }
        else if (x_right > width - 1)
        {
            // ER - edge-right
            if (x_right == width &&
                hilbertPointsInt[x, y] == 1 &&
                hilbertPointsInt[x_left, y] == 1 &&
                hilbertPointsInt[x, y_top] == 0 &&
                hilbertPointsInt[x, y_bottom] == 0)
            {
                cell.type = CurveCellType.ER;
            }
        }
        else if (y_top > height - 1)
        {
            // ET - edge-top
            if (y_top == height &&
                hilbertPointsInt[x, y] == 1 &&
                hilbertPointsInt[x_left, y] == 0 &&
                hilbertPointsInt[x, y_top] == 0 &&
                hilbertPointsInt[x, y_bottom] == 1)
            {
                cell.type = CurveCellType.ET;
            }
        }
        // We are dealing with 'edge' cell
        else if (x_left == 0 || x_right == width - 1 ||
                 y_bottom == 0 || y_top == height - 1)
        {
            // ER - edge-right
            if (x_right + 1 == width &&
                hilbertPointsInt[x_right, y] == 1 &&
                hilbertPointsInt[x, y] == 1 &&
                hilbertPointsInt[x, y_top] == 0 &&
                hilbertPointsInt[x, y_bottom] == 0)
            {
                cell.type = CurveCellType.ER;
            }
            // EL - edge-left
            else if (x_left == 0 &&
                     hilbertPointsInt[x_left, y] == 1 &&
                     hilbertPointsInt[x, y] == 1 &&
                     hilbertPointsInt[x, y_top] == 0 &&
                     hilbertPointsInt[x, y_bottom] == 0)
            {
                cell.type = CurveCellType.EL;
            }
            // ET - edge-top
            else if (y_top + 1 == height &&
                     hilbertPointsInt[x_left, y] == 0 &&
                     hilbertPointsInt[x_right, y] == 0 &&
                     hilbertPointsInt[x, y] == 1 &&
                     hilbertPointsInt[x, y_bottom] == 1)
            {
                cell.type = CurveCellType.ET;
            }
            // EB - edge-bottom
            else if (y_bottom == 0 &&
                     hilbertPointsInt[x_left, y] == 0 &&
                     hilbertPointsInt[x_right, y] == 0 &&
                     hilbertPointsInt[x, y_top] == 1 &&
                     hilbertPointsInt[x, y] == 1)
            {
                cell.type = CurveCellType.EB;
            }
        }
        else if (x_left > 0 && x_right < width - 1 &&
                 y_bottom > 0 && y_top < height - 1)
        {
            // H - horizontal
            if (hilbertPointsInt[x_left, y] == 1 &&
                hilbertPointsInt[x_right, y] == 1 &&
                hilbertPointsInt[x, y_top] == 0 &&
                hilbertPointsInt[x, y_bottom] == 0)
            {
                cell.type = CurveCellType.H;
            }
            // V - vertical
            else if (hilbertPointsInt[x_left, y] == 0 &&
                     hilbertPointsInt[x_right, y] == 0 &&
                     hilbertPointsInt[x, y_top] == 1 &&
                     hilbertPointsInt[x, y_bottom] == 1)
            {
                cell.type = CurveCellType.V;
            }
            // TL - top-left
            else if (hilbertPointsInt[x_left, y] == 1 &&
                     hilbertPointsInt[x_right, y] == 0 &&
                     hilbertPointsInt[x, y_top] == 0 &&
                     hilbertPointsInt[x, y_bottom] == 1)
            {
                cell.type = CurveCellType.TL;
            }
            // TR - top-right
            else if (hilbertPointsInt[x_left, y] == 0 &&
                     hilbertPointsInt[x_right, y] == 1 &&
                     hilbertPointsInt[x, y_top] == 0 &&
                     hilbertPointsInt[x, y_bottom] == 1)
            {
                cell.type = CurveCellType.TR;
            }
            // BL - bottom-left
            else if (hilbertPointsInt[x_left, y] == 1 &&
                     hilbertPointsInt[x_right, y] == 0 &&
                     hilbertPointsInt[x, y_top] == 1 &&
                     hilbertPointsInt[x, y_bottom] == 0)
            {
                cell.type = CurveCellType.BL;
            }
            // BR - bottom-right
            else if (hilbertPointsInt[x_left, y] == 0 &&
                     hilbertPointsInt[x_right, y] == 1 &&
                     hilbertPointsInt[x, y_top] == 1 &&
                     hilbertPointsInt[x, y_bottom] == 0)
            {
                cell.type = CurveCellType.BR;
            }
        }

        return cell;
    }

    void CreateCurveCorners()
    {
        // corners of the curve
        curvePoints.Clear();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                CurvePoint cPoint = GetCurvePoint(x, y);

                if (cPoint.type.Equals(CurveCellType.N) ||
                    cPoint.type.Equals(CurveCellType.H) ||
                    cPoint.type.Equals(CurveCellType.V))
                    continue;

                curvePoints.Add(cPoint);
            }
        }
    }

    void CreateSegments()
    {
        // create path's segments
        segments.Clear();
        ArrayList endEdgePoints = new ArrayList();          // to contain ending edge point so that we can skip them

        for (int i = 0; i < curvePoints.Count; i++)
        {
            CurvePoint cPoint = (CurvePoint)curvePoints[i];
            Vector2Int ePoint = new Vector2Int(cPoint.x, cPoint.y);

            if (endEdgePoints.Contains(ePoint))
                continue;

            if (cPoint.type.Equals(CurveCellType.EL) ||
                cPoint.type.Equals(CurveCellType.EB))
            {
                ArrayList segmentPoints = new ArrayList();
                Vector2Int point = ePoint;

                while (!segments.ContainsKey(ePoint))
                {
                    Vector2Int right = new Vector2Int(point.x + 1, point.y);
                    Vector2Int left = new Vector2Int(point.x - 1, point.y);
                    Vector2Int top = new Vector2Int(point.x, point.y + 1);
                    Vector2Int bottom = new Vector2Int(point.x, point.y - 1);

                    // go right!
                    if (hilbertPointsInt[right.x, right.y] == 1 &&
                       !segmentPoints.Contains(right))
                    {
                        point = right;
                        segmentPoints.Add(point);
                    }
                    // go up!
                    else if (hilbertPointsInt[top.x, top.y] == 1 &&
                            !segmentPoints.Contains(top))
                    {
                        point = top;
                        segmentPoints.Add(point);
                    }
                    // go left!
                    else if (hilbertPointsInt[left.x, left.y] == 1 &&
                             !segmentPoints.Contains(left))
                    {
                        point = left;
                        segmentPoints.Add(point);
                    }
                    // go down!
                    else if (hilbertPointsInt[bottom.x, bottom.y] == 1 &&
                            !segmentPoints.Contains(bottom))
                    {
                        point = bottom;
                        segmentPoints.Add(point);
                    }

                    CurvePoint curvePoint = GetCurvePoint(point.x, point.y);

                    if (curvePoint.type.Equals(CurveCellType.EB) ||
                        curvePoint.type.Equals(CurveCellType.EL) ||
                        curvePoint.type.Equals(CurveCellType.ER) ||
                        curvePoint.type.Equals(CurveCellType.ET) ||
                        getNeighbours(point.x, point.y, hilbertPointsInt) == 1)
                    {
                        endEdgePoints.Add(point);   // last point in a segment
                        segments.Add(ePoint, segmentPoints);
                    }
                }
            }
            else if (cPoint.type.Equals(CurveCellType.ET) ||
                     cPoint.type.Equals(CurveCellType.ER))
            {
                ArrayList segmentPoints = new ArrayList();
                Vector2Int point = ePoint;

                while (!segments.ContainsKey(ePoint))
                {
                    Vector2Int right = new Vector2Int(point.x + 1, point.y);
                    Vector2Int left = new Vector2Int(point.x - 1, point.y);
                    Vector2Int top = new Vector2Int(point.x, point.y + 1);
                    Vector2Int bottom = new Vector2Int(point.x, point.y - 1);

                    // go left!
                    if (hilbertPointsInt[left.x, left.y] == 1 &&
                        !segmentPoints.Contains(left))
                    {
                        point = left;
                        segmentPoints.Add(point);
                    }
                    // go down!
                    else if (hilbertPointsInt[bottom.x, bottom.y] == 1 &&
                            !segmentPoints.Contains(bottom))
                    {
                        point = bottom;
                        segmentPoints.Add(point);
                    }

                    // go right!
                    else if (hilbertPointsInt[right.x, right.y] == 1 &&
                            !segmentPoints.Contains(right))
                    {
                        point = right;
                        segmentPoints.Add(point);
                    }
                    // go up!
                    else if (hilbertPointsInt[top.x, top.y] == 1 &&
                            !segmentPoints.Contains(top))
                    {
                        point = top;
                        segmentPoints.Add(point);
                    }

                    CurvePoint curvePoint = GetCurvePoint(point.x, point.y);

                    if (curvePoint.type.Equals(CurveCellType.EB) ||
                        curvePoint.type.Equals(CurveCellType.EL) ||
                        curvePoint.type.Equals(CurveCellType.ER) ||
                        curvePoint.type.Equals(CurveCellType.ET) ||
                        getNeighbours(point.x, point.y, hilbertPointsInt) == 1)
                    {
                        endEdgePoints.Add(point);   // last point in a segment
                        segments.Add(ePoint, segmentPoints);
                    }
                }
            }
        }
    }

    void CreateCriticalPath()
    {
        // if we have more than one segments, let's connect them!
        if (segments != null && segments.Count > 1)
        {
            Dictionary<string, List<KeyValuePair<Vector2Int, Vector2Int>>> list = new Dictionary<string, List<KeyValuePair<Vector2Int, Vector2Int>>>();

            for (int cnt = 0; cnt < segments.Count - 1; cnt++)
            {
                ArrayList cellsA = segments.ElementAt(cnt).Value;
                List<KeyValuePair<Vector2Int, Vector2Int>> segmentConnectors = new List<KeyValuePair<Vector2Int, Vector2Int>>();
                int i = 0;

                do
                {
                    i++;
                    ArrayList cellsB = segments.ElementAt(cnt + i).Value;
                    segmentConnectors = FindConnectorsBetweenSegments(cellsA, cellsB, segmentConnectors);
                }
                while (segmentConnectors.Count == 0);

                string key = cnt + "_" + i;
                list.Add(key, segmentConnectors);
            }

            foreach (KeyValuePair<string, List<KeyValuePair<Vector2Int, Vector2Int>>> item in list)
            {
                List<KeyValuePair<Vector2Int, Vector2Int>> connectors = item.Value;

                if (connectors.Count < 1) continue;

                int rand = pseudoRandom.Next(0, connectors.Count);
                Vector2Int pointFrom = (Vector2Int)connectors.ElementAt(rand).Key;
                Vector2Int pointTo = (Vector2Int)connectors.ElementAt(rand).Value;

                ImprintPathBetweenCells(pointFrom, pointTo, hilbertPointsInt);
            }
        }
    }

    bool AreCellsLeveled(Vector2 pointA, Vector2 pointB)
    {
        // for verifying if two points are connectable
        // we check whether they are on the same x or y axis
        if (pointA.x == pointB.x || pointA.y == pointB.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    List<KeyValuePair<Vector2Int, Vector2Int>> FindConnectorsBetweenSegments(
        ArrayList cellsA, ArrayList cellsB,
        List<KeyValuePair<Vector2Int, Vector2Int>> segmentConnectors)
    {
        for (int i = 0; i < cellsA.Count; i++)
        {
            for (int j = 0; j < cellsB.Count; j++)
            {
                Vector2Int posA = (Vector2Int)cellsA[i];
                Vector2Int posB = (Vector2Int)cellsB[j];
                int x_dist = (int)Mathf.Abs(posA.x - posB.x);
                int y_dist = (int)Mathf.Abs(posA.y - posB.y);
                Vector2 dir = ((Vector2)posB - (Vector2)posA).normalized;

                if (AreCellsLeveled(posA, posB))
                {
                    if (x_dist == 0 && y_dist > 0)
                    {
                        if (hilbertPointsInt[posA.x, posA.y + (int)dir.y] == 1)
                            continue;
                    }
                    else if (x_dist > 0 && y_dist == 0)
                    {
                        if (hilbertPointsInt[posA.x + (int)dir.x, posA.y] == 1)
                            continue;
                    }
                    segmentConnectors.Add(new KeyValuePair<Vector2Int, Vector2Int>(posA, posB));
                }
            }
        }

        return segmentConnectors;
    }

    void ImprintPathBetweenCells(Vector2 pointFrom, Vector2 pointTo, int[,] map)
    {
        // filling in all cells between hilbert curve nodes
        int x_curr = (int)pointFrom.x;
        int y_curr = (int)pointFrom.y;
        int x_next = (int)pointTo.x;
        int y_next = (int)pointTo.y;
        int x_dist = (int)Mathf.Abs(x_curr - x_next);
        int y_dist = (int)Mathf.Abs(y_curr - y_next);

        Vector2 dir = ((Vector2)pointTo - (Vector2)pointFrom).normalized;

        if (x_dist == 0 && y_dist > 0)
        {
            for (int y = 0; y < y_dist; y++)
            {
                if (x_curr >= 0 && x_curr < Mathf.Max(width, height) &&
                    y_curr + ((int)dir.y * y) >= 0 &&
                    y_curr + ((int)dir.y * y) < Mathf.Max(width, height))
                {
                    map[x_curr, y_curr + ((int)dir.y * y)] = 1;
                }
            }
        }
        else if (x_dist > 0 && y_dist == 0)
        {
            for (int x = 0; x < x_dist; x++)
            {
                if (x_curr + ((int)dir.x * x) >= 0 &&
                    x_curr + ((int)dir.x * x) < Mathf.Max(width, height) &&
                    y_curr >= 0 && y_curr < Mathf.Max(width, height))
                {
                    map[x_curr + ((int)dir.x * x), y_curr] = 1;
                }
            }
        }
    }


    void FixedUpdate()
    {
        if (generatedMap != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (generatedMap[x, y] != 1)
                    {
                        Tile color = t1;
                        Vector3Int pos = new Vector3Int(x + 1, y + 1, 0);
                        tm.SetTile(pos, color);
                    }

                }
            }
        }
    }

}
