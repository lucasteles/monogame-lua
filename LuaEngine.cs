using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLua;

public class LuaEngine : IDisposable
{
    readonly Game game;
    readonly GraphicsDeviceManager graphics;
    readonly SpriteBatch spriteBatch;

    SpriteFont errorFont;
    Lua lua;
    LuaFunction luaInitialize, luaLoadContent, luaUpdate, luaDraw;

    Exception currentError = null;

    public LuaEngine(Game game, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        this.game = game;
        this.graphics = graphics;
        this.spriteBatch = spriteBatch;
    }

    public void Initialize()
    {
        try
        {
            lua = new();
            lua.LoadCLRPackage();
            lua["graphicsDevice"] = game.GraphicsDevice;
            lua["graphics"] = graphics;
            lua["spriteBatch"] = spriteBatch;
            lua["content"] = new Content(game);
            lua["game"] = game;
            lua.DoFile("lua/main.lua");
            luaInitialize = lua["Initialize"] as LuaFunction;
            luaLoadContent = lua["LoadContent"] as LuaFunction;
            luaUpdate = lua["Update"] as LuaFunction;
            luaDraw = lua["Draw"] as LuaFunction;
            luaInitialize?.Call();
        }
        catch (Exception e)
        {
            currentError = e;
            throw;
        }
    }

    public void LoadContent()
    {
        errorFont = game.Content.Load<SpriteFont>("errorfont");
        try
        {
            luaLoadContent?.Call();
        }
        catch (Exception e)
        {
            currentError = e;
        }
    }

    public void Update(GameTime gameTime)
    {
        if (currentError is not null) return;
        try
        {
            luaUpdate?.Call(gameTime);
        }
        catch (Exception e)
        {
            currentError = e;
        }
    }

    public void Draw(GameTime gameTime)
    {
        if (currentError is not null)
        {
            DrawErrorScreen();
            return;
        }

        try
        {
            luaDraw?.Call(gameTime);
        }
        catch (Exception e)
        {
            currentError = e;
        }
    }

    void DrawErrorScreen()
    {
        if (currentError is null)
            return;
        Console.WriteLine(currentError);
        game.GraphicsDevice.Clear(Color.Black);
        var error = $"ERROR:{currentError.Message}";
        spriteBatch.Begin();
        spriteBatch.DrawString(errorFont, error, Vector2.Zero, Color.White);
        spriteBatch.End();
    }

    public void Dispose()
    {

    }
}