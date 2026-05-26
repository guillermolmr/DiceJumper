void VoronoiRandom_float(float2 uv, float angleOffset, float cellCount,
                         out float random, out float dist, out float2 cellUV)
{
    uv *= cellCount;

    float2 i = floor(uv);
    float2 f = frac(uv);

    float minDist = 8.0;
    float2 closestCell = float2(0, 0);
    float2 closestPoint = float2(0, 0);

    for (int y = -1; y <= 1; y++)
    for (int x = -1; x <= 1; x++)
    {
        float2 neighbor = float2(x, y);
        float2 cellID   = i + neighbor;

        float2 rnd = frac(sin(float2(dot(cellID, float2(127.1, 311.7)),
                                     dot(cellID, float2(269.5, 183.3)))) * 43758.5453);

        // Hash extra para el offset de ángulo, usando constantes distintas
        float rndAngle = frac(sin(dot(cellID, float2(419.2, 371.9))) * 43758.5453);

        float angle = rnd.x * 6.2831853 + rndAngle * angleOffset;
        float2 featurePoint = neighbor + float2(cos(angle), sin(angle)) * 0.5 * rnd.y;
        float2 p = featurePoint - f;

        float d = dot(p, p);
        if (d < minDist)
        {
            minDist      = d;
            closestCell  = cellID;
            closestPoint = featurePoint;
        }
    }

    float2 cellRnd = frac(sin(float2(dot(closestCell, float2(127.1, 311.7)),
                                     dot(closestCell, float2(269.5, 183.3)))) * 43758.5453);
    random  = cellRnd.x;
    dist    = sqrt(minDist);
    cellUV  = (f - closestPoint) * 2.0 + 0.5;
}