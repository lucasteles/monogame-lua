using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLua;

sealed class LuaEngine : IDisposable
{
    readonly Game game;
    readonly GraphicsDeviceManager graphics;
    readonly SpriteBatch spriteBatch;
    readonly FileSystemWatcher watcher = new();
    readonly string luaSrc;

    SpriteFont errorFont;
    Lua lua;
    LuaFunction luaInitialize, luaLoadContent, luaUpdate, luaDraw;

    Exception currentError;
    bool showedError, forceReload;

    public LuaEngine(Game game, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
    {
        this.game = game;
        this.graphics = graphics;
        this.spriteBatch = spriteBatch;
        luaSrc = Path.Combine(GetProjectPath(), "lua");
        ConfigureWatcher();
    }

    bool ShouldWait() => currentError is not null;

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
        if (e.Name is null || File.GetAttributes(e.FullPath.TrimEnd('~'))
                .HasFlag(FileAttributes.Directory))
            return;

        forceReload = true;
    }

    public void Initialize()
    {
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

            lua.DoFile($"{luaSrc}/main.lua");
            luaInitialize = lua["Initialize"] as LuaFunction;
            luaLoadContent = lua["LoadContent"] as LuaFunction;
            luaUpdate = lua["Update"] as LuaFunction;
            luaDraw = lua["Draw"] as LuaFunction;
            luaInitialize?.Call();
        }
        catch (Exception e)
        {
            currentError = e;
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
            Console.WriteLine("Reloding...");
            currentError = null;
            showedError = forceReload = false;
            Initialize();
            luaLoadContent?.Call();
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
        if (!showedError)
        {
            Console.WriteLine(exn);
            showedError = true;
        }

        var error =
            string.Join("\n",
                exStr.Select((c, index) => new {c, index})
                    .GroupBy(x => x.index / 100)
                    .Select(group => group.Select(elem => elem.c))
                    .Select(chars => new string(chars.ToArray())));

        spriteBatch.Begin();
        spriteBatch.DrawString(errorFont, error, Vector2.Zero, Color.White);
        spriteBatch.End();
    }

    static string GetProjectPath(string path = null)
    {
        while (true)
        {
            if (string.IsNullOrWhiteSpace(path)) path = Directory.GetCurrentDirectory();

            if (Directory.GetFiles(path, "*.csproj").Any()) return path;
            path = Directory.GetParent(path)?.FullName;
        }
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