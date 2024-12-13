# Zenith Engine

## Introduction
Zenith Engine is a modern, cross-platform graphics rendering engine written in C#.

## Graphics Backends
| API    | Supported |
| ---    | --------- |
| D3D12  | 🚧 |
| Vulkan | ✅ |

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
	- [x] Add `ZenithEngine.Windowing` project.
	- [x] Add `ZenithEngine.ShaderCompiler` project.
	- [x] Add `ZenithEngine.ImGuiRender` project.
	- [ ] Add `ZenithEngine.ImGuiRender.MultipleWindows` project.
	- [ ] Add `ZenithEngine.Material` project.
	- [ ] Add `ZenithEngine.Editor` project.

- ZenithEngine.Vulkan
	- [ ] Complete Vulkan API bindings.
		- [ ] Improve acceleration structures and ray tracing pipeline.

- ZenithEngine.ShaderCompiler
	- [ ] Use Slang instead of HLSL.

- ZenithEngine.Editor
	- [ ] Use ImGui for the editor's UI.
	- [ ] Add project design and parsing, as well as loading and saving of project files.

## Code Standard
- Prefer using `(static item => item.Property)` Lambda expressions.
- If a class is `internal` and has value type parameters that need to be accessed externally, allow `public Type FieldName`.
- Class members should be in the following order: Fields, Constructors, Properties, Methods.
- Modifiers: public > internal > protected > private, non-static > static.
- The remaining modifiers are sorted by function or business logic.

## Regular Expression Format
- `,+[\s]+[^\S\r\n]+};` Search for the comma after the last property when simplifying property assignments.