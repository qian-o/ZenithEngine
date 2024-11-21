# Zenith Engine

## Introduction
Zenith Engine is a modern, cross-platform graphics rendering engine written in C#.

## Graphics Backends
| API    | Supported |
| ---    | --------- |
| D3D12  | 🚧 |
| Vulkan | 🚧 |

## UI Framework Support
| Platform     | D3D12 | Vulkan |
| --------     | ----- | ------ |
| WPF          | 🚧 | 🚧 |
| WinUI        | 🚧 | 🚧 |
| Avalonia     | 🚧 | 🚧 |
| MAUI-Android | 🚧 | 🚧 |
| MAUI-iOS     | 🚧 | 🚧 |
| MAUI-MacOS   | 🚧 | 🚧 |
| MAUI-Windows | 🚧 | 🚧 |

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

- ZenithEngine.Vulkan
	- [ ] Complete Vulkan API bindings.

## Proposed Features
- [ ] Bindless resources.
- [ ] SPIR-V reflection.
- [ ] Graphics API shared resources.

## Code Standard
- Prefer using `(static item => item.Property)` Lambda expressions.
- If a class is `internal` and has value type parameters that need to be accessed externally, allow `public Type FieldName`.
- Class members should be in the following order: Fields, Constructors, Properties, Methods.
- Modifiers: public > internal > protected > private, non-static > static.