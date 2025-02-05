﻿# Zenith Engine

## Introduction
Zenith Engine is a modern, cross-platform graphics rendering engine written in C#.

## Graphics Backends
| API       | Supported |
| :-:       | :-------: |
| DirectX12 | ✅ |
| Vulkan    | ✅ |

## TODO
- ZenithEngine
	- [ ] Use `slnx` instead of `sln` for Visual Studio solution.
	- [x] Add `ZenithEngine.DirectX12` project.
	- [x] Add `ZenithEngine.Vulkan` project.
	- [x] Add `ZenithEngine.Windowing` project.
	- [x] Add `ZenithEngine.ShaderCompiler` project.
	- [x] Add `ZenithEngine.ImGuiWrapper` project.
	- [ ] Add `ZenithEngine.Material` project.
	- [ ] Add `ZenithEngine.Editor` project.
	- [ ] Add `ZenithEngine.Viewer` project.

- ZenithEngine.Common
	- [x] Add `GenerateMipmaps` method in Utils.
	- [ ] Add Vertex and Mesh structures for the material system.

- ZenithEngine.DirectX12
	- [ ] Add Compute and Ray Tracing support.

- ZenithEngine.ShaderCompiler
	- [ ] Add DXIL compilation support.
	- [ ] Use DXC reflection instead of SPIRV.Reflect.
	- [ ] Use Slang instead of HLSL.

- ZenithEngine.ImGuiWrapper
	- [ ] Add DPI scaling support.

- ZenithEngine.Windowing
	- [ ] Use SDL3 instead of SDL2.
	- [ ] Add DPI Changed event.

- ZenithEngine.Editor
	- [ ] Use WPF for the editor's UI. 
	- [ ] Add project design and parsing, as well as loading and saving of project files.
	- [ ] Design Redo and Undo features.

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
- All structs that involve the static method `Default` should be changed to `Create` or `New`.