using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeLiquid : MonoBehaviour
{
    [SerializeField] private float FILLDURATION = 5f;
    [SerializeField] private LineRenderer lineRenderer;
    private Vector3[] points;
    /// <summary>
    /// array of (p-1) normalized distances between points. 
    /// 
    /// For example, consider points (0,0), (0,3), and (4,0)
    /// Then distance will be [3/8, 5/8]
    /// </summary>
    private float[] distance; 


    private void Awake()
    {
        SavePoints();
        Fill(Vector2.zero);
    }

    private void SavePoints()
    {
        int numPoints = lineRenderer.positionCount;
        points = new Vector3[numPoints];
        lineRenderer.GetPositions(points);

        distance = new float[numPoints - 1];
        float totalDistance = 0;
        for(int i = 0; i < numPoints - 1; i++)
        {
            Vector3 currPoint = points[i];
            Vector3 nextPoint = points[i+1];
            float d = Vector3.Distance(currPoint, nextPoint);
            totalDistance += d;
            distance[i] = d;
        }
        
        for(int i = 0; i < distance.Length; i++)
        {
            distance[i] /= totalDistance;
        }
    }

    private IEnumerator AnimateFill(Vector2 startState, Vector2 endState, float duration)
    {
        Fill(startState);
        float t = 0;
        while (t < duration)
        {
            Vector2 state = Vector2.Lerp(startState, endState, t/duration);
            Fill(state);
            t += Time.deltaTime;
            yield return null;
        }
        Fill(endState);
    }

    private void Fill(Vector2 state)
    {
        Fill(state[0], state[1]);
    }

    //Fills the segment of the pipe (0-1) from start to end.
    private void Fill(float startPos, float endPos)
    {
        if(startPos >= endPos)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        List<Vector3> newPositions = new();

        for(int i = 0; i < distance.Length - 1; i++)
        {
            float currDistance = distance[i];
            float nextDistance = distance[i + 1];
            Vector3 currStart = points[i];
            Vector3 currEnd = points[i + 1];

            if(endPos < currDistance || startPos > nextDistance) //not included
                continue;
            else if(startPos < currDistance) //include start
            {
                newPositions.Add(currStart);
                if(endPos < nextDistance)
                {
                    float t = (endPos - currDistance) / (nextDistance - currDistance);
                    Vector3 v = Vector3.Lerp(currStart, currEnd, t);
                    newPositions.Add(v);
                }
            }
            else if (startPos < nextDistance)
            {
                float t = (startPos - currDistance) / (nextDistance - currDistance);
                Vector3 v = Vector3.Lerp(currStart, currEnd, t);
                newPositions.Add(v);

                if(endPos < nextDistance)
                {
                    float t2 = (endPos - currDistance) / (nextDistance - currDistance);
                    Vector3 v2 = Vector3.Lerp(currStart, currEnd, t2);
                    newPositions.Add(v2);
                }
            }
        }
        
        lineRenderer.positionCount = newPositions.Count;
        lineRenderer.SetPositions(newPositions.ToArray());
    }

    public void FillPipe()
    {
        FillPipe(FILLDURATION);
    }

    public void FillPipe(float duration)
    {
        StartCoroutine(AnimateFill(Vector2.zero, Vector2.up, duration));
    } 
}
