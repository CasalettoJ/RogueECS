?Engine Things:

Game Loop 
	-> Construct
	-> Initialize
	-> SetState
	-> Update
		-> State -> Update
		-> Handle State Stack
	-> Draw
		-> State -> Draw
	-> Destruct
______________________________________________________________________
	->State
		-> Construct
			-> SetNextLevel
			-> Load State Data From File or Set Default Values
		-> Update
			-> Level - > Update
		-> Draw
			-> Level -> Draw
		-> SetNextLevel
			-> Content.Unload
			-> Level = Next Level
			-> Next Level.LoadContent
		-> Destruct	
______________________________________________________________________	
______________________________________________________________________
		-> Level
			-> Construct
			-> LoadContent
				-> Load Level Data From File or Set Default Values
			-> Update
			-> Draw
			-> Destruct
		
		

Level update returns the next level of the state, if it returns null the next state will be null.
State update returns the next state of the game.  If there is no next level it will return null.

When a null state is returned to the game loop the state stack is popped and the next state is loaded.

Possible States:
-Title
-PlayingGame
-Paused
-Menu