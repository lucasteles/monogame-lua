﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class Game1 : Game
{
    readonly GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    LuaEngine luaEngine;

    public Game1()
    {
        graphics = new(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        spriteBatch = new(GraphicsDevice);
        luaEngine = new(this, graphics, spriteBatch);
        luaEngine.Initialize();
        base.Initialize();
    }

    protected override void LoadContent() =>
        luaEngine.LoadContent();

    protected override void Update(GameTime gameTime)
    {
        luaEngine.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        luaEngine.Draw(gameTime);
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        luaEngine.Dispose();
        base.UnloadContent();
    }
}