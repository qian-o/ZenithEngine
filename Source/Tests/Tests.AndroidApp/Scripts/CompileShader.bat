@echo off
setlocal enabledelayedexpansion

set SHADERS_DIR="Assets\Shaders"

for %%f in (%SHADERS_DIR%\*.hlsl) do (
    echo ���� %%f ...
    dxc -T lib_6_4 -spirv -fvk-use-scalar-layout -fspv-target-env=vulkan1.3 -Fo %%f.spv %%f
    if !errorlevel! neq 0 (
        echo ���� %%f ʧ��
        exit /b !errorlevel!
    )
)

echo �����ļ��������

endlocal