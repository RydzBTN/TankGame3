using Dreamteck.Splines;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// </summary>

public class TrackAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct WheelPointMapping
    {
        public Transform wheel;
        public int splinePointIndex;
        public Vector3 offset;
    }

    private SplineComputer spline;
    private List<Transform> trackLinks = new List<Transform>();

    

    [SerializeField] private float linkSpacing;
    [SerializeField] private bool reverse;
    [SerializeField] private GameObject linkPrefab;
    [SerializeField] private int linkCount = 40;
    [SerializeField] private float animationSpeed = 0f;

    private float trackLength;
    private float currentOffset = 0f;
    [SerializeField] private List<WheelPointMapping> wheelMappings = new List<WheelPointMapping>();

    private void Awake()
    {
        spline = GetComponent<SplineComputer>();

        trackLength = linkCount * linkSpacing;
        InitializeTrackLinks();
    }


    private void FixedUpdate()
    {
        UpdateSpline();

        currentOffset += (reverse ? -animationSpeed : animationSpeed) * Time.fixedDeltaTime;

        for(int i = 0; i < trackLinks.Count; i++)
        {
            UpdateLinkTransform(i);
        }



    }
    private void InitializeTrackLinks()
    {
        for (int i = 0; i < linkCount; i++)
        {
            Transform newLink = Instantiate(linkPrefab, gameObject.transform).transform;
            trackLinks.Add(newLink);
        }

    }
    private void UpdateLinkTransform(int linkIndex)
    {
        float distance = Mathf.Repeat((linkIndex * linkSpacing + currentOffset), trackLength);
        double percentage = spline.Travel(0.0, distance);
        SplineSample sample = spline.Evaluate(percentage);

        // Ustaw rotację wykorzystując odczytany kierunek do przodu oraz wektor up 
        if (sample.forward != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(sample.forward, sample.up);
            trackLinks[linkIndex].rotation = rotation;
            
        }
        

        // Ustaw pozycję ogniwa
        trackLinks[linkIndex].position = sample.position;
    } 


    /// <summary>
    /// oblicza prędkość obrotu gąsienicy na podstawie prędkości w m/s
    /// </summary>
    /// <param name="speed"></param>
    public void UpdateSpeed(float speed)
    {
        animationSpeed = speed;
    }

    public void UpdateSpline()
    {
        SplinePoint[] points = spline.GetPoints();

        foreach(WheelPointMapping mapping in wheelMappings)
        {
            if (mapping.wheel != null && mapping.splinePointIndex >= 0 && mapping.splinePointIndex < points.Length)
            {
                Vector3 newPos = mapping.wheel.position + mapping.offset;
                points[mapping.splinePointIndex].position = newPos;
            }
        }

        spline.SetPoints(points);
        //trackLength = GetTrackLength();
    }
}
