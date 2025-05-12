# Zenith Engine

## Introduction
Zenith Engine is a modern, cross-platform graphics rendering engine written in C#.

## Graphics Backends
| API       | Supported |
| :-:       | :-------: |
| DirectX12 | ✅ |
| Vulkan    | ✅ |

## TODO
- ZenithEngine
	- [x] Add `ZenithEngine.DirectX12` project.
	- [x] Add `ZenithEngine.Vulkan` project.
	- [x] Add `ZenithEngine.Windowing` project.
	- [x] Add `ZenithEngine.ShaderCompiler` project.
	- [x] Add `ZenithEngine.ImGuiWrapper` project.
	- [ ] Add `ZenithEngine.Material` project.
	- [ ] Add `ZenithEngine.Editor` project.

- ZenithEngine.Common
	- [ ] Add Vertex and Mesh structures for the material system.

- ZenithEngine.ImGuiWrapper
	- [ ] Add DPI scaling support.

- ZenithEngine.Windowing
	- [ ] Use SDL3 instead of SDL2.
	- [ ] Add DPI Changed event.

- ZenithEngine.Editor
	- [ ] Use WinUI3 for the editor's UI. 
	- [ ] Add project design and parsing, as well as loading and saving of project files.
	- [ ] Design Redo and Undo features.
	- [ ] Try to minimize dependencies on external libraries, and use CommunityToolkit controls.

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
- Delete the `Triangle` project and change it to the `Rasterization` project.
- `Rasterization` and `RayTracing` use the Cornell Box model.
- After the release of Silk.NET 3.0, refactor all interface calls and use the unsafe method.