using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Content
{
    readonly Game game;

    public Content(Game game) => this.game = game;

    public Texture2D LoadTexture2D(string name) => game.Content.Load<Texture2D>(name);
    public SpriteFont LoadSpriteFont(string name) => game.Content.Load<SpriteFont>(name);
}