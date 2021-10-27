import ('Microsoft.Xna.Framework')
import ('Microsoft.Xna.Framework.Graphics')
import ('Microsoft.Xna.Framework.Input')
import ('MonoGame.Framework')

local rotation = 0
local position,logoTexture,font

function Initialize()
	game.Window.Title = "Monogame with Lua"
	game.IsMouseVisible = true
	graphics.PreferredBackBufferWidth = 1024
	graphics.PreferredBackBufferHeight = 768
	graphics:ApplyChanges()

	local bounds = game.Window.ClientBounds
	position = Vector2(bounds.Width / 2, bounds.Height / 2)
end


function LoadContent()
	logoTexture = content:LoadTexture2D("logo")
	font = content:LoadSpriteFont("zorque")
end

function MovementDirection(keyboard)
	local baseDirection = Vector2.Zero
	if keyboard:IsKeyDown(Keys.W) then
		baseDirection = baseDirection + Vector2(0,-1)
	end
	if keyboard:IsKeyDown(Keys.S) then
		baseDirection = baseDirection + Vector2(0,1)
	end

	if keyboard:IsKeyDown(Keys.A) then
		baseDirection = baseDirection + Vector2(-1,0)
	end
	if keyboard:IsKeyDown(Keys.D) then
		baseDirection = baseDirection + Vector2(1,0)
	end
	return baseDirection
end

function Update(gameTime)
	local keyboardState = Keyboard.GetState()
	if keyboardState:IsKeyDown(Keys.Escape) then
	    game:Exit()
	end

	rotation = rotation + 0.01
	local velocity = MovementDirection(keyboardState) * 2

	position = position + velocity
end

function Draw(gameTime)
	local logoCenter = Vector2(logoTexture.Bounds.Width / 2,
							   logoTexture.Bounds.Height / 2)

    graphicsDevice:Clear(Color.LightGray)

	spriteBatch:Begin()

	spriteBatch:Draw(logoTexture, position, logoTexture.Bounds,
					 Color.White, rotation, logoCenter, 0.5, -- scale
					 SpriteEffects.None, 0)


	spriteBatch:DrawString(font, "Hello monogame", Vector2(10,5), Color.DarkBlue)
	spriteBatch:End()
end
