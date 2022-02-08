INSERT or IGNORE INTO 
RunningGameTeams 
(Id, RunningGameId, Side, Name, Coach, Race, Tv, Rating, Score, Logo, LogoLarge)
values 
(@Id, @RunningGameId, @Side, @Name, @Coach, @Race, @Tv, @Rating, @Score, @Logo, @LogoLarge)