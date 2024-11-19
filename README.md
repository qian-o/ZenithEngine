﻿# Zenith Engine

## Introduction
Zenith Engine is a modern, cross-platform graphics rendering engine written in C#.

## Graphics Backends
| API        | Supported |
| ---        | --------- |
| D3D12      | 🚧 |
| Vulkan     | 🚧 |

## UI Backends
| Platform | Supported |
| -------- | --------- |
| WPF      | 🚧 |
| WinUI    | 🚧 |
| Avalonia | 🚧 |
| MAUI     | 🚧 |

## TODO
- ZenithEngine
	- [ ] Use `slnx` instead of `sln` for Visual Studio solution.
	- [ ] Add `ZenithEngine.D3D12` project.
	- [x] Add `ZenithEngine.Vulkan` project.
	- [ ] Add `ZenithEngine.Windowing` project.
	- [ ] Add `ZenithEngine.ShaderCompiler` project.
	- [ ] Add `ZenithEngine.ImGui` project.
	- [ ] Add `ZenithEngine.ImGui.MultipleWindows` project.
	- [ ] Add `ZenithEngine.Material` project.

- ZenithEngine.Common
	- [ ] Add `ComputePipelineDesc` and `ComputePipeline` classes.
	- [ ] Add `RayTracingPipelineDesc` and `RayTracingPipeline` classes.

## Proposed Features
- [ ] Bindless resources.
- [ ] SPIR-V reflection.
- [ ] Graphics API shared resources.