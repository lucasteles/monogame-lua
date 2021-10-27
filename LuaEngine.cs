using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    string luaSrc;

    FileSystemWatcher watcher = new();
    Exception currentError;
    bool showedError, reloading, forceReload;

    public LuaEngine(Game game, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        this.game = game;
        this.graphics = graphics;
        this.spriteBatch = spriteBatch;
        luaSrc = Path.Combine(GetProjectPath(), "lua");
        ConfigureWatcher();
    }

    bool ShouldWait() => currentError is not null || reloading;
    void ConfigureWatcher()
    {
        watcher.Filter = "*.*";
        watcher.Path = luaSrc;
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Created += WatcherHandler;
        watcher.Deleted += WatcherHandler;
        watcher.Renamed += WatcherHandler;
        watcher.Changed += WatcherHandler;
    }

    void WatcherHandler(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("RELOADING...");
        forceReload = true;
        currentError = null;
        showedError = false;
    }

    public void Initialize(bool fromwatch = false)
    {
        if (reloading) return;
        reloading = true;
        try
        {
            lua?.Dispose();
            lua = new();
            lua.LoadCLRPackage();
            lua["graphicsDevice"] = game.GraphicsDevice;
            lua["graphics"] = graphics;
            lua["spriteBatch"] = spriteBatch;
            lua["content"] = new Content(game);
            lua["game"] = game;
            //lua.DoFile("lua/main.lua");
            lua.DoFile($"{luaSrc}/main.lua");
            luaInitialize = lua["Initialize"] as LuaFunction;
            luaLoadContent = lua["LoadContent"] as LuaFunction;
            luaUpdate = lua["Update"] as LuaFunction;
            luaDraw = lua["Draw"] as LuaFunction;
            luaInitialize?.Call();
            if (fromwatch) luaLoadContent?.Call();
        }
        catch (Exception e)
        {
            currentError = e;
        }
        finally
        {
            reloading = false;
        }
    }

    public void LoadContent()
    {
        errorFont = game.Content.Load<SpriteFont>("arialfont");
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
        if (forceReload)
        {
            forceReload = false;
            Initialize(true);
            return;
        }

        if (ShouldWait()) return;
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
        if (currentError is not null) DrawErrorScreen();
        if (ShouldWait()) return;

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
        var exn = currentError;
        if (exn is null)
            return;

        game.GraphicsDevice.Clear(Color.Black);

        var exStr = $"{exn}\nInnerException:{exn.InnerException}";
        var error =
            string.Join("\n",
                exStr.Select((c, index) => new {c, index})
                    .GroupBy(x => x.index/100)
                    .Select(group => group.Select(elem => elem.c))
                    .Select(chars => new string(chars.ToArray())));

        if (!showedError)
        {
            Console.WriteLine(error);
            showedError = true;
        }
        spriteBatch.Begin();
        spriteBatch.DrawString(errorFont, error, Vector2.Zero, Color.White);
        spriteBatch.End();
    }

    string GetProjectPath(string path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            path = Directory.GetCurrentDirectory();

        return Directory.GetFiles(path, "*.csproj").Any()
            ? path
            : GetProjectPath(Directory.GetParent(path)?.FullName);
    }

    public void Dispose()
    {
        watcher.Created -= WatcherHandler;
        watcher.Deleted -= WatcherHandler;
        watcher.Changed -= WatcherHandler;
        watcher.Renamed -= WatcherHandler;
        lua?.Dispose();
    }
}