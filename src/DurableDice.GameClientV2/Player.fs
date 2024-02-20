module Player

open Theme

type Player =
    { Index: int
      Id: string
      Name: string
      DiceBuffer: int
      DiceMovesThisTurn: int
      ContinuousFieldCount: int
      IsReady: bool
      BotType: int option }

module Player = 
    let isPlayer (playerId) (player) = player.Id = playerId
    let color (player) =
        Theme.color (Some player.Index)
    
