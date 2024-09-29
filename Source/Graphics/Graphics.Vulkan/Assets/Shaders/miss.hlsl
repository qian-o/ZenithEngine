struct RayPayload
{
	float4 color;
};

[shader("miss")]
void main(inout RayPayload payload)
{
	payload.color = float4(0.0, 0.0, 0.2, 1.0);
}