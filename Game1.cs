using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLua;

public class Game1 : Game
{
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    readonly Lua lua = new();
    LuaFunction luaInitialize, luaLoadContent, luaUpdate, luaDraw;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        spriteBatch = new(GraphicsDevice);
        lua.LoadCLRPackage();
        lua["graphicsDevice"] = GraphicsDevice;
        lua["graphics"] = graphics;
        lua["content"] = new Content(this, GraphicsDevice);
        lua["game"] = this;
        lua.DoFile("lua/main.lua");
        luaInitialize = lua["Initialize"] as LuaFunction;
        luaLoadContent = lua["LoadContent"] as LuaFunction;
        luaUpdate = lua["Update"] as LuaFunction;
        luaDraw = lua["Draw"] as LuaFunction;
        luaInitialize?.Call();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new (GraphicsDevice);
        lua["spriteBatch"] = spriteBatch;
        luaLoadContent?.Call();
    }

    protected override void Update(GameTime gameTime)
    {
        luaUpdate?.Call(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        luaDraw?.Call(gameTime);
        base.Draw(gameTime);
    }
}