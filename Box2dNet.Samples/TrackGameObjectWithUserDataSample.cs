﻿using Box2dNet.Interop;
using System.Numerics;

namespace Box2dNet.Samples
{
    internal class Ball : IDisposable
    {
        public string Name { get; }

        
        private readonly NativeHandle<Ball> _handle; 
        private readonly b2BodyId _b2BodyId;
        private readonly b2ShapeId _b2ShapeId;

        public Ball(string name, b2WorldId b2WorldId, Vector2 position, Vector2 velocity)
        {
            Name = name;
            var bodyDef = B2Api.b2DefaultBodyDef();
            bodyDef.type = b2BodyType.b2_dynamicBody;
            bodyDef.position = position;
            bodyDef.linearVelocity = velocity;
            _b2BodyId = B2Api.b2CreateBody(b2WorldId, bodyDef);

            var shapeDef = B2Api.b2DefaultShapeDef();

            // now tag the Box2d Shape with a handle to our .NET game object so we can always find the .NET game object back:
            _handle = new NativeHandle<Ball>(this);
            shapeDef.userData = _handle.IntPtr;

            var circle = new b2Circle { radius = 0.5f };
            _b2ShapeId = B2Api.b2CreateCircleShape(_b2BodyId, in shapeDef, in circle);
        }

        public Vector2 Position => B2Api.b2Body_GetPosition(_b2BodyId);
        public Vector2 Velocity => B2Api.b2Body_GetLinearVelocity(_b2BodyId);

        public void Dispose()
        {
            // It's a good idea in a game engine to have a baseclass representing the b2shapeid that stores and frees the b2shapeid and NativeHandle.
            // This way you never forget to free them up when you inherit from that baseclass.

            B2Api.b2DestroyShape(_b2ShapeId, false);
            B2Api.b2DestroyBody(_b2BodyId);
            _handle.Dispose();
        }
    }

    internal class TrackGameObjectWithUserDataSample : ISample
    {
        public void Run()
        {
            Console.WriteLine("This sample stores a managed object (eg. your .NET game object) in the Box2D Shape, and then finds the managed object back in events received from Box2D.");

            // create world
            var worldDef = B2Api.b2DefaultWorldDef();
            worldDef.gravity = new(0, 0);
            var b2WorldId = B2Api.b2CreateWorld(worldDef);

            try
            {
                using var ballA = new Ball("Ball A", b2WorldId, new(-2, 0), new(5, 0)); // Don't forget to dispose!! 
                using var ballB = new Ball("Ball B", b2WorldId, new(2, 0), new(0, 0));

                // simulate world and detect collisions:
                for (var i = 0; i < 40; i++)
                {
                    B2Api.b2World_Step(b2WorldId, 1 / 60f, 4);

                    Console.WriteLine($"Step {i}: ball A position = {ballA.Position}, velocity = {ballA.Velocity}");

                    var hitEvents = B2Api.b2World_GetContactEvents(b2WorldId);
                    // use helper extension method to efficiently read the native hitEvents array with little code:
                    foreach (var @event in hitEvents.beginEvents.NativeArrayAsSpan<b2ContactBeginTouchEvent>(hitEvents.beginCount))
                    {
                        // here, we now can find our .NET Ball back because it was stored onto the B2Shape.UserData
                        var ball1 = NativeHandle<Ball>.GetObjectFromIntPtr(B2Api.b2Shape_GetUserData(@event.shapeIdA));
                        var ball2 = NativeHandle<Ball>.GetObjectFromIntPtr(B2Api.b2Shape_GetUserData(@event.shapeIdB));

                        Console.WriteLine($"!!!!!!  HIT detected between .NET Ball '{ball1.Name}' and .NET Ball '{ball2.Name}'.");
                    }
                }
            }
            finally
            {
                B2Api.b2DestroyWorld(b2WorldId);
            }
        }
    }
}