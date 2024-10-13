@echo off
setlocal enabledelayedexpansion

set SHADERS_DIR="Assets\Shaders"

for %%f in (%SHADERS_DIR%\*vs.hlsl) do (
	echo ���� %%f ...
	dxc -T vs_6_3 -E main -spirv -fvk-use-scalar-layout -fspv-target-env=vulkan1.3 -Fo %%f.spv %%f
	if !errorlevel! neq 0 (
		echo ���� %%f ʧ��
		exit /b !errorlevel!
	)
)

for %%f in (%SHADERS_DIR%\*ps.hlsl) do (
	echo ���� %%f ...
	dxc -T ps_6_3 -E main -spirv -fvk-use-scalar-layout -fspv-target-env=vulkan1.3 -Fo %%f.spv %%f
	if !errorlevel! neq 0 (
		echo ���� %%f ʧ��
		exit /b !errorlevel!
	)
)

echo �����ļ��������

endlocal