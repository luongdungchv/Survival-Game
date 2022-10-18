#include "Random.cginc"
float worleyNoise(float2 value){
    float2 baseCell = floor(value);

    float minDistToCell = 10;
    [unroll]
    for(int x=-1; x<=1; x++){
        [unroll]
        for(int y=-1; y<=1; y++){
            float2 cell = baseCell + float2(x, y);
            float2 cellPosition = cell + rand2dTo2d(cell);
            float2 toCell = cellPosition - value;
            float distToCell = length(toCell);
            if(distToCell < minDistToCell){
                minDistToCell = distToCell;
            }
        }
    }
    return minDistToCell;
}
float2 unity_voronoi_noise_randomVector (float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}

float worleyNoise(float2 UV, float AngleOffset)
{
    float2 g = floor(UV);
    float2 f = frac(UV);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);
            if(d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                
            }
        }
    }
    return res.x;
}