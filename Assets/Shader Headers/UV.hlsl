float Twirl(float2 UV, float2 Center, float Strength, float2 Offset)
{
    float2 delta = UV - Center;
    float angle = Strength * length(delta);
    float x = cos(angle) * delta.x - sin(angle) * delta.y;
    float y = sin(angle) * delta.x + cos(angle) * delta.y;
    return float2(x + Center.x + Offset.x, y + Center.y + Offset.y);
}
