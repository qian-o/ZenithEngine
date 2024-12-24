﻿# Zenith Engine

## Introduction
Zenith Engine is a modern, cross-platform graphics rendering engine written in C#.

## Graphics Backends
| API    | Supported |
| ---    | :-------: |
| D3D12  | 🚧 |
| Vulkan | ✅ |

## UI Framework Support
| Platform     | D3D12 | Vulkan |
| --------     | :---: | :----: |
| WPF          | 🚧 | 🚧 |
| WinUI        | 🚧 | 🚧 |
| Avalonia     | ❎ | 🚧 |
| MAUI-Android | ❎ | 🚧 |
| MAUI-iOS     | ❎ | 🚧 |
| MAUI-MacOS   | ❎ | 🚧 |
| MAUI-Windows | 🚧 | 🚧 |

## TODO
- ZenithEngine
	- [ ] Use `slnx` instead of `sln` for Visual Studio solution.
	- [ ] Add `ZenithEngine.D3D12` project.
	- [x] Add `ZenithEngine.Vulkan` project.
	- [x] Add `ZenithEngine.Windowing` project.
	- [x] Add `ZenithEngine.ShaderCompiler` project.
	- [x] Add `ZenithEngine.ImGuiWrapper` project.
	- [ ] Add `ZenithEngine.Material` project.
	- [ ] Add `ZenithEngine.Editor` project.
	- [ ] Add `ZenithEngine.Viewer` project.

- ZenithEngine.Vulkan
	- [ ] Complete Vulkan API bindings.
		- [ ] Improve acceleration structures and ray tracing pipeline.

- ZenithEngine.ShaderCompiler
	- [ ] Use Slang instead of HLSL.

- ZenithEngine.Editor
	- [ ] Use WPF for the editor's UI. 
	- [ ] Add project design and parsing, as well as loading and saving of project files.

- ZenithEngine.Viewer
	- [ ] Use ImGui for the viewer's UI.
	- [ ] Load project files generated by ZenithEngine.Editor.

## Code Standard
- Prefer using `(static item => item.Property)` Lambda expressions.
- Prefer using new() syntax sugar.
- Prefer using pattern matching. Such as `is, is not, =>`.
- If a class is `internal` and has value type parameters that need to be accessed externally, allow `public Type FieldName`.
- Class members should be in the following order: Fields, Constructors, Properties, Methods.
- Modifiers: public > internal > protected > private, non-static > static.
- The remaining modifiers are sorted by function or business logic.

## Regular Expression Format
- `,+[\s]+[^\S\r\n]+};` Search for the comma after the last property when simplifying property assignments.

## Draft
- Do we need to add a `CommandProcessor Processor` property in the `CommandBuffer` so that some parameters can be used when executing commands in the `CommandBuffer`?
- Direct3D 12 does not support automatic mipmap generation.
