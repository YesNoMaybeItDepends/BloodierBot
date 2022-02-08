INSERT or IGNORE INTO 
RunningGames 
(Id, Half, Turn, Division, TournamentId) 
values 
(@Id, @Half, @Turn, @Division, @TournamentId)