# Box2DNet - .NET wrapper for 'box2d v3.x'

Latest regen from [Box2d v3](https://github.com/erincatto/box2d): **2024/12/22**

## Description

This is a .NET 8.0 PInvoke wrapper for [Box2d v3.x](https://github.com/erincatto/box2d) for .NET games that have their own engine. 

A custom tool parses the Box2D C code and generates the C# wrapper code.

Next to the generated wrapper, a few reusable helpers are manually added to ease working with native pointers and wiring the new multithreading support of Box2d to .NET Task Parallel Library.

> This is not a wrapper with a fully tested API. I'm doing this for my own game but due to the automatic generation, it turned out pretty complete, so I'm sharing it.

> I don't use Unity and therefore cannot support it. This wrapper is meant for stand-alone use in .NET code, for instance with Monogame.

### License

You may do whatever you like with the code in this repo. Don't forget to respect the [Box2d v3.x](https://github.com/erincatto/box2d) license, though!

## Manual

Box2DNet uses no abstraction layer: you work directly with functions and types directly mapped to the original C definitions with original identifiers. You can therefore use the original [Box2d manual](https://box2d.org/documentation/) 'as is'.

### Include Box2DNet in your game

There's no nuget package. Just clone ```Box2DNet``` next to your game folder and include the ```.csproj``` into your game solution.

This will build the Box2DNet project along with your game. When you build in DEBUG it will use the native debug dll ```box2dd.dll```, when you build in RELEASE it will use the native production dll ```box2d.dll```.

> The debug version ```box2dd.dll``` will quit your game with assertion errors when you did something wrong: this helps for debugging your mistakes.

### The Box2D API

All Box2D functions are available as C# methods in static class ```Box2dNet.Interop.B2Api```. Original comments are also available, so code completion is quite rich.

NOT included:

* the timer functions (b2CreateTimer, ..): use .NET timers :)
* b2World_Draw and b2DefaultDebugDraw: hard to support this code in the codegen tool. I intend to manually port this code some day, as it will probably not change much in the future.
* b2DynamicTree_X: also hard to support these in the codegen tool. But most games don't need to use this directly.

### Dealing with IntPtr

A lot of API functions use pointers as parameters, or in structs. In C# these become ```IntPtr```, so you cannot see which `struct` they pointed to in C. To help with this, the struct name is added in comment in the generated C# code, so you can use `go to definition` on these to find out.

Example:

``` C#
/// <summary>
/// Array of sensor begin touch events
/// </summary>
public IntPtr /* b2SensorBeginTouchEvent* */ beginEvents;
```

> If the IntPtr is an array, like in the example, you can use the provided C# method ```NativeArrayAsSpan``` to loop over the contents. 

Like this:

``` C#
var hitEvents = B2Api.b2World_GetContactEvents(b2WorldId);
// use helper extension method to efficiently read the native hitEvents array with little code:
foreach (var @event in hitEvents.beginEvents.NativeArrayAsSpan<b2ContactBeginTouchEvent>(hitEvents.beginCount))
{
    Console.WriteLine($"!!!!!!!   HIT detected between {@event.shapeIdA} and {@event.shapeIdB}");
}
```

If you want to pass a game object reference into Box2D, like `userData`, you must create a `NativeHandle` for your game object on the .NET side and pass the `IntPtr` to your .NET object to Box2D. Example:

``` C#
_handle = NativeHandle.Alloc(ball); // allocate a IntPtr handle for the .NET object and return it as IntPtr.

var shapeDef = B2Api.b2DefaultShapeDef();
// now tag the Box2d Shape with a handle to our .NET game object so we can always find the .NET game object back:
shapeDef.userData = _handle;
```

After this, when Box2D passes the IntPtr back to you somewhere, you can get the corresponding .NET game object back like this:

``` C#
var userDataIntPtr = B2Api.b2Shape_GetUserData(@event.shapeIdA);
var ball = NativeHandle<Ball>.GetObjectFromIntPtr(userDataIntPtr);
```

Note that you must keep the NativeHandle alive as long as you as you need the native Box2D to keep hold of the IntPtr to it.
When you're fully done with it, you must not forget to `NativeHandle.Free(_handle)`. For instance, when you remove the game object from your game.

### Multi-threading support

Box2dNet comes with .NET integration for the new multi-threaded task system that Box2D uses. 

Simply use `B2Api.b2DefaultWorldDef_WithDotNetTpl()` instead of `B2Api.b2DefaultWorldDef` to create your Box2D world:

``` C#
var worldDef = useMultiThreading
    ? B2Api.b2DefaultWorldDef_WithDotNetTpl() // <---- this is all it takes for default multi threading
    : B2Api.b2DefaultWorldDef();

var b2WorldId = B2Api.b2CreateWorld(worldDef);
```

> the TPL in `b2DefaultWorldDef_WithDotNetTpl` stands for Task Parallel Library, which is the name of the .NET Task framework.

### Samples

Check out the ```Box2dNet.Samples``` console app to see working code that gets you started on common usecases like detecting collisions.

## Regenerating the wrapper

Currently, I regenerate every few weeks. The corresponding Win x64 DLLs (debug and release) are included in this repo too, so generally, you won't have to regenerate yourself. 

But if you must:

* regenerate the existing `B2Api.cs` with the `Box2dWrap` tool.
* rebuild and replace the existing two native dlls in the Box2dNet project

### Regenerate the C# code

The C# wrapper code can be regenerated with the companion codegen tool ```Box2dWrap```, also in this repo. 
It's a naive C parsing + codegen tool, very specific to the Box2D codebase. It was meant for my eyes only, so it's not the easiest code to find your way in. You are warned :)

It expects commandline parameters: 

`Box2dWrap.exe <box2d-repo-root> <B2Api.cs-file-output>`

Example:

`Box2dWrap.exe C:\repos\box2d "C:\repos\Box2dNet\Box2dNet\Interop\B2Api.cs"`

> Make sure the second parameter points to the existing `B2Api.cs` file in your local Box2dNet repo so it gets overwritten.

The generator gives several warnings about the "exclude-list", which is deliberate, and therefore to be ignored.
If you get other warnings or errors, though, some of the new C code is incompatible with my tool. You can let me know if I'm not working on it already, or you can give it a shot yourself and send me a PR.

### Rebuilding the Box2D DLLs

(make sure you have `cmake` installed, (use the .msi from https://cmake.org/download))

* Clone the latest version of ```erincatto/box2d``` onto your PC
* in ```CMakeLists.txt``` around line 11 add a line ```option(BUILD_SHARED_LIBS "Build using shared libraries" ON)``` which makes it build to dll instead of statically linked lib
* run ```build.cmd``` -> generates a .sln in ```./build```
* if it did not open automatically, open the generated ```./build/box2d.sln``` in Visual Studio
* **Re**build the ```box2d``` project both in Debug and Release to get both debug dll ```box2dd.dll``` and production dll ```box2d.dll```. (no need to build the entire sln, i have seen the other projects give errors even)
* copy the 2 dlls from ```.\build\bin\Debug``` and ```.\build\bin\Release``` to the Box2dNet project and set to *copy on build*



