# Description

This is a .NET 8.0 PInvoke wrapper for **Box2d v3.x** for .NET games that have their own engine. The wrapper is generated using a homebrewn codegen tool that parses the Box2D C code and generates the C# wrapper.

The codegen also brings in the C comments, so code completion is quite rich (thank Erin for the quality doc). You may also *goto definition* because there is more comment than what code completion picks up.

Next to the codegen output, I added a few reusable helpers like wiring the new multithreading support of Box2d to .NET Task Parallel Library.

## DISCLAIMER

> This wrapper is very young and largely untested. I'm doing this for my own Monogame game but it turned out pretty well, so I'm sharing it. But there may come breaking changes when i fix bugs or find better ways of doing things, especially in the C# helper code.

> This will probably not work in Unity, it's meant for stand-alone use in .NET code.

# Naming conventions

I don't like wrappers that need their own manual, so Box2DNet uses the exact same naming and types as the C original. The Box2D manual is therefore 99% applicable to Box2dNet too. 

All Box2D API functions (marked ```B2API``` in the C code) map to C# methods with the same name in static class ```Box2dNet.Interop.B2Api```. So, B2Api is the place to be. 
Some .NET helper-code is in the other files as partial classes and extention methods.

# What's included?

## B2API Functions

Most of the ```B2API``` marked functions are there, along with the types they require. 

NOT included:

* the timer functions (b2CreateTimer, ..): use .NET timers :)
* b2World_Draw and b2DefaultDebugDraw: temporary, due to translation difficulties. I intend to manually port this code, as it will probably not change much in the future.
* b2DynamicTree_X: these were hard to wrap automatically. But most games don't need to use this directly, I think.

## Multi-threading support

There is .NET TPL wiring code for the new multi-threaded Task system that Box2D uses. See ```B2Api.b2DefaultWorldDef_WithDotNetTpl()``` or the example on parallellism.

# Usage

## Include the project in your game

There's no nuget package. Just copy the ```Box2DNet``` project into your game solution, or refer to the csproj.

> When you build the ```Box2DNet``` project along with your game in DEBUG it will use the debug ```box2dd.dll``` which will quit your game when a C assertion fails: this helps for debugging. Build your game in RELEASE to use the ```box2d.dll``` production version.

## Samples

Check out the ```Box2dNet.Samples``` console app to see working code that gets you started.

# Regenerating the wrapper

Currently, I regenerate every few weeks, as Erin pushes changes. The corresponding Win x64 DLLs (debug and release) are included in this repo too, so you won't have to regenerate yourself. 

## Regenerating yourself:

If you still want to do it yourself: the C# wrapper code can be regenerated with the companion tool ```Box2dWrap```, also in this repo. 
It's a naive C parsing + codegen tool, very specific to the Box2D codebase. It was meant for my eyes only, so it's not the easiest code to find your way in. You are warned :)

It expects commandline parameters: 

```Box2dWrap.exe <box2d-repo-root> <B2Api.cs-file-output>``` 

Example:

```Box2dWrap.exe C:\repos\box2d "C:\repos\Box2dNet\Box2dNet\Interop\B2Api.cs"```

## Building the Box2D DLLs

Steps to rebuild Box2D dlls (from the ```erincatto/box2d``` code):

* get latest version
* in CMakeLists.txt around line 11 add a line ```option(BUILD_SHARED_LIBS "Build using shared libraries" ON)``` which makes it build to dll instead of statically linked lib
* run ```build.cmd``` -> generates a .sln in ```./build```
* if it did not open automatically, open the generated ```./build/box2d.sln``` in Visual Studio
* Rebuild the ```box2d``` project both in Debug and Release to get both .dlls ```box2dd.dll``` and ```box2d.dll```. (don't build the entire sln, it gives errors in the non-static build, atm)
* copy the 2 dlls to the Box2dNet project and set to copy on build (currently I use the csproj directly in my game's .sln)
* rerun Box2DWrap to regenerate B2Api C# code with ```./Interop/B2Api.cs``` in the Box2dNet project as output.

The generator gives several  warnings about things on the "exclude-list", which is because I deliberately exclude a few things (see the NOT included list above). 
If you get other warnings or errors, though, some new changes in the C code are incompatible with my tool. You can let me know, and I will try to fix them.

History:

16/11/2024 built from Box2D commit a9f2c92f7af8f5a8e2c9b371044e15c1f8959c45
1/11/2024 built from Box2D commit 87e13e44378afc42598a4f7e8b2d5289982cdda7
1/10/2024 built from Box2D commit df7373c08a41b7a4ba6edd5d4be200675a948176
