struct Payload
{
    float4 color;
};

[shader("miss")]
void main(inout Payload payload)
{
    payload.color = float4(0.0, 0.0, 0.2, 1.0);
}
