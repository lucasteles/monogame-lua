using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Content
{
    readonly GraphicsDevice device;
    readonly Game game;

    public Content(Game game, GraphicsDevice device) => (this.game, this.device) = (game, device);

    public Texture2D LoadTexture2D(string name) => game.Content.Load<Texture2D>(name);
    public SpriteFont LoadSpriteFont(string name) => game.Content.Load<SpriteFont>(name);
}